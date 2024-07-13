using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkModule
{
    private Socket socket;
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private byte[] receiveBuffer;

    public NetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
    {
        this.logAction = logAction;
        this.messageReceivedAction = messageReceivedAction;
        receiveBuffer = new byte[1024];
    }

    public void StartServer(string ipAddress, int port)
    {
        try
        {
            IPEndPoint endP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endP);
            socket.Listen(10);
            socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            logAction("Server started...");
        }
        catch (Exception ex)
        {
            logAction($"Error starting server: {ex.Message}");
        }
    }

    public void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), null);
        }
        catch (Exception ex)
        {
            logAction($"Error connecting to server: {ex.Message}");
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Socket serverSocket = socket;
            Socket clientSocket = serverSocket.EndAccept(ar);
            logAction($"Client connected: {clientSocket.RemoteEndPoint}");

            clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }
        catch (Exception ex)
        {
            logAction($"Error in AcceptCallback: {ex.Message}");
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            socket.EndConnect(ar);
            logAction("Connected to server");

            socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        catch (Exception ex)
        {
            logAction($"Error in ConnectCallback: {ex.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            int bytesRead = clientSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                string message = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
                messageReceivedAction(message);

                if (message.Trim().ToLower() != "bye")
                {
                    
                    clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
                }
                else
                {
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    logAction("Connection closed by server.");
                }
            }
            else
            {
                logAction("No data received or socket is closed");
            }
        }
        catch (Exception ex)
        {
            logAction($"Error in ReceiveCallback: {ex.Message}");
        }
    }

    public void SendMessage(string message)
    {
        try
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }


    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            if (clientSocket.Connected)
            {
                int bytesSent = clientSocket.EndSend(ar);
                logAction($"Sent {bytesSent} bytes to server");
            }
            else
            {
                logAction("Socket is not connected.");
            }
        }
        catch (Exception ex)
        {
            logAction($"Error in SendCallback: {ex.Message}");
        }
    }
}
