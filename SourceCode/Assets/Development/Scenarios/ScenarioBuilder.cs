using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScenarioBuilder : MonoBehaviour
{

    public enum Scenario
    {
        ai,
        aircraftSim,
    }

    public Scenario scenario;
    public SimObjCommunication communication;
    bool[] acDataStatus;

    public AIAircraft aiAircraftPrefab;
    public Missile missilePrefab;
    public SAM samPrefab;




    public bool isPlayerAircraftInAIControl = false;

    //AI Com
    public Action<uint, bool> SetControlToAI;

    //SimCom
    private Action<float> SetHOT;
    public Action<bool> SetGearDown;
    public Action<bool> SetSteeringActive;
    public Action<float, float> SetBrakes;


    public Queue<SimObjData> initializetionJobList= new Queue<SimObjData>(); 


    private void Awake()
    {
        GameManager.GameStarted += AfterGameStarted;
    }

    private void AfterGameStarted()
    {

        try
        {
            CommunicationConfig confData = Utility.ReadJsonDataFromStreamingAssets<CommunicationConfig>("comConfig.json");

            switch (scenario)
            {
                case Scenario.ai:
                    communication = new AISimCom(confData, ref SetControlToAI);
                    InputManager.Instance.AircraftInputs.AircraftInputs_Development.AISwitch.performed += SwitchAIOpts;
                    SetAIControl(true);
                    break;
                case Scenario.aircraftSim:
                    communication = new AircraftCom(confData, ref SetGearDown, ref SetSteeringActive, ref SetHOT, ref SetBrakes);
                    break;
                default:
                    Debug.LogError("Scenario not setted!");
                    break;
            }


            communication.OnInstanceNumberChanged += OnInstanceNumberChanged;

            GameManager.Instance.InitPreRegisteredSimObjs(communication);
        }
        catch (Exception ex)
        {

        }
    }

    uint[] instanceIDArr;
    private void OnInstanceNumberChanged()
    {
        instanceIDArr = new uint[communication.InstanceNumber];

        //0 is player ac
        for (int i = 1; i < communication.InstanceNumber; i++)
        {

            SimObjData simObjData = communication.GetSimObjData(i);
            instanceIDArr[i] = simObjData.instanceID;

            initializetionJobList.Enqueue(simObjData);
            //SyncInstances(ref simObjData);
            //StartCoroutine(SyncInstanceses(simObjData));
        }
    }


    void SyncInstances(ref SimObjData simObjData)
    {
        if (GameManager.Instance.GetSimObjByInstanceID(simObjData.instanceID) == null)
        {
            try
            {
                SimCommObject simObj;
                switch (simObjData.type)
                {
                    case SimObjCommunication.InstanceTypes.DUMMY_ESCAPE:
                    case SimObjCommunication.InstanceTypes.DUMMY_CHASE:
                    case SimObjCommunication.InstanceTypes.DUMMY_GOING_FORWARD:
                    case SimObjCommunication.InstanceTypes.F22_INSOURCED:
                    case SimObjCommunication.InstanceTypes.F16_INSOURCED:
                    case SimObjCommunication.InstanceTypes.SU27:
                    case SimObjCommunication.InstanceTypes.OTHER_CLIENT_AIRCRAFT_INSTANCE:
                        simObj = Instantiate(aiAircraftPrefab);
                        simObj.name = "AI Aircraft_" + simObjData.instanceID;
                        break;
                    case SimObjCommunication.InstanceTypes.MISSILLE:
                        simObj = Instantiate(missilePrefab);
                        simObj.name = "Missile_" + simObjData.instanceID;
                        break;
                    case SimObjCommunication.InstanceTypes.SAM:
                        simObj = Instantiate(samPrefab);
                        simObj.name = "SAM_" + simObjData.instanceID;
                        simObj.simModelOffset = new Vector3(400,0,0);
                        break;
                    default:
                        simObj = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<EmptySimObj>();
                        simObj.name = "SimObj_" + simObjData.instanceID;
                        break;
                }



                simObj.SetCommunication(communication, Array.IndexOf(instanceIDArr, simObjData.instanceID));
            }
            catch(Exception e )
            {
                Debug.Log(e);
            }
        }
    }

    private void Update()
    {
        while(initializetionJobList.Count > 0)
        {
            SimObjData simObjData= initializetionJobList.Dequeue();
            SyncInstances(ref simObjData);
        }
    }

    private void SwitchAIOpts(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        SetAIControl(!isPlayerAircraftInAIControl);

    }
    
    void SetAIControl(bool isAIControls=false)
    {
        if (SetControlToAI != null)
        {
            SetControlToAI.Invoke(GameManager.PlayerAircraft.GetSimObjData().instanceID, isAIControls);
            GameManager.PlayerAircraft.SetAIControl(isAIControls);
            isPlayerAircraftInAIControl = isAIControls;
        }
    }


    private void OnApplicationQuit()
    {
        if(communication!=null)
            communication.Dispose();
    }

}
