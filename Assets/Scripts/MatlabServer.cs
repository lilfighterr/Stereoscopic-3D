using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using RoomAliveToolkit;

public class MatlabServer : MonoBehaviour {

    public static MatlabServer instance;
    public float xMove, yMove, zMove = 0;
    public float xForce, yForce = 0;
    public float collisionStatus = 0;
    public float forceFeedback = 0;

    [ReadOnly] public bool serverRunning = false;
    [ReadOnly] public string ipAddress = "142.244.62.103"; //This comp: 142.244.63.45, Localhost: 127.0.0.1   
    [ReadOnly] public int port = 9000; 
    private Thread thread;
    private Socket newsock;
    private bool stop = false;
    private bool threadRunning = false;
    private int recvSize = 24;

    // Use this for initialization
    void Awake()
    { //Always called before start() functions
      //Makes sure that there is only one instance of Matlab Server (singleton)
        if (instance == null) //If no game control found
        {
            instance = this; //Then this is the instance of the game control
        }
        else if (instance != this) //If the game object finds that instance is already on another game object, then this destroys itself as it's not needed
        {
            Destroy(gameObject);
        }
        //ipAddress = PlayerPrefs.GetString("IPAddress", "127.0.0.1");
        forceFeedback = (float)PlayerPrefs.GetInt("ForceToggle", 1);
    }

    private void Start()
    {
        StartThread();
    }

    void OnApplicationQuit()
    {
        if (serverRunning || threadRunning)
        {
            Debug.Log("Quit~!");
            StopThread();
        }
    }

    public void StartThread()
    {
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    public void StopThread()
    {
        if (serverRunning)
        {
            stop = true;
        }
        else
        {
            thread.Abort();
        }
        newsock.Close();
    }

    private void ThreadMethod()
    {
        threadRunning = true;
        int recv;
        byte[] dataRecv = new byte[recvSize];  //Data Received from Simulink
        byte[] dataSend = new byte[32]; //Send Collision Status, X, Y
        IEnumerable<byte> dataSendLINQ = new byte[32]; //Initialize LINQ for easy concatenation later for sending
        //Create IP End point, where I want to connect (Local IP/Port)
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        //Create UDP Socket
        newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //Bind to ip. Server waits for a client at specified ip & port. 
        try
        {
            newsock.Bind(ipep);
        }
        catch (Exception e)
        {
            Debug.Log("Winsock Error: " + e.ToString());
        }
        Debug.Log("Connecting to IP: "+ ipAddress + " Port: "+ port +" Waiting for a client...");

        //Get IP of client
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        Debug.Log("1");
        EndPoint Remote = (EndPoint)(sender);
        Debug.Log("2");
        //Receive binary Data from client
        recv = newsock.ReceiveFrom(dataRecv, ref Remote);
        Debug.Log("3");
        //Decode data and display
        serverRunning = true;
        Debug.Log("Message received from " + Remote.ToString());

        while (true)
        {
            //Receive X Y positions
            dataRecv = new byte[recvSize];
            recv = newsock.ReceiveFrom(dataRecv, ref Remote);


            //Convert Bytes into Doubles (This gets referenced by mainPlayer.cs to move the character) X/Y Position
            xMove = (float)BitConverter.ToDouble(dataRecv, 0); //x
            yMove = (float)BitConverter.ToDouble(dataRecv, 8); //y
            zMove = (float)BitConverter.ToDouble(dataRecv, 16); //z


            //Debug.Log("X: " +  xMove + " Y: " + yMove + "Z: " + zMove); //Display X/Y Position

            //Concatenate Collision Status, ForceFeedbackStatus, xForce, yForce. 
            dataSendLINQ = (BitConverter.GetBytes((double)collisionStatus)).Concat(BitConverter.GetBytes((double)forceFeedback)).Concat(BitConverter.GetBytes((double)xForce)).Concat(BitConverter.GetBytes((double)yForce));
            dataSend = dataSendLINQ.ToArray(); //Convert to byte Array from IEnumerable byte Array

            //Debug.Log("X_f: " + xForce + " Y_f: " + yForce); //xForce, yForce

            //Send Info to Simulink
            newsock.SendTo(dataSend, dataSend.Length, SocketFlags.None, Remote);

            if (stop)
            {
                break;
            }

            //Debug.Log(dataSend[0] + " " + dataSend[1] + " " + dataSend[2] + " " + dataSend[3] + " " + dataSend[4]); 
        }
        Debug.Log("Exiting Thread...");
    }
}
