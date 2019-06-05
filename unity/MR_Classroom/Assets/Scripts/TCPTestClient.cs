using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPTestClient : MonoBehaviour
{
    public Action<TCPTestClient> OnConnected = delegate { };
    public Action<TCPTestClient> OnDisconnected = delegate { };
    public Action<string> OnLog = delegate { };
    public Action<TCPTestServer.ServerMessage> OnMessageReceived = delegate { };

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
        ConnectToTcpServer();
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
                        length = stream.Read(bytes, 0, bytes.Length);
                        if (length != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            // Convert byte array to string message.                        
                            string serverJson = Encoding.ASCII.GetString(incomingData);
                            TCPTestServer.ServerMessage serverMessage = JsonUtility.FromJson<TCPTestServer.ServerMessage>(serverJson);
                            MessageReceived(serverMessage);
                        }
                    }
                }
            }
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
        OnMessageReceived(serverMessage);
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
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
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
        clientReceiveThread.Join();
    }
}