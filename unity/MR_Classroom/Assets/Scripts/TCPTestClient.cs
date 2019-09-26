using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    Dictionary<GameObject,Vector3> current_positions;
    Dictionary<GameObject, float> t0;
    Dictionary<GameObject, float> t1;

    public SimulationController sim_controller;
    public List<OrganelleSpawn> spawn_points;


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
            grabbableObjects.Add(objects[i].GetComponent<OrganelleController>().objectId, objects[i]);
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
                JSONArray current_spawn = current_data["spawn"].AsArray;
                for (int a = 0; a < current_ids.Count; a++)
                {
                        if(current_ids[a] == 47 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Animal;
                        sim_controller.StartSimulation();
                        simulationStarted = true;

                    }
                        else if(current_ids[a] == 48 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Plant;
                        sim_controller.StartSimulation();
                        simulationStarted = true;
                      
                    }
                        else if(current_ids[a] == 49 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Prokaryotic;
                        sim_controller.StartSimulation();
                        simulationStarted = true;
                    }
                    else if(current_ids[a] != 47 && current_ids[a] != 48 && current_ids[a] != 49 && grabbableObjects[current_ids[a]].activeSelf == false && grabbableObjects[current_ids[a]].GetComponent<OrganelleController>().locked == false)
                    {
                            grabbableObjects[current_ids[a]].GetComponent<OrganelleController>().locked = true;
                            int min_amount = 100;
                            int min_index = -1;
                            List<int> spawn_indices = new List<int>();
                            for (int b = 0; b < spawn_points.Count; b++)
                            {
                                if (spawn_points[b].organellesActive < min_amount)
                                {
                                    min_index = b;
                                    min_amount = spawn_points[b].organellesActive;
                                }
                                if (spawn_points[b].organellesActive == 0)
                                {
                                    spawn_indices.Add(b);
                                }
                            }

                        if(spawn_indices.Count > 0)
                        {
                            int random_index = UnityEngine.Random.Range(0, spawn_indices.Count);

                            Debug.Log("Is it here at least:" + current_ids[a]);

                            spawn_points[spawn_indices[random_index]]._organelleToSpawn = grabbableObjects[current_ids[a]].GetComponent<OrganelleController>();
                            spawn_points[spawn_indices[random_index]].ActivatePortal();
                        }
                        else
                        {
                            spawn_points[min_index]._organelleToSpawn = grabbableObjects[current_ids[a]].GetComponent<OrganelleController>();
                            spawn_points[min_index].ActivatePortal();
                        }
                          

                        //grabbableObjects[current_ids[a]].SetActive(true);
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
                if (current.transform.position == current_positions[current])
                {
                    current.GetComponent<OrganelleController>().lastGrabLoop++;
                }
                else
                {
                    current.GetComponent<OrganelleController>().lastGrabLoop = 0;
                    if (current.GetComponent<OrganelleController>().grabLooping == false)
                    {
                        current.GetComponent<OrganelleController>().grabLooping = true;
                        current.GetComponent<OrganelleController>().OnGrabStarted();
                    }
                }

                float t = dt / (t1[current] - t0[current]);
                current.transform.position = Vector3.Lerp(current.transform.position, current_positions[current], t);

                if(current.GetComponent<OrganelleController>().lastGrabLoop > 5 && current.GetComponent<OrganelleController>().grabLooping == true)
                {
                    current.GetComponent<OrganelleController>().OnGrabFinished();
                    current.GetComponent<OrganelleController>().lastGrabLoop = 0;
                    current.GetComponent<OrganelleController>().grabLooping = false;

                    //Release Message:

                    JSONNode node = new JSONObject();
                    node["type"] = "release";
                    node["uid"] = current.GetComponent<OrganelleController>().objectId;
                    SendTCPMessage(node.ToString());
                }
                
            }
            if (current.GetComponent<OrganelleController>().hasBeenGrabbed == true && !current.GetComponent<OrganelleController>().isGrabbed())
            {
                //current.GetComponent<OrganelleController>().OnGrabFinished();
                current.GetComponent<OrganelleController>().hasBeenGrabbed = false;

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
