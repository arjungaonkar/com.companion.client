using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;

public class LiteNetLibTest : MonoBehaviour, INetEventListener
{
    private NetManager _server;
    private NetManager _client;
    private NetPeer _clientPeer;

    private bool _isRunning = true;

    void Start()
    {
        Debug.Log("Starting LiteNetLib IL2CPP Test...");

        // Start Server
        _server = new NetManager(this);
        _server.Start(9050);
        Debug.Log("Server started on port 9050");

        // Start Client
        _client = new NetManager(this);
        _client.Start();
        _clientPeer = _client.Connect("127.0.0.1", 9050, "TestKey");
        Debug.Log("Client attempting to connect...");

        // Run networking loop in a separate thread
        Thread netThread = new Thread(NetworkLoop);
        netThread.Start();
    }

    private void NetworkLoop()
    {
        while (_isRunning)
        {
            _server.PollEvents();
            _client.PollEvents();
            Thread.Sleep(15); // Prevent 100% CPU usage
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[SERVER] Client connected: " + peer.EndPoint);

        // Send a message from Client to Server
        NetDataWriter writer = new NetDataWriter();
        writer.Put("Hello from Client!");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        string message = reader.GetString();
        Debug.Log("[SERVER] Received: " + message);
        reader.Recycle();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[SERVER] Client disconnected: " + peer.EndPoint);
    }

    public void OnNetworkError(IPEndPoint endPoint, int socketErrorCode) 
    {
        Debug.LogError("[ERROR] Network error: " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public void OnConnectionRequest(ConnectionRequest request) 
    {
        request.Accept();
    }

    void OnDestroy()
    {
        _isRunning = false;
        _server?.Stop();
        _client?.Stop();
    }
}
