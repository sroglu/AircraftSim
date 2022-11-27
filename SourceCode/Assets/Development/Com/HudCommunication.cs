using System;
using System.Runtime.InteropServices;


public struct HudData
{
    public float speed;
    public float alt;
    public float roll;
    public float pitch;
    public float yaw;

    public HudData(SimObjData aircraftData)
    {
        speed = (float)aircraftData.speed;
        alt = (float)aircraftData.transform.alt;
        roll = (float)aircraftData.transform.roll;
        pitch = (float)aircraftData.transform.pitch;
        yaw = (float)aircraftData.transform.yaw;
    }

}


public class HudCommunication : Communication<HudData, object>
{
    public HudCommunication(CommunicationConfig configData) : base(configData)
    {
    }

    protected override bool CheckSndDataReady(HudData sndData)
    {
        return true;
    }

    protected override int GetPackageSizeRcv(ref CommunicationConfig configData)
    {
        return -1;
    }

    protected override void InitRcvData(out object rcvData)
    {
        rcvData = null;
    }

    protected override void InitSndData(out byte[] sndData)
    {
        sndData = new byte[Marshal.SizeOf(typeof(HudData))];
    }

    protected override bool PackData(HudData data, ref byte[] sndData)
    {
        bool result = false;
        try
        {
            Array.Copy(BitConverter.GetBytes(data.speed), 0, sndData, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes(data.alt), 0, sndData, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes(data.pitch), 0, sndData, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes(data.yaw), 0, sndData, 0, sizeof(float));
            Array.Copy(BitConverter.GetBytes(data.roll), 0, sndData, 0, sizeof(float));

            result = true;
        }
        catch (Exception ex)
        {

        }
        return result;
    }

    protected override bool UnPackData(byte[] data, ref object rcvData)
    {
        return true;
    }

    protected override void OnEndOfCommunication()
    {
    }
}
