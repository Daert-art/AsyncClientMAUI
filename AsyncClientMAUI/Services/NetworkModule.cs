using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkModule
{
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private TcpClient tcpClient;

    public NetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
    {
        this.logAction = logAction;
        this.messageReceivedAction = messageReceivedAction;        
    }

    public async void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, port);
            logAction("Connected to server.");

            var stream = tcpClient.GetStream();
            byte[] receiveBuffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);

            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                messageReceivedAction(receivedMessage);
            }
        }
        catch (Exception ex)
        {
            logAction($"Error in ConnectToServer: {ex.Message}");
        }
    }

    public async void SendMessage(string message)
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            logAction("Not connected to server.");
            return;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var stream = tcpClient.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length);
            logAction($"Sent {buffer.Length} bytes to server.");
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }
}
