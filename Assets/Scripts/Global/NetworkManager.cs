using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkManager : MonoBehaviour
{
    //Host
    //Client
    //Null
    public string mode { get; private set; }
    private int port;
    private SocketServer socketServer;
    private SocketClient socketClient;
    private IPAddress localIP;
    private bool started;

    private void Awake()
    {
        //0: Listen Port 1: Send Port
        port = 8965;
        started = false;
        getLocalIP();
    }

    private void getLocalIP()
    {
        //TODO:
        //find the correct local ip automatically

        //temp
        localIP = IPAddress.Parse("127.0.0.1");
    }

    private void setup()
    {
        socketServer = new SocketServer(localIP, port, this);
    }

    public void Startup()
    {
        if(!started && mode != "Null")
        {
            started = true;
            setup();
        }
    }

    public void setMode(string m)
    {
        mode = m;
    }

    private void connectSocket(IPAddress ip)
    {
        Debug.Log("Declaring Client Socket");
        socketClient = new SocketClient(localIP, ip, port);
    }

    public void connectSocket(string ip)
    {
        Debug.Log("Connect Socket Mode: " + mode);
        if (mode == "Client")
        {
            IPAddress remoteIP = IPAddress.Parse(ip);
            connectSocket(remoteIP);
        }
    }

    public void onNewConnection(IPAddress ip)
    {
        if(mode == "Host")
        {
            connectSocket(ip);
        }
    }

    public void disconnectSocket()
    {
        Debug.Log("Disconnecting Client Socket...");
        socketClient = null;
    }

    private void OnApplicationQuit()
    {
        if(socketServer != null)
        {
            socketServer.exit();
        }
    }

    public string sendMessage(string msg)
    {
        if (socketClient != null)
        {
            return socketClient.sendMessage(msg);
        } else
        {
            Debug.Log("Error sending message client socket is null");
            return "socketClient is null";
        }
    }
}

class SocketClient
{
    Socket sender;
    IPAddress ipAddr;
    IPAddress remoteAddr;
    int port;
    public SocketClient(IPAddress ipAddr, IPAddress remoteAddr, int port)
    {
        this.ipAddr = ipAddr;
        this.remoteAddr = remoteAddr;
        this.port = port;
    }

    private void connect()
    {
        IPEndPoint endPoint = new IPEndPoint(remoteAddr, port);

        Debug.Log("Client IP: " + ipAddr);
        Debug.Log("Server IP: " + remoteAddr);

        sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            sender.Connect(endPoint);
        }
        catch (Exception e)
        {
            Debug.Log("Network Error: " + e.ToString());
        }
    }

    public string sendMessage(string msg)
    {
        connect();
        try
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            int byteSent = sender.Send(msgBytes);
            Debug.Log("Message Sent: " + msg);
            return receiveResponse();
        }
        catch(Exception e)
        {
            Debug.Log("Error Sending Message to Server: " + e.ToString());
            return "Error Sending Message to Server: " + e.ToString();
        }
    }

    private string receiveResponse()
    {
        try
        {
            byte[] receiveMsg = new byte[1024];
            int byteRecv = sender.Receive(receiveMsg);
            string msgReceived = Encoding.ASCII.GetString(receiveMsg, 0, byteRecv);
            Debug.Log("Response Received: " + msgReceived);
            closeConnection();
            return msgReceived;
        }
        catch(Exception e)
        {
            Debug.Log("Error Receiving Response: " + e.ToString());
            return "Error Receiving Response: " + e.ToString();
        }
    }

    private void closeConnection()
    {
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}

class SocketServer
{
    Socket listener;
    Thread listenThread;
    NetworkManager networkManager;
    bool breakLoop;
    bool firstConnection;
    public SocketServer(IPAddress ipAddr, int port, NetworkManager manager)
    {
        firstConnection = true;
        networkManager = manager;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);

        listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);
            breakLoop = false;

            listenThread = new Thread(() =>
            {
                listen();
            });
            listenThread.Start();
        }
        catch(Exception e)
        {
            Debug.Log("Error in Socket Server: " + e);
        }
    }

    private void listen()
    {
        try
        {
            while (!breakLoop)
            {
                Debug.Log("Server waiting for connection...");
                Socket clientSocket = listener.Accept();

                if(firstConnection)
                {
                    firstConnection = false;
                    IPEndPoint endPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    networkManager.onNewConnection(endPoint.Address);
                }

                byte[] bytes = new byte[1024];
                string data = null;

                while (true)
                {
                    int numByte = clientSocket.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, numByte);

                    if (data.IndexOf("</MSG>") > -1) break;
                }

                string msg = data.Replace("</MSG>", "");
                Debug.Log("Server Recieved Message: " + msg);

                reply(clientSocket);
            }
        }
        catch(Exception e)
        {
            Debug.Log("Server Error Listening: " + e);
        }
    }

    private void reply(Socket clientSocket)
    {
        byte[] msg = Encoding.ASCII.GetBytes("Message Recieved");
        clientSocket.Send(msg);

        clientExit(clientSocket);
    }

    private void clientExit(Socket clientSocket)
    {
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }

    public void exit()
    {
        breakLoop = true;
        listener.Shutdown(SocketShutdown.Both);
        listener.Close();

        listenThread.Join();
    }
}