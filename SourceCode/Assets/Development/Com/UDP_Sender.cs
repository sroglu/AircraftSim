using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UDP_Sender : IDisposable
{
    byte[] sendData;
    IPEndPoint remoteEndPoint;
    UdpClient client;


    private bool connected = false;
    BackgroundWorker worker;

    public bool Available { 
        get { 
            if(client==null) return false; 
            return client.Available>0;
        } 
    }


    private bool disposed = false;

    public UDP_Sender(string IP, int port)
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        worker = new BackgroundWorker();
        worker.DoWork += new DoWorkEventHandler(SetData);
        worker.WorkerSupportsCancellation = true;



        //client = new UdpClient();
        StartConnection();
    }



    void StartConnection(bool alwaysSendData=false)
    {
        connected = alwaysSendData;
        client = new UdpClient();
        if(connected)
            worker.RunWorkerAsync();
    }


    private void SetData(object sender, DoWorkEventArgs e)
    {
        while (connected)
        {
            SetSnapshotData(sendData);            
        }
    }

    public void SetSnapshotData(byte[] data)
    {
        client.Send(data, data.Length, remoteEndPoint);
    }

    void EndConnection()
    {
        connected = false;
        worker.CancelAsync();
        client.Dispose();
    }

    public void Dispose()
    {
        EndConnection();
    }
}
