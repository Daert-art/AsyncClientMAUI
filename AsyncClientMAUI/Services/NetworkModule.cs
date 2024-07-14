using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ClientNetworkModule
{
    private Action<string> logAction;
    private Action<string> messageReceivedAction;
    private TcpClient tcpClient;
    private NetworkStream stream;

    public ClientNetworkModule(Action<string> logAction, Action<string> messageReceivedAction)
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

            stream = tcpClient.GetStream();
            byte[] receiveBuffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length)) > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                messageReceivedAction(receivedMessage);

                if (receivedMessage.Trim().Equals("Bye", StringComparison.OrdinalIgnoreCase))
                {
                    logAction("Connection closed by server.");
                    break;
                }
            }

            tcpClient.Close();
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
            await stream.WriteAsync(buffer, 0, buffer.Length);
            logAction($"Sent {buffer.Length} bytes to server.");
        }
        catch (Exception ex)
        {
            logAction($"Error in SendMessage: {ex.Message}");
        }
    }
}
