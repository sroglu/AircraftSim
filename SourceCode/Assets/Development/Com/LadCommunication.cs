using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class TouchData
{
    public float x = 0f;
    public float y = 0f;
    public int touchType = 0; // 0: none, 1:down, 2:up        
}

[Serializable]
public class HotasData
{
    public int cursorLeft = 0;
    public int cursorRight = 0;
    public int cursorUp = 0;
    public int cursorDown = 0;

    public int tmsUp=0;//bug key
    public int tmsDown=0;//bug key

    public int dmsLeft = 0;//Active Display
    public int dmsRight = 0;//Active Display
}
[Serializable]
public class TouchDataArr
{
    public int count = 0;
    public TouchData[] dataArr;
    public HotasData hotasData;

}

public class LadCommunication : Communication<TouchDataArr, object>
{
    public LadCommunication(CommunicationConfig configData) : base(configData)
    {
    }

    protected override bool CheckSndDataReady(TouchDataArr sndData)
    {
        return (sndData.count > 0);
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
        sndData = new byte[64];
    }

    protected override bool PackData(TouchDataArr data, ref byte[] sndData)
    {
        bool result = false;
        try
        {
            Array.Copy(BitConverter.GetBytes(data.count), 0, sndData, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)data.dataArr[0].x), 0, sndData, sizeof(int), sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)data.dataArr[0].y), 0, sndData, sizeof(int) * 2, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.dataArr[0].touchType), 0, sndData, sizeof(int) * 3, sizeof(int));

            Array.Copy(BitConverter.GetBytes(data.hotasData.cursorLeft), 0, sndData, sizeof(int) * 4, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.cursorRight), 0, sndData, sizeof(int) * 5, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.cursorUp), 0, sndData, sizeof(int) * 6, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.cursorDown), 0, sndData, sizeof(int) * 7, sizeof(int));


            Array.Copy(BitConverter.GetBytes(data.hotasData.tmsUp), 0, sndData, sizeof(int) * 8, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.tmsDown), 0, sndData, sizeof(int) * 9, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.dmsLeft), 0, sndData, sizeof(int) * 10, sizeof(int));
            Array.Copy(BitConverter.GetBytes(data.hotasData.dmsRight), 0, sndData, sizeof(int) * 11, sizeof(int));

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

    protected override void OnEndOfCommunication() { }
}



public class SomeComModule : Communication<int, string>
{
    public SomeComModule(CommunicationConfig configData) : base(configData)
    {
    }

    protected override void InitRcvData(out string rcvData)
    {
        throw new NotImplementedException();
    }

    protected override void InitSndData(out byte[] sndData)
    {
        throw new NotImplementedException();
    }

    protected override bool PackData(int data, ref byte[] sndData)
    {
        throw new NotImplementedException();
    }

    protected override bool UnPackData(byte[] data, ref string rcvData)
    {
        throw new NotImplementedException();
    }
}