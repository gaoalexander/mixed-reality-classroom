using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class TCPTestClient : MonoBehaviour
{
    public Action<TCPTestClient> OnConnected = delegate { };
    public Action<TCPTestClient> OnDisconnected = delegate { };
    public Action<string> OnLog = delegate { };
    public Action<TCPTestServer.ServerMessage> OnMessageReceived = delegate { };
    string message; 

    public GameObject[] objects;
    Dictionary<int, GameObject> grabbableObjects;
    public GameObject plasmaMembrane;
    Dictionary<GameObject,Vector3> current_positions;
    Dictionary<GameObject, float> t0;
    Dictionary<GameObject, float> t1;

    bool simulationStarted;

    float dt;

    public int id;

    public bool IsConnected
    {
        get { return socketConnection != null && socketConnection.Connected; }
    }

    public string IPAddress = "192.168.43.142";
    public int Port = 8052;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private NetworkStream stream;
    private bool running;


    private void myLog(string message)
    {
        Debug.Log(message);
    }

    private void Start()
    {
        simulationStarted = false;
        grabbableObjects = new Dictionary<int, GameObject>();
        current_positions = new Dictionary< GameObject,Vector3 >();
        t0 = new Dictionary<GameObject, float>();
        t1 = new Dictionary<GameObject, float>();


        for (int i = 0; i < objects.Length; i++)
        {
            //TODO: set the id of the obj not the type here....
            grabbableObjects.Add(objects[i].GetComponent<Grab>().objectId, objects[i]);
        }
        ConnectToTcpServer();
    }

    private void Update()
    {
        if(IsConnected && message != null)
        {
            //Debug.Log("HERE IS THE SERVER MESSAGE :" + message);
            JSONNode current_data = JSON.Parse(message);

            //Debug.Log("!!!!!!!" + current_data["type"]);
            if (current_data["type"] == "active")
            {
                JSONArray current_ids = current_data["ids"].AsArray;
                for (int a = 0; a < current_ids.Count; a++)
                {
                    if (current_ids[a].AsInt == 0)
                    {
                        plasmaMembrane.SetActive(true);
                    }
                    else
                    {
                        grabbableObjects[current_ids[a]].SetActive(true);
                    }
                }
            }
            else
            {
                //Loop through all objects and update them...
                for (int i = 0; i < current_data.Count; i++)
                {

                    //Debug.Log(current_data[i]);
                    if (current_data[i]["uid"] != null)
                    {
                        //Debug.Log("Updating all objects");
                        int uid = current_data[i]["uid"].AsInt;
                        float x = current_data[i]["x"].AsFloat;
                        float y = current_data[i]["y"].AsFloat;
                        float z = current_data[i]["z"].AsFloat;
                        GameObject current = grabbableObjects[uid];
                        
                        current_positions[current] = new Vector3(x, y, z);
                        if (t1.ContainsKey(current))
                        {
                            t0[current] = t1[current];
                            t1[current] = dt;
                        }
                        else
                        {
                            t0[current] = 0;
                            t1[current] = dt;
                        }
                        //current.transform.position = Vector3.Lerp(current.transform.position, new Vector3(x, y, z),);
                        //Debug.Log(string.Format("updated {0} position: {1}, {2}, {3}", uid, x, y, z));
                    }

                }
            }
           
        }

        dt += Time.deltaTime;
        foreach (KeyValuePair<int, GameObject> entry in grabbableObjects)
        {
            GameObject current = entry.Value;
            if (t1.ContainsKey(current))
            {
                float t = dt / (t1[current] - t0[current]);
                current.transform.position = Vector3.Lerp(current.transform.position, current_positions[current], t);
            }
            if (!current.GetComponent<Grab>().isGrabbed())
            {
                current.GetComponent<OrganelleController>().OnGrabFinished();
            }
        }

    }


    /// <summary>   
    /// Setup socket connection.    
    /// </summary>  
    public void ConnectToTcpServer()
    {
        try
        {
            OnLog(string.Format("Connecting to {0}:{1}", IPAddress, Port));
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            OnLog += myLog;
        }
        catch (Exception e)
        {
            OnLog("On client connect exception " + e);
        }
    }

    /// <summary>   
    /// Runs in background clientReceiveThread; Listens for incoming data.  
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(IPAddress, Port);
            OnConnected(this);
            OnLog("Connected :)");

            Byte[] bytes = new Byte[1024];
            running = true;
            while (running)
            {
                // Get a stream object for reading
                using (stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incoming stream into byte array.                    
                    while (running && stream.CanRead)
                    {
                        //Debug.Log("BEFORE READING:");
                        length = stream.Read(bytes, 0, bytes.Length);
                        //Debug.Log("AFTER READING:");
                        if (length != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            // Convert byte array to string message.                        
                            message = Encoding.ASCII.GetString(incomingData);
                            //TCPTestServer.ServerMessage serverMessage = JsonUtility.FromJson<TCPTestServer.ServerMessage>(serverJson);

                        }
                    }
                }
            }
            stream.Close();
            socketConnection.Close();
            OnLog("Disconnected from server");
            OnDisconnected(this);
        }
        catch (SocketException socketException)
        {
            OnLog("Socket exception: " + socketException);
        }
    }

    public void CloseConnection()
    {
        SendTCPMessage("!disconnect");
        running = false;
    }

    public void MessageReceived(TCPTestServer.ServerMessage serverMessage)
    {
       
        //OnMessageReceived(serverMessage);
    }

    /// <summary>   
    /// Send message to server using socket connection.     
    /// </summary>  
    public void SendTCPMessage(string clientMessage = "test")
    {
        if (socketConnection != null && socketConnection.Connected)
        {
            try
            {
                // Get a stream object for writing.             
                NetworkStream writestream = socketConnection.GetStream();
                if (writestream.CanWrite)
                {
                    // Convert string message to byte array.                 
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage+"`");
                    // Write byte array to socketConnection stream.                 
                    writestream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                    OnLog("sending:" + clientMessage);
                    //return true;
                }
            }
            catch (SocketException socketException)
            {
                OnLog("Socket exception: " + socketException);
            }
        }

        //return false;
    }

    public virtual void OnSentMessage(string message)
    {
    }

    public void OnApplicationQuit()
    {
        stream.Close();
        socketConnection.Close();
        clientReceiveThread.Join();
        CloseConnection();
    }
}