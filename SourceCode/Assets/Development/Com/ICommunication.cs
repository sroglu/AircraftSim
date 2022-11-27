using System;
using System.Collections.Generic;
using UnityEngine;


//public interface ICommunication : IDisposable
//{
//    public void SendData();
//    public void ReceiveData();
//}


public interface ICommunication<Snd,Rcv> :IDisposable
{
    public bool SendData(Snd data);
    public Rcv ReceiveData(byte[] data);
    public Rcv GetLastReceivedData();
}

public class CommunicationConfig
{
    public string otherIP;
    public int portRcv;
    public int portSnd;
}

public abstract class Communication<Snd, Rcv> : ICommunication<Snd, Rcv>
{
    public enum NetworkProtocol { UDP }

    public NetworkProtocol protocol = NetworkProtocol.UDP;
    [Header("Sender Settings")]
    public string IP;
    public int sndPort;
    [Header("Receiver Settings")]
    public int rcvPort;
    public int rcvPackageSize;

    protected UDP_Sender senderUDP;
    protected UDP_Receiver receiverUDP;

    byte[] sndBinData;
    Rcv rcvData,defaultRcvData;
    bool dataReceived = false;

    CommunicationConfig configData;

    public Communication(CommunicationConfig configData)
    {
        this.configData = configData;
        IP = configData.otherIP;
        sndPort = configData.portSnd;
        rcvPort = configData.portRcv;

        UpdateRcvPackageSize();

        if (rcvPackageSize!=-1)
            InitRcvData(out rcvData);

        InitSndData(out sndBinData);

        switch (protocol)
        {
            default:
            case NetworkProtocol.UDP:
                senderUDP = new UDP_Sender(IP, sndPort);
                if (rcvPackageSize != -1 && rcvPort!=-1)
                {
                    receiverUDP = new UDP_Receiver(rcvPort, rcvPackageSize, IP);
                    receiverUDP.OnDataReceived += (data) => { rcvData = ReceiveData(data); };
                }
                break;
        }
    }

    protected void UpdateRcvPackageSize()
    {
        rcvPackageSize = GetPackageSizeRcv(ref configData);
    }


    public void Dispose()
    {
        OnEndOfCommunication();
        if(senderUDP!=null)
            senderUDP.Dispose();
        if(receiverUDP != null)
            receiverUDP.Dispose();
    }


    public bool SendData(Snd data)
    {
        bool success = false;
        PackData(data, ref sndBinData);
        if (sndBinData != null && senderUDP != null && CheckSndDataReady(data))
        {
            senderUDP.SetSnapshotData(sndBinData);
            OnDataSend(ref data);
            success = true;
        }
        return success;
    }

    public Rcv ReceiveData(byte[] data)
    {
        if (UnPackData(data, ref rcvData))
        {
            OnDataReceived(ref rcvData);
            return rcvData;
        }
        return defaultRcvData;
    }

    public Rcv GetLastReceivedData()
    {
        if(rcvData!=null)
            return rcvData;
        return defaultRcvData;
    }

    public void SetDefaultRcvData(Rcv defaultRcvData)
    {
        this.defaultRcvData = defaultRcvData;
        rcvData = this.defaultRcvData;
    }

    protected virtual int GetPackageSizeRcv(ref CommunicationConfig configData) { return 255; }
    protected virtual void OnEndOfCommunication() { }
    protected virtual bool CheckSndDataReady(Snd sndData) { return true; }

    protected abstract void InitRcvData(out Rcv rcvData);
    protected abstract void InitSndData(out byte[] sndData);

    protected abstract bool PackData(Snd data,ref byte[] sndData);
    protected abstract bool UnPackData(byte[] data,ref Rcv rcvData);

    protected virtual void OnDataReceived(ref Rcv rcvData) { }
    protected virtual void OnDataSend(ref Snd rcvData) { }

}
