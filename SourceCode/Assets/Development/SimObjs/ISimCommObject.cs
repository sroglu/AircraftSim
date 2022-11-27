using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ISimCommObject:IDisposable
{
    bool Registered();
    SimObjData GetSimObjData();
    SimObjTransform GetObjTransform();
    SimObjInputData GetSimObjInputData();
    void SetSimObjData(SimObjData simObjData);
    void SetCommunication(SimObjCommunication comm,int instanceID);
}

public abstract class SimCommObject : SimRelativeObject, ISimCommObject
{

    #region TransformFunctions
    //Place All Object to lat0 lon0 on earth
    public sealed override Quaternion GetBaseRotation() { return Utility.CreateBaseRotationByCoords(GetObjTransform()); } 
    public sealed override Quaternion GetRotation() { return Utility.CreateLocalRotationByCoords(GetObjTransform()); }
    public sealed override Vector3 GetEarthPos() { return CreateEarthPositionByCoords(GetObjTransform()); }
    #endregion

    static Dictionary<SimObjCommunication, List<SimCommObject>> communications = new Dictionary<SimObjCommunication, List<SimCommObject>>();

    #region EditorInputOutputs
    [SerializeField]
    protected int index = int.MaxValue;
    [SerializeField]
    protected GameObject[] attachmentSlots;
    [SerializeField]
    protected GameObject attachedTo;
    [SerializeField]
    protected SimObjData simObjData;
    [SerializeField]
    protected SimObjInputData inputData;

    #endregion

    #region PublicValues
    public int IndexInRcv
    {
        get
        {
            return index;
        }
        protected set { index = value; }
    }
    //public Action OnExploded;
    public Action<SimObjCommunication.InstanceStatus> OnStatusChanged;
    public Action<EventType,bool> OnEventTriggered;
    public enum EventType
    {
        chaff,
        flare,
        jammer,
        missile
    }
    #endregion

    #region Accesors
    public SimObjCommunication.InstanceStatus CurrentStatus { get; private set; }
    public bool IsAttached { get { return attachedTo != null; } }
    //public bool IsAlive { get; private set; }
    public bool HasCommunication { get { return hasCommunication; } }

    #endregion


    SimObjCommunication simObjCommunication;
    bool hasCommunication = false;
    SimObjInputData emptyInputData = new SimObjInputData();

    protected const int maxNumberOfDataBuffer = 10;
    protected Queue<SimObjData> simObjDataBuffer = new Queue<SimObjData>(maxNumberOfDataBuffer+1);

    public bool Registered()
    {
        if (GameManager.Instance == null)
            return false;
        return GameManager.Instance.IsRegistered(this);
    }
    public SimObjData GetSimObjData()
    {
        return simObjData;
    }

    //public abstract SimObjTransform GetObjTransform();
    public SimObjTransform GetObjTransform()
    {
        return simObjData.transform;
    }

    public void SetSimObjData(SimObjData simObjData)
    {
        this.simObjData = simObjData;
    }
    public SimObjData CurrentAircraftData { get { return simObjData; } }

    protected override void Init()
    {
        base.Init();
        OnStatusChanged += StatusChanged;
        OnEventTriggered += EventTriggered;

        //Status = status.none;
        foreach (var attachmentSlot in attachmentSlots)
        {
            attachmentSlot.SetActive(false);
        }
    }

    public void SetCommunication(SimObjCommunication simObjCommunication, int index)
    {
        IndexInRcv = index;

        this.simObjCommunication = simObjCommunication;

        if (!communications.ContainsKey(simObjCommunication))
        {
            communications.Add(simObjCommunication, new List<SimCommObject>());
        }
        communications[simObjCommunication].Add(this);

        simObjData = simObjCommunication.GetSimObjData(IndexInRcv);

        GameManager.Instance.RegisterSimObject(this);

        SetAttachments();

        hasCommunication = true;
    }

    public void SetAttachments()
    {
        if (simObjData.attachedInstanceID != simObjData.instanceID)
            GameManager.Instance.GetSimObjByInstanceID(simObjData.attachedInstanceID).AttachSimObject(this);
    }


    //float simTimer = 0f;
    //float simTimePeriod = 0f;
    protected override void Updated()
    {
        base.Updated();
        if (!HasCommunication) return;

        if (IndexInRcv != int.MaxValue)
        {
            if (simObjCommunication.GetSimObjData(IndexInRcv) != simObjData)
            {
                simObjData = simObjCommunication.GetSimObjData(IndexInRcv);
                SimDataUpdated();
                //simTimePeriod = simTimer;
                //simTimer = 0;
            }
        }
        //simTimer+=Time.deltaTime;
    }
    protected void EndComm()
    {
        communications[simObjCommunication].Remove(this);
        simObjCommunication = null;
        hasCommunication = false;
    }

    protected void SendInputData()
    {
        inputData = GetSimObjInputData();
        inputData.instanceID = simObjData.instanceID;
        simObjCommunication.SendData(inputData);
        InputDataSent();
    }

    protected override void AfterTransformSet()
    {
        base.AfterTransformSet();
        if (IsAttached)
        {
            float cohesion = GetAttachObjectCohesion();
            transform.position = Vector3.Lerp(transform.position, attachedTo.transform.position, cohesion);
            //Rotation is seperated into to transform
            //transform.rotation = Quaternion.Slerp(transform.rotation, attachedTo.transform.rotation, cohesion);
        }
    }

    void CheckStatus()
    {
        if(CurrentStatus!= simObjData.status)
        {
            CurrentStatus = simObjData.status;
            OnStatusChanged?.Invoke(simObjData.status);
        }
        CurrentStatus= simObjData.status;
    }
    void CheckEvents()
    {
        if (simObjDataBuffer.Count > 0)
        {
            if (simObjData.chaffTrigger == 1 && simObjDataBuffer.Last().chaffTrigger != simObjData.chaffTrigger)
            {
                OnEventTriggered?.Invoke(EventType.chaff, true);
            }
            if (simObjData.chaffOnFlight == 0 && simObjDataBuffer.Last().chaffOnFlight != simObjData.chaffOnFlight)
            {
                OnEventTriggered?.Invoke(EventType.chaff, false);
            }

            if (simObjData.flareTrigger == 1 && simObjDataBuffer.Last().flareTrigger != simObjData.flareTrigger)
            {
                OnEventTriggered?.Invoke(EventType.flare, true);
            }
            if (simObjData.flareOnFlight == 0 && simObjDataBuffer.Last().flareOnFlight != simObjData.flareOnFlight)
            {
                OnEventTriggered?.Invoke(EventType.flare, false);
            }

        }
    }

    void UpdateDataBuffer()
    {
        if (!simObjDataBuffer.Contains(simObjData))
        {
            simObjDataBuffer.Enqueue(simObjData);
            if (simObjDataBuffer.Count > maxNumberOfDataBuffer)
            {
                simObjDataBuffer.Dequeue();
            }
        }
    }

    protected override void FixedUpdated()
    {
        base.FixedUpdated();

        if (!HasCommunication) return;

        if (!SimObjCommunication.IsRcvDataValid(ref simObjData)) return;

        //CheckAlive();
        //CheckIsStationary();
        CheckStatus();

        CheckEvents();

        SendInputData();

        UpdateDataBuffer();

    }


    #region PublicMethods
    public bool AttachSimObject(SimCommObject attachObj)
    {
        foreach (var attachmentSlot in attachmentSlots)
        {
            if (!attachmentSlot.gameObject.activeInHierarchy)
            {
                attachObj.transform.parent= attachmentSlot.transform;
                attachObj.transform.localRotation = Quaternion.identity;
                attachObj.transform.localPosition = Vector3.zero;
                attachObj.attachedTo = attachmentSlot;
                attachmentSlot.SetActive(true);
                return true;
            }
        }
        return false;        
    }
    #endregion


    public void Dispose()
    {
        OnStatusChanged = null;
        OnEventTriggered = null;
    }


    #region LogicalMethods
    public virtual SimObjInputData GetSimObjInputData() { return emptyInputData; }
    protected virtual float GetAttachObjectCohesion() { return 1; }

    protected virtual void StatusChanged(SimObjCommunication.InstanceStatus status)
    {
        if(status==SimObjCommunication.InstanceStatus.DEAD)
            Explode();
    }
    protected virtual void EventTriggered(EventType type,bool evOccurs)
    {
        //Implement default events here.
    }
    #endregion

    #region VirtualLifecycleMethos    

    //protected virtual void BeforeTransformSet() { }
    //protected virtual void AfterTransformSet() { }
    protected virtual void SimDataUpdated() { }
    protected virtual void InputDataSent() { }

    #endregion


}


public class EmptySimObj : SimCommObject
{

}