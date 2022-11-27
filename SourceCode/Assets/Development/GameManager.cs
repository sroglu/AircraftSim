using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
        private set
        {
            if (instance == null)
                instance = value;
            else
                Destroy(value);
        }
    }

    public static Vector3 SimPivot
    {
        get
        {
            if (PlayerAircraft == null)
                return Vector3.zero;
            return PlayerAircraft.GetEarthPos();
        }
    }


    public SimObjData defaultAircraftData;


    [SerializeField]
    List<SimCommObject> preRegisteredSimObjects;
    [SerializeField]
    List<SimCommObject> simObjects;
    [SerializeField]
    Dictionary<GameObject,Vector3> otherRelativeInstances;

    [SerializeField]
    PlayerAircraft playerAircraft;
    public static PlayerAircraft PlayerAircraft { get { return Instance.playerAircraft; } }
    //public static void SetPlayerAircraft(AircraftRefactored playerAircraft) { PlayerAircraft = playerAircraft; }

    public static Action GameStarted=null;

    private void Awake()
    {
        Instance = this;
        simObjects = new List<SimCommObject>();
        otherRelativeInstances= new Dictionary<GameObject, Vector3>();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "FPS: "+ fps);
    }

    int fps;
    private void Update()
    {
        fps = (int)(1f / Time.unscaledDeltaTime);
    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LateInvoke(new Action[] { GameStarted}));
    }

    public IEnumerator LateInvoke(Action[] callbacks,ushort waitFrameCount=1)
    {
        ushort counter = waitFrameCount;
        foreach (Action callback in callbacks)
        {
            counter = waitFrameCount;
            while (counter > 0)
            {
                yield return new WaitForEndOfFrame();
                counter--;
            }
            callback?.Invoke();
        }
    }


    // Update is called once per frame
    //void Update()
    //{
    //    //foreach (var relativeInstance in otherRelativeInstances)
    //    //{
    //    //    relativeInstance.Key.transform.position=relativeInstance.Value-SimPivot;
    //    //}
    //}

    public SimCommObject GetSimObjByInstanceID(uint instanceID)
    {
        return simObjects.Find((simObj) => simObj.GetSimObjData().instanceID == instanceID);
    }

    public void InitPreRegisteredSimObjs(SimObjCommunication comm) {
        foreach (var simObj in preRegisteredSimObjects)
        {
            if (simObj.IndexInRcv != int.MaxValue)
                simObj.SetCommunication(comm, simObj.IndexInRcv);
        }
    }

    public uint RegisterSimObject(SimCommObject simObj)
    {
        if (!IsRegistered(simObj))
        {
            simObjects.Add(simObj);
            simObjects=simObjects.OrderBy((x) => x.IndexInRcv).ToList();
        }

        return (uint)(simObjects.Count-1);
    }

    public bool IsRegistered(SimCommObject simObj)
    {
        return simObjects.Contains(simObj);
    }

    //public void RegisterRelativeInstances(GameObject go)
    //{
    //    if (!otherRelativeInstances.Keys.Contains(go))
    //        otherRelativeInstances.Add(go, SimPivot);
    //}   
    //public void RemoveRelativeInstances(GameObject go)
    //{
    //    if (otherRelativeInstances.Keys.Contains(go))
    //        otherRelativeInstances.Remove(go);
    //}


    private void OnApplicationQuit()
    {
        foreach (var aircraft in simObjects)
        {
            aircraft.Dispose();
        }

        GameStarted = null;
    }

}
