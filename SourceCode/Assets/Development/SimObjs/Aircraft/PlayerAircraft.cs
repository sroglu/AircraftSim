using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAircraft : Aircraft
{
    [SerializeField]
    Afterburner afterburner;
    [SerializeField]
    GameObject landinGears;

    [SerializeField, Range(0f, 1f)]
    float afterburnerStartValue=0.7f;

    [SerializeField]
    bool isPlayerAircraftInAIControl=false;
    

    #region Actions
    public void SetAIControl(bool isPlayerAircraftInAIControl)
    {
        this.isPlayerAircraftInAIControl=isPlayerAircraftInAIControl;
    }



    //public void UseFlare(Action destroyFlareCallback=null)
    //{
    //    foreach (var flareP in flareStartPoints)
    //    {
    //        HandleVFX flareVFX = GameObject.Instantiate(Assets.Instance.FlareHandleVFX);
    //        flareVFX.Size = ObjSize;

    //        flareVFX.transform.position = flareP.transform.position;
    //        flareVFX.transform.rotation = flareP.transform.rotation;

    //        flareVFX.AnimForce = Vector3.back;
    //        flareVFX.SetDestroyAction(out destroyFlareCallback);
    //        flareVFX.PlayVFX();
    //        flareVFX.ObjPhysics.AddForce(flareVFX.transform.forward * ObjSize * 10, ForceMode.Impulse);
    //    }
    //}
    //public void UseChaff(Action destroyChaffCallback = null)
    //{
    //    HandleVFX chaffVFX = GameObject.Instantiate(Assets.Instance.ChaffHandleVFX);
    //    chaffVFX.Size = ObjSize;
    //    chaffVFX.SetDestroyAction(out destroyChaffCallback);
    //    chaffVFX.PlayVFX();
    //}

    //void UseFlare(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{
    //    UseFlare();
    //}

    //void UseChaff(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{
    //    UseChaff();
    //}

    //void UseMissile(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    //{

    //}


    #endregion

    public override SimObjInputData GetSimObjInputData()
    {
        UpdateAfterBurner();
        return InputManager.Instance.AircraftInput;
    }
    protected override void Init()
    {
        base.Init();
        IndexInRcv = 0;
        SetSimObjData(GameManager.Instance.defaultAircraftData);

        //InputManager.Instance.AircraftInputs.AircraftInputs_Development.Fire.performed += UseMissile;
        //InputManager.Instance.AircraftInputs.AircraftInputs_Development.UseFlare.performed += UseFlare;
        //InputManager.Instance.AircraftInputs.AircraftInputs_Development.UseChaff.performed += UseChaff;


        afterburner.startValMultiplier = ObjSize;
        afterburner.SetAfterburnerVal(1);
        afterburner.SetActiveBurner(false);
    }


    void UpdateAfterBurner()
    {
        if (InputManager.Instance.AircraftInput.throttle >= afterburnerStartValue)
        {
            afterburner.SetActiveBurner(true);
            afterburner.SetAfterburnerVal(
                ((float)InputManager.Instance.AircraftInput.throttle - afterburnerStartValue) /
                (1f - afterburnerStartValue)
                );
        }
        else
        {
            afterburner.SetActiveBurner(false);
            afterburner.SetAfterburnerVal(0);
        }
    }

    protected override void StatusChanged(SimObjCommunication.InstanceStatus status)
    {
        base.StatusChanged(status);

        landinGears.SetActive(status == SimObjCommunication.InstanceStatus.NONE || status == SimObjCommunication.InstanceStatus.STATIONARY);

    }

    //protected override void EventTriggered(EventType type, bool evOccurs)
    //{
    //    base.EventTriggered(type, evOccurs);

    //    Action destroyChaffCallback=null, destroyFlareCallback= null;

    //    switch (type)
    //    {
    //        case EventType.chaff:
    //            if(evOccurs)
    //                UseChaff(destroyChaffCallback);
    //            else 
    //                destroyChaffCallback?.Invoke();
    //            break;

    //        case EventType.flare:
    //            if(evOccurs)
    //                UseFlare(destroyFlareCallback);
    //            else
    //                destroyFlareCallback?.Invoke();
    //            break;
    //    }

    //}
}
