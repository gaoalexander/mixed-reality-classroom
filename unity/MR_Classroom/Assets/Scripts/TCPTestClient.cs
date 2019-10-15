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
    Dictionary<GameObject, Vector3> current_positions;
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

    public bool playLocally = false;

    public bool tracking = true;

    public int simToLaunch = -1;

    private void myLog(string message)
    {
        Debug.Log(message);
    }

    public void InitTcpClient()
    {
        simulationStarted = false;
        grabbableObjects = new Dictionary<int, GameObject>();
        current_positions = new Dictionary<GameObject, Vector3>();
        t0 = new Dictionary<GameObject, float>();
        t1 = new Dictionary<GameObject, float>();

        for (int i = 0; i < objects.Length; i++)
        {
            //TODO: set the id of the obj not the type here....
            grabbableObjects.Add(objects[i].GetComponent<OrganelleController>().objectId, objects[i]);
            objects[i].GetComponent<OrganelleController>().SetOriginalScale();
        }

        if (!playLocally)
        {
            ConnectToTcpServer();
        }
        else
        {
            //gameObject.SetActive(false);
            sim_controller.StartSimulation(simToLaunch);
        }
    }

    private void Update()
    {
        if (!playLocally)
        {
            int deactivatedId = -1;
            int releaseId = -1;

            if (IsConnected && message != null && message.Length > 0)
            {
                //TODO: CHECK THE MESSAGES WE ARE RECEIVING. 
                //Debug.Log("HERE IS THE SERVER MESSAGE :" + message);
                JSONNode current_data = JSON.Parse(message);

                //Debug.Log("!!!!!!!" + current_data["type"]);
                if (current_data["type"] == "active")
                {
                    JSONArray current_ids = current_data["ids"].AsArray;
                    JSONArray current_spawn = current_data["spawn"].AsArray;
                    for (int a = 0; a < current_ids.Count; a++)
                    {

                        InterpretMarker(current_ids[a], current_spawn[a]);

                    }
                }
                else if (current_data["type"] == "initialize")
                {
                    Debug.Log("###Initialize:");
                    Debug.Log(current_data.ToString());
                    if (current_data["sim"].AsInt == 47 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Animal;
                        sim_controller.StartSimulation(0);
                        simulationStarted = true;

                    }
                    else if (current_data["sim"].AsInt == 48 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Plant;
                        sim_controller.StartSimulation(0);
                        simulationStarted = true;

                    }
                    else if (current_data["sim"].AsInt == 49 && !simulationStarted)
                    {
                        sim_controller.currentCell = SimulationController.TypeOfCell.Prokaryotic;
                        sim_controller.StartSimulation(0);
                        simulationStarted = true;
                    }
                        for(int i = 0; i < current_data.Count; i++)
                        {
                            if (current_data[i]["uid"] != null && grabbableObjects.ContainsKey(current_data[i]["uid"]))
                            {
                            
                                int uid = current_data[i]["uid"].AsInt;
                                float x = current_data[i]["x"].AsFloat;
                                float y = current_data[i]["y"].AsFloat;
                                float z = current_data[i]["z"].AsFloat;
                                GameObject current = grabbableObjects[uid];
                                current.SetActive(true);
                                current.transform.position =  new Vector3(x, y, z);
                            }   
                        }
                   


                }
                else if (current_data["type"] == "deactivate")
                {
                    deactivatedId = current_data["eventid"].AsInt;
                }
                else if (current_data["type"] == "release")
                {
                    releaseId = current_data["eventid"].AsInt;
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
                message = null;
            }

            dt += Time.deltaTime;
            foreach (KeyValuePair<int, GameObject> entry in grabbableObjects)
            {
                GameObject current = entry.Value;
                if (t1.ContainsKey(current))
                {
                    /*if (current.transform.position == current_positions[current])
                    {
                        //used to define that grabbing has stopped when the object hasn't moved for a certain period of time
                        //instead, it should be the server the one sending a stop grabbing
                        current.GetComponent<OrganelleController>().lastGrabLoop++;
                    }
                    else
                    {
                        current.GetComponent<OrganelleController>().lastGrabLoop = 0;*/
                    if (current.GetComponent<OrganelleController>().grabLooping == false)
                    {
                        current.GetComponent<OrganelleController>().grabLooping = true;
                        current.GetComponent<OrganelleController>().OnGrabStarted();
                    }
                    //}

                    float t = dt / (t1[current] - t0[current]);
                    current.transform.position = Vector3.Lerp(current.transform.position, current_positions[current], t);
                }
                //we might not need this anymore
                /*if (current.GetComponent<OrganelleController>().hasBeenGrabbed == true && !current.GetComponent<OrganelleController>().isGrabbed())
                {
                    //current.GetComponent<OrganelleController>().OnGrabFinished();
                    current.GetComponent<OrganelleController>().hasBeenGrabbed = false;
                }*/
                //this condition should come from the server
                if (current.GetComponent<OrganelleController>().objectId == releaseId && current.GetComponent<OrganelleController>().grabLooping == true)
                {
                    current.GetComponent<OrganelleController>().grabLooping = false;
                    current.GetComponent<OrganelleController>().OnGrabFinished();
                    //current.GetComponent<OrganelleController>().lastGrabLoop = 0;
                }
                if (current.GetComponent<OrganelleController>().objectId == deactivatedId)
                {
                    current.GetComponent<OrganelleController>().Deactivate();
                }
            }
        }
    }

    public void GrabReleased(int organelleId)
    {
        JSONNode node = new JSONObject();
        node["type"] = "release";
        node["uid"] = organelleId;
        SendTCPMessage(node.ToString());
    }

    public void SetObjectInactive(int organelleId)
    {
        JSONNode node = new JSONObject();
        node["type"] = "deactivate";
        node["uid"] = organelleId;
        SendTCPMessage(node.ToString());
    }

    public void SetSpawnIds(int[] ids)
    {
        JSONNode node = new JSONObject();
        node["type"] = "active";
        string ids_array = "[";
        for (int i = 0; i < ids.Length; i++)
        {
            if (i == ids.Length - 1)
            {
                ids_array += ids[i].ToString() + "]";
                break;
            }
            ids_array += ids[i].ToString() + ",";
        }
        node["ids"] = ids_array;
        SendTCPMessage(node.ToString());
    }


    public void InterpretMarker(int markerId, int spawnId)
    {

        if (markerId == 47 && !simulationStarted)
        {
            sim_controller.currentCell = SimulationController.TypeOfCell.Animal;
            sim_controller.StartSimulation(0);
            simulationStarted = true;

        }
        else if (markerId == 48 && !simulationStarted)
        {
            sim_controller.currentCell = SimulationController.TypeOfCell.Plant;
            sim_controller.StartSimulation(0);
            simulationStarted = true;

        }
        else if (markerId == 49 && !simulationStarted)
        {
            sim_controller.currentCell = SimulationController.TypeOfCell.Prokaryotic;
            sim_controller.StartSimulation(0);
            simulationStarted = true;
        }
        else if (grabbableObjects.ContainsKey(markerId) && grabbableObjects[markerId].activeSelf == false && grabbableObjects[markerId].GetComponent<OrganelleController>().locked == false)
        {
            grabbableObjects[markerId].GetComponent<OrganelleController>().locked = true;
            //from here, it will be handled from the server
            if (playLocally)
            {
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

                if (spawn_indices.Count > 0)
                {
                    int random_index = UnityEngine.Random.Range(0, spawn_indices.Count);

                    //Debug.Log("Is it here at least:" + markerId);

                    spawn_points[spawn_indices[random_index]]._organelleToSpawn = grabbableObjects[markerId].GetComponent<OrganelleController>();
                    spawn_points[spawn_indices[random_index]].ActivatePortal();
                }
                else
                {
                    spawn_points[min_index]._organelleToSpawn = grabbableObjects[markerId].GetComponent<OrganelleController>();
                    spawn_points[min_index].ActivatePortal();
                }
            }
            else
            {
                //only call this when the spawn index comes from the server
                spawn_points[spawnId]._organelleToSpawn = grabbableObjects[markerId].GetComponent<OrganelleController>();
                spawn_points[spawnId].ActivatePortal();
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
        //SendTCPMessage("!disconnect");
        running = false;
    }

    public void MessageReceived(TCPTestServer.ServerMessage serverMessage)
    {

        //OnMessageReceived(serverMessage);
    }

    /// <summary>   
    /// Send message to server using socket connection.     
    /// </summary>  
    public void SendTCPMessage(string clientMessage)
    {
        if (socketConnection != null && socketConnection.Connected && clientMessage.Length > 0)
        {
            try
            {
                // Get a stream object for writing.             
                NetworkStream writestream = socketConnection.GetStream();
                if (writestream.CanWrite)
                {
                    // Convert string message to byte array.                 
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage + "`");
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
        if (!playLocally)
        {
            stream.Close();
            socketConnection.Close();
            clientReceiveThread.Join();
            CloseConnection();
        }
    }
}
