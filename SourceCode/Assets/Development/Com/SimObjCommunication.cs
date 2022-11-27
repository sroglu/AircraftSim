using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SimObjTransform
{
    public double lat;
    public double lon;
    public double alt;

    public double pitch;
    public double yaw;
    public double roll;

    public static SimObjTransform Zero { 
        get {
            SimObjTransform zero = new SimObjTransform();
            zero.lat = 0;
            zero.lon = 0;
            zero.alt = 0;
            zero.pitch = 0;
            zero.yaw = 0;
            zero.roll = 0;
            return zero;
        } 
    }    
    public static SimObjTransform SeaLevelZero
    { 
        get {
            SimObjTransform zero = new SimObjTransform();
            zero.lat = 0;
            zero.lon = 0;
            zero.alt = 8848f;
            zero.pitch = 0;
            zero.yaw = 0;
            zero.roll = 0;
            return zero;
        } 
    }

    public override bool Equals(object obj)
    {
        if (!(obj is SimObjTransform))
            return false;

        SimObjTransform simObj = (SimObjTransform)obj;
        return (
            lat == simObj.lat &&
            lon == simObj.lon &&
            alt == simObj.alt &&
            pitch == simObj.pitch &&
            yaw == simObj.yaw &&
            roll == simObj.roll);
    }

    public static bool operator ==(SimObjTransform x, SimObjTransform y)
    {
        return x.Equals(y);
    }
    public static bool operator !=(SimObjTransform x, SimObjTransform y)
    {
        return !(x == y);
    }
    public override int GetHashCode()
    {
        return  lat.GetHashCode() * 
                lon.GetHashCode() * 
                alt.GetHashCode() * 
                pitch.GetHashCode() * 
                yaw.GetHashCode() * 
                roll.GetHashCode() ;
    }
}

[System.Serializable]
public struct SimObjData
{
    public DateTime timeStamp;
    public uint instanceID;
    public uint attachedInstanceID;
    public SimObjCommunication.InstanceStatus status;
    public SimObjCommunication.InstanceTypes type;
    public uint team;
    public SimObjTransform transform;

    public double speed;

    public uint chaffTrigger;
    public uint chaffOnFlight;
    public uint flareTrigger;
    public uint flareOnFlight;


    public float kias_kts;
    public float nz_g;
    public float mach_number;
    public float radar_altitude_ft;
    public float vertical_speed_fpm;
    public float angle_of_attack;
    public float angle_of_slide_deg;
    public float gamma_deg;
    public float throttle;
    public float lateral_fpm_deg;
    public float target_roll_deg;
    public float target_pitch_deg;
    public float target_lateral_deg;
    public int pipper_red;


    public override bool Equals(object obj)
    {
        if (!(obj is SimObjData))
            return false;

        SimObjData simObj = (SimObjData)obj;
        return (
            instanceID == simObj.instanceID &&
            timeStamp == simObj.timeStamp);
    }

    public static bool operator ==(SimObjData x, SimObjData y)
    {
        return x.Equals(y);
    }
    public static bool operator !=(SimObjData x, SimObjData y)
    {
        return !(x == y);
    }
    public override int GetHashCode()
    {
        return instanceID.GetHashCode() * timeStamp.GetHashCode();
    }
}

[System.Serializable]
public struct SimObjInputData
{
    public uint instanceID;
    public double jostickX;
    public double jostickY;
    public double jostickZ;

    public double throttle;

    //TODO: Düzeltilecek
    public uint chaffTrigger,flareTrigger,jammerTrigger,missileTrigger;
}

public abstract class SimObjCommunication : Communication<SimObjInputData, SimObjData[]>
{
    public enum InstanceAction
    {
        NONE = 0,
        FLARE = 1,
        MOVING = 2,
        DEAD = 3,
    }
    public enum InstanceStatus
    {
        NONE = 0,
        STATIONARY = 1,
        MOVING = 2,
        DEAD = 3,
    }

    public enum InstanceTypes
    {
        DUMMY_ESCAPE=0,
        DUMMY_CHASE=1,
        DUMMY_GOING_FORWARD=2,
        F22_INSOURCED=3,
        F22_OUTSOURCED=4,
        F16_INSOURCED=5,
        F16_OUTSOURCED=6,
        AC_INSOURCED=7,
        AC_OUTSOURCED=8,
        SAM=9,
        OTHER_CLIENT_AIRCRAFT_INSTANCE=10,
        SU27=11,
        MISSILLE=12,

    }   


    public Action OnInstanceNumberChanged;
    public uint InstanceNumber { get; private set; }
    protected SimObjCommunication(CommunicationConfig configData,uint instanceNumber) : base(configData)
    {
        InstanceNumber=instanceNumber;
    }

    protected sealed override void InitRcvData(out SimObjData[] rcvData)
    {
        rcvData = new SimObjData[InstanceNumber];
    }

    public SimObjData GetSimObjData(int index = 0)
    {
        SimObjData[] simObjData = GetLastReceivedData();
        if (simObjData != null && simObjData.Length > 0 && index < simObjData.Length)
        return simObjData[(int)index];
        return new SimObjData();
    }

    protected override void OnDataReceived(ref SimObjData[] rcvData)
    {
        for (int i = 0; i < rcvData.Length; i++)
        {
            rcvData[i].timeStamp= DateTime.Now;
        }

        if(rcvData.Length!=InstanceNumber)
        {
            InstanceNumber = (uint)rcvData.Length;
            UpdateRcvPackageSize();
            OnInstanceNumberChanged();
        }
    }

    public static bool IsRcvDataValid(ref SimObjData rcvData)
    {
        return rcvData.timeStamp != DateTime.MinValue;
    }
}







class AircraftCom : SimObjCommunication
{
    float hot = 835f;
    bool gear_down = true;
    float leftBrake = 0.5f;
    float rightBrake = 0.5f;
    bool steeringActive = true;

    //Inputs
    int LatAddress { get { return SimConfig.oLatAddr * sizeof(double); } }
    int LonAddress { get { return SimConfig.oLonAddr * sizeof(double); } }
    int AltAddress { get { return SimConfig.oAltAddr * sizeof(double); } }
    int PitchAddress { get { return SimConfig.oPitchAddr * sizeof(double); } }
    int RollAddress { get { return SimConfig.oRollAddr * sizeof(double); } }
    int YawAddress { get { return SimConfig.oYawAddr * sizeof(double); } }
    int SpeedAddress { get { return SimConfig.oSpeedAddr * sizeof(double); } }


    //Outputs
    int Send_JostickXAddress { get { return SimConfig.iAileronAddr * sizeof(double); } }
    int Send_JostickYAddress { get { return SimConfig.iElevatorAddr * sizeof(double); } }
    int Send_JostickZAddress { get { return SimConfig.iRudderAddr * sizeof(double); } }
    int Send_ThrottleAddress { get { return SimConfig.iThrottleAddr * sizeof(double); } }
    int Send_LeftBrakeAddress { get { return SimConfig.iBreakLeftAddr * sizeof(double); } }
    int Send_RightBrakeAddress { get { return SimConfig.iBrakeRightAddr * sizeof(double); } }
    int Send_GearDownAddress { get { return SimConfig.iGearDownAddr * sizeof(double); } }
    int Send_SpeedBrakeAddress { get { return SimConfig.iSpeedBrakeAddr * sizeof(double); } }
    int Send_SteeringActiveAddress { get { return SimConfig.iSteeringActive * sizeof(double); } }
    int Send_HeightOfTerrainAddress { get { return SimConfig.iHeightOfTerAddr * sizeof(double); } }

    AircraftSimConfigData simConf;


    public AircraftCom(CommunicationConfig configData,
        ref Action<bool> SetGearDown,
        ref Action<bool> SetSteeringActive,
        ref Action<float> SetHOT,
        ref Action<float, float> SetBrakes) : base(configData,1)
    {
        SetGearDown += (isGearDown) =>
        {
            gear_down = isGearDown;
        };
        SetSteeringActive += (isSteeringActive) =>
        {
            steeringActive = isSteeringActive;
        };
        SetHOT += (desired_hot) =>
        {
            hot = desired_hot;
        };
        SetBrakes += (leftBrakeVal, rightBrakeVal) =>
        {
            leftBrake = leftBrakeVal;
            rightBrake = rightBrakeVal;
        };
        //sendData = new byte[sizeof(double) * SimConfig.inputSize];
        //rcvAircraftDataArr = new AircraftData[GetNumberOfAircraft()];

        SetDefaultRcvData(new SimObjData[] { GameManager.Instance.defaultAircraftData });
    }

    AircraftSimConfigData SimConfig
    {
        get
        {
            if (simConf == null)
                simConf = Utility.ReadJsonDataFromStreamingAssets<AircraftSimConfigData>("aircraftSimConfigData.json");
            return simConf;
        }
    }


    protected override void InitSndData(out byte[] sndData)
    {
        sndData = new byte[sizeof(double) * SimConfig.inputSize];
    }

    protected override int GetPackageSizeRcv(ref CommunicationConfig configData)
    {
        return 1376;
    }

    protected override bool PackData(SimObjInputData data, ref byte[] sndData)
    {
        bool result = false;
        try
        {
            float aileronIntpol = NormalizeScale((float)data.jostickX, -1, 1, SimConfig.minAileron, SimConfig.maxAileron);
            Array.Copy(BitConverter.GetBytes((double)aileronIntpol), 0, sndData, Send_JostickXAddress, sizeof(double));

            float rudderIntpol = NormalizeScale((float)data.jostickZ, -1, 1, SimConfig.minRudder, SimConfig.maxRudder);
            Array.Copy(BitConverter.GetBytes((double)rudderIntpol), 0, sndData, Send_JostickZAddress, sizeof(double));

            float elevatorIntpol = NormalizeScale((float)-data.jostickY, -1, 1, SimConfig.minElevator, SimConfig.maxElevator);
            Array.Copy(BitConverter.GetBytes((double)elevatorIntpol), 0, sndData, Send_JostickYAddress, sizeof(double));

            float throttleNorm = NormalizeFit((float)data.throttle, -1, 1, SimConfig.minThrottle, SimConfig.maxThrottle);
            Array.Copy(BitConverter.GetBytes((double)throttleNorm), 0, sndData, Send_ThrottleAddress, sizeof(double));

            Array.Copy(BitConverter.GetBytes(Convert.ToDouble(gear_down)), 0, sndData, Send_GearDownAddress, sizeof(double));
            Array.Copy(BitConverter.GetBytes((double)hot), 0, sndData, Send_HeightOfTerrainAddress, sizeof(double));

            Array.Copy(BitConverter.GetBytes(Convert.ToDouble(leftBrake)), 0, sndData, Send_LeftBrakeAddress, sizeof(double));
            Array.Copy(BitConverter.GetBytes(Convert.ToDouble(rightBrake)), 0, sndData, Send_RightBrakeAddress, sizeof(double));
            Array.Copy(BitConverter.GetBytes(Convert.ToDouble(steeringActive)), 0, sndData, Send_SteeringActiveAddress, sizeof(double));

            Array.Copy(BitConverter.GetBytes(Convert.ToDouble(false)), 0, sndData, Send_SpeedBrakeAddress, sizeof(double));
            
            result = true;
        }
        catch (Exception ex)
        {

        }
        return result;
    }

    protected override bool UnPackData(byte[] data, ref SimObjData[] rcvData)
    {
        bool result = false;
        try
        {
            rcvData[0].transform.lat = BitConverter.ToDouble(data, LatAddress);
            rcvData[0].transform.lon = BitConverter.ToDouble(data, LonAddress);
            rcvData[0].transform.alt = BitConverter.ToDouble(data, AltAddress) * Constants.FEET_TO_METER - Constants.OFFSET_BY_TERRAIN;


            rcvData[0].speed = BitConverter.ToDouble(data, SpeedAddress);

            rcvData[0].transform.pitch = BitConverter.ToDouble(data, PitchAddress);
            rcvData[0].transform.roll = BitConverter.ToDouble(data, RollAddress);
            rcvData[0].transform.yaw = BitConverter.ToDouble(data, YawAddress);
            result = true;
        }
        catch (Exception ex)
        {

        }
        return result;
    }

    protected override void OnEndOfCommunication()
    {

    }

    private float NormalizeScale(float value, float minVal, float maxVal, float minNorm, float maxNorm)
    {
        if (value > 0)
        {
            return maxNorm * value;
        }
        else
        {
            return minNorm * Mathf.Abs(value);
        }
    }

    private float NormalizeFit(float value, float minVal, float maxVal, float minNorm, float maxNorm)
    {
        float valRange = maxVal - minVal;
        float valPlace = value - minVal;
        float valRatio = valPlace / valRange;

        float normRange = maxNorm - minNorm;
        return (normRange * valRatio) + minNorm;
    }

    protected override bool CheckSndDataReady(SimObjInputData sndData)
    {
        return true;
    }

    public class AircraftSimConfigData
    {
        public string ladIP;
        public int ladPort;
        public string FlightModelIP;
        public int flightModelPort;
        public string vrIP;
        public int vrPort;
        public string toFlightModelPort;
        public int toLadPort;

        public int inputSize;
        public int iThrottleAddr;
        public int iBrakeRightAddr;
        public int iBreakLeftAddr;
        public int iGearDownAddr;
        public int iSpeedBrakeAddr;
        public int iSteeringActive;
        public int iHeightOfTerAddr;
        public int iElevatorAddr;
        public int iAileronAddr;
        public int iRudderAddr;

        public float minAileron;
        public float maxAileron;
        public float minRudder;
        public float maxRudder;
        public float minElevator;
        public float maxElevator;
        public float minThrottle;
        public float maxThrottle;
        public float minBrakeRight;
        public float maxBrakeRight;
        public float minBrakeLeft;
        public float maxBrakeLeft;

        public int oAltAddr;
        public int oRollAddr;
        public int oPitchAddr;
        public int oSpeedAddr;
        public int oYawAddr;
        public int oLatAddr;
        public int oLonAddr;

        public float startGroundLat;
        public float startGroundLon;
        public float startGroundAlt;

        public void SetDefault()
        {
            ladIP = "127.0.0.1";
            ladPort = 8007;
            FlightModelIP = "127.0.0.1";
            flightModelPort = 8008;
            vrIP = "192.168.20.247";
            vrPort = 8009;
            toFlightModelPort = "8001";
            toLadPort = 8002;

            inputSize = 65;
            iThrottleAddr = 5;
            iBrakeRightAddr = 6;
            iBreakLeftAddr = 7;
            iGearDownAddr = 14;
            iSpeedBrakeAddr = 15;
            iSteeringActive = 19;
            iHeightOfTerAddr = 20;
            iElevatorAddr = 50;
            iAileronAddr = 51;
            iRudderAddr = 52;


            minAileron = -4.6123f;
            maxAileron = 4.6123f;
            minRudder = -1f;
            maxRudder = 1f;
            minElevator = -7.3667f;
            maxElevator = 4.6670f;
            minThrottle = 20f;
            maxThrottle = 130f;
            minBrakeRight = 0f;
            maxBrakeRight = 1f;
            minBrakeLeft = 0f;
            maxBrakeLeft = 1f;

            oSpeedAddr = 1;
            oAltAddr = 7;
            oRollAddr = 8;
            oPitchAddr = 9;
            oYawAddr = 10;
            oLatAddr = 51;
            oLonAddr = 50;

            startGroundLat = 40.068748f;
            startGroundLon = 32.556995f;
            startGroundAlt = 843f;
        }
    }
}

public class AISimCom : SimObjCommunication
{
    private int CounterAddress = 0, InstanceNumberAdress = sizeof(uint);
    //private int instanceRcvDataSize = 48, sndDataSize = 24;
    //private int instanceRcvDataSize = 64, sndDataSize = 40;
    private int instanceRcvDataSize = 120, sndDataSize = 48;
    private Dictionary<uint, bool> controlSimObjData = new Dictionary<uint, bool>();

    int InstanceAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize;
        return -1;
    }
    int LatAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint);
        return -1;
    }
    int LonAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint)*2;
        return -1;
    }
    int AltAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 3;
        return -1;
    }
    int RollAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 4;
        return -1;
    }
    int PitchAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 5;
        return -1;
    }
    int YawAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 6;
        return -1;
    }
    int SpeedAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 7;
        return -1;
    }
    int TypeAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 8;
        return -1;
    }
    int TeamAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 9;
        return -1;
    }
    int AttachedInstanceAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 10;
        return -1;
    }
    int StatusAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 11;
        return -1;
    }
    int ChaffTriggerAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 12;
        return -1;
    }
    int ChaffOnFlightAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 13;
        return -1;
    }
    int FlareTriggerAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 14;
        return -1;
    }
    int FlareOnFlightAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 15;
        return -1;
    }
    int KiasKtsAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 16;
        return -1;
    }
    int NzgAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 17;
        return -1;
    }
    int MachNumberAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 18;
        return -1;
    }
    int RadarAltitudeFtAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 19;
        return -1;
    }
    int VerticalSpeedFpmAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 20;
        return -1;
    }
    int AngleOfAttackAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 21;
        return -1;
    }
    int AngleOfSlideDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 22;
        return -1;
    }
    int GammaDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 23;
        return -1;
    }
    int ThrottleAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 24;
        return -1;
    }
    int LateralFpmDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 25;
        return -1;
    }
    int TargetRollDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 26;
        return -1;
    }
    int TargetPitchDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 27;
        return -1;
    }
    int TargetLateralDegAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 28;
        return -1;
    }
    int PipperRedAddress(int instanceID)
    {
        if (instanceID < rcvInstanceNumber)
            return sizeof(uint) * 2 + instanceID * instanceRcvDataSize + sizeof(uint) * 29;
        return -1;
    }


    //SND addresses
    int Send_IdAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID;
        return -1;
    }
    int Send_ModeAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint);
        return -1;
    }
    int Send_JostickXAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool);
        return -1;
    }
    int Send_JostickYAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint);
        return -1;
    }
    int Send_JostickZAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 2;
        return -1;
    }
    int Send_ThrottleAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 3;
        return -1;
    }
    int Send_ChaffTriggerAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 4;
        return -1;
    }
    int Send_FlareTriggerAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 5;
        return -1;
    }
    int Send_JammerTriggerAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 6;
        return -1;
    }
    int Send_MissileTriggerAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 7;
        return -1;
    }
    int Send_SelectedMissileIDAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 8;
        return -1;
    }
    int Send_EnemyIDAddress(int instanceID)
    {
        if (instanceID < InstanceNumber)
            return sizeof(uint) + sndDataSize * instanceID + sizeof(uint) + sizeof(bool) + sizeof(uint) * 9;
        return -1;
    }

    uint counter,rcvInstanceNumber;

    public AISimCom(CommunicationConfig configData, ref Action<uint, bool> SetControlToAI, uint instanceNumber = 0) : base(configData,instanceNumber)
    {
        SetControlToAI += (instanceID, control) =>
        {
            if (controlSimObjData.ContainsKey(instanceID))
            {
                controlSimObjData[instanceID] = control;
            }
            else
            {
                controlSimObjData.Add(instanceID, control);
            }

            Debug.Log("setted control to: " + control);
        };
        UpdateDefaultData();
    }

    void UpdateDefaultData()
    {
        SimObjData[] defaultData;
        if (InstanceNumber > 0)
        {
            defaultData = new SimObjData[InstanceNumber];
            for (int i = 0; i < defaultData.Length; i++)
            {
                defaultData[i] = GameManager.Instance.defaultAircraftData;
            }
        }
        else
        {
            defaultData = new SimObjData[] { GameManager.Instance.defaultAircraftData };
        }
        SetDefaultRcvData(defaultData);
    }

    protected override bool CheckSndDataReady(SimObjInputData sndData)
    {
        return (sndData.instanceID == 0);
    }

    protected override void InitSndData(out byte[] sndData)
    {
        //sndData = new byte[(sizeof(uint) + acInstanceRcvDataSize * (int)aircraftNumber)];
        //sndData = new byte[sizeof(uint)*6 + sizeof(bool)];
        //sndData = new byte[44];
        sndData = new byte[52];
    }

    protected override int GetPackageSizeRcv(ref CommunicationConfig configData)
    {
        if(InstanceNumber>0)
            return sizeof(uint) * 2 + (int)(instanceRcvDataSize * InstanceNumber);
        return base.GetPackageSizeRcv(ref configData);
    }

    protected override bool PackData(SimObjInputData data, ref byte[] sndData)
    {
        bool result = false;
        try
        {
            sndData= new byte[sndData.Length];
            //sndData = new byte[(sizeof(uint) + acInstanceRcvDataSize * (int)aircraftNumber)];
            Array.Copy(BitConverter.GetBytes(counter), 0, sndData, 0, sizeof(uint));


            Array.Copy(BitConverter.GetBytes(data.instanceID), 0, sndData, Send_IdAddress((int)data.instanceID), sizeof(uint));

            if (controlSimObjData.ContainsKey(data.instanceID))
            {
                Array.Copy(BitConverter.GetBytes(controlSimObjData[data.instanceID]), 0, sndData, Send_ModeAddress((int)data.instanceID), sizeof(bool));
            }
            else
            {
                Array.Copy(BitConverter.GetBytes(false), 0, sndData, Send_ModeAddress((int)data.instanceID), sizeof(bool));
            }

            //Added additional 3 byte to get 28 byte( those bytes will be sent as 0)
            Array.Copy(BitConverter.GetBytes((float)data.jostickX), 0, sndData, Send_JostickXAddress((int)data.instanceID) + 3, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)-data.jostickY), 0, sndData, Send_JostickYAddress((int)data.instanceID) + 3, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)data.jostickZ), 0, sndData, Send_JostickZAddress((int)data.instanceID) + 3, sizeof(float));
            Array.Copy(BitConverter.GetBytes((float)data.throttle), 0, sndData, Send_ThrottleAddress((int)data.instanceID) + 3, sizeof(float));

            Array.Copy(BitConverter.GetBytes(data.chaffTrigger), 0, sndData, Send_ChaffTriggerAddress((int)data.instanceID) + 3, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(data.flareTrigger), 0, sndData, Send_FlareTriggerAddress((int)data.instanceID) + 3, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(data.jammerTrigger), 0, sndData, Send_JammerTriggerAddress((int)data.instanceID) + 3, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(data.missileTrigger), 0, sndData, Send_MissileTriggerAddress((int)data.instanceID) + 3, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(0), 0, sndData, Send_SelectedMissileIDAddress((int)data.instanceID) + 3, sizeof(uint));
            Array.Copy(BitConverter.GetBytes(0), 0, sndData, Send_EnemyIDAddress((int)data.instanceID) + 3, sizeof(uint));

            result = true;
        }
        catch (Exception ex) { }
        return result;
    }

    protected override bool UnPackData(byte[] data, ref SimObjData[] rcvData)
    {
        bool result = false;
        try
        {
            counter = BitConverter.ToUInt32(data, CounterAddress);

            rcvInstanceNumber = BitConverter.ToUInt32(data, InstanceNumberAdress);

            if (rcvData.Length != rcvInstanceNumber)
                rcvData = new SimObjData[rcvInstanceNumber];

            for (int i = 0; i < rcvData.Length; i++)
            {
                rcvData[i].instanceID = (uint)BitConverter.ToInt32(data, InstanceAddress(i));

                rcvData[i].transform.lat = BitConverter.ToSingle(data, LatAddress(i));
                rcvData[i].transform.lon = BitConverter.ToSingle(data, LonAddress(i));
                rcvData[i].transform.alt = BitConverter.ToSingle(data, AltAddress(i)) * Constants.FEET_TO_METER - Constants.OFFSET_BY_TERRAIN;

                rcvData[i].transform.roll = BitConverter.ToSingle(data, RollAddress(i));
                rcvData[i].transform.pitch = BitConverter.ToSingle(data, PitchAddress(i));
                rcvData[i].transform.yaw = BitConverter.ToSingle(data, YawAddress(i));

                rcvData[i].speed = BitConverter.ToSingle(data, SpeedAddress(i));

                rcvData[i].type = (InstanceTypes)(uint)BitConverter.ToInt32(data, TypeAddress(i));
                rcvData[i].team = (uint)BitConverter.ToInt32(data, TeamAddress(i));
                rcvData[i].attachedInstanceID = (uint)BitConverter.ToInt32(data, AttachedInstanceAddress(i));
                rcvData[i].status = (InstanceStatus)(uint)BitConverter.ToInt32(data, StatusAddress(i));
                //TODO: Çok yak???ks?z oldu
                rcvData[i].chaffTrigger = (uint)BitConverter.ToInt32(data, ChaffTriggerAddress(i));
                rcvData[i].chaffOnFlight = (uint)BitConverter.ToInt32(data, ChaffOnFlightAddress(i));
                rcvData[i].flareTrigger = (uint)BitConverter.ToInt32(data, FlareTriggerAddress(i));
                rcvData[i].flareOnFlight = (uint)BitConverter.ToInt32(data, FlareOnFlightAddress(i));

                rcvData[i].kias_kts = BitConverter.ToSingle(data, KiasKtsAddress(i));
                rcvData[i].nz_g = BitConverter.ToSingle(data, NzgAddress(i));
                rcvData[i].mach_number = BitConverter.ToSingle(data, MachNumberAddress(i));
                rcvData[i].radar_altitude_ft = BitConverter.ToSingle(data, RadarAltitudeFtAddress(i));
                rcvData[i].vertical_speed_fpm = BitConverter.ToSingle(data, VerticalSpeedFpmAddress(i));
                rcvData[i].angle_of_attack = BitConverter.ToSingle(data, AngleOfAttackAddress(i));
                rcvData[i].angle_of_slide_deg = BitConverter.ToSingle(data, AngleOfSlideDegAddress(i));
                rcvData[i].gamma_deg = BitConverter.ToSingle(data, GammaDegAddress(i));
                rcvData[i].throttle = BitConverter.ToSingle(data, ThrottleAddress(i));
                rcvData[i].lateral_fpm_deg = BitConverter.ToSingle(data, LateralFpmDegAddress(i));
                rcvData[i].target_roll_deg = BitConverter.ToSingle(data, TargetRollDegAddress(i));
                rcvData[i].target_pitch_deg = BitConverter.ToSingle(data, TargetPitchDegAddress(i));
                rcvData[i].target_lateral_deg = BitConverter.ToSingle(data, TargetLateralDegAddress(i));
                rcvData[i].pipper_red = (int)BitConverter.ToSingle(data, PipperRedAddress(i));

            }
            result = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Unpack Error!!: with message: " + ex?.Message);
        }
        return result;
    }

    protected override void OnEndOfCommunication()
    {

    }
}