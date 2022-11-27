using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


[System.Serializable]
public class UDP_Receiver:IDisposable
{
    [SerializeField]
    int packageSize;
    [SerializeField]
    string IP;  // define in init
    [SerializeField]
    int port;  // define in init

    IPEndPoint remoteEndPoint;
    UdpClient client;

    private bool connected = false;

    public Action<byte[]> OnDataReceived;

    BackgroundWorker worker;


    public UDP_Receiver(int port, int packageSize, string IP="")
    {
        this.packageSize = packageSize;
        this.port = port;
        this.IP = IP;

        if(IP=="")
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        else
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);

        worker = new BackgroundWorker();
        worker.DoWork += new DoWorkEventHandler(GetData);
        worker.WorkerSupportsCancellation = true;

        //if (Utility.IsNetworkAvailable(0))
        //{
        StartConnection();
        //}
        //else
        //{
        //    Debug.LogError("No network!");
        //}


    }

    void StartConnection()
    {
        connected = true;
        client = new UdpClient(port);
        //Debug.Log("UDP_Receiver   ->   port: " + port+ "  packageSize: " + packageSize+ "  remoteEndPoint: "+ remoteEndPoint);

        client.Client.ReceiveBufferSize = packageSize;

        worker.RunWorkerAsync();
    }

    void EndConnection()
    {
        connected = false;
        worker.CancelAsync();
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }
    }

    private void GetData(object sender, DoWorkEventArgs e)
    {
        while (connected)
        {
            //Debug.Log("client.Available? " + client.Available);
            if (client != null&&client.Available > 0)
            {
                byte[] receiveData = client.Receive(ref remoteEndPoint);
                if (receiveData != null && OnDataReceived != null)
                    OnDataReceived.Invoke(receiveData);
            }
        }
    }

    public byte[] GetSnapshotData()
    {
        byte[] receiveData;
        if (client.Available > 0)
        {
            receiveData = client.Receive(ref remoteEndPoint);
            return receiveData;
        }
        return new byte[0];
    }

    public void Dispose()
    {
        OnDataReceived = null;
        EndConnection();
    }



    //IEnumerator GetData()
    //{
    //    if(disposed)
    //        client.Dispose();
    //        yield return null;

    //    if (client.Available > 0)
    //    {
    //        byte[] receiveData = client.Receive(ref remoteEndPoint);
    //        if(receiveData!=null && OnDataReceived!=null)
    //            OnDataReceived.Invoke(receiveData);
    //    }
    //    yield return new WaitForEndOfFrame();
    //    comObj.StartCoroutine(GetData());
    //}

}
