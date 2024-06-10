using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CatalogueAvalonia.Services.Messeges;
using CommunityToolkit.Mvvm.Messaging;

namespace CatalogueAvalonia.Services.BarcodeServer;

public class Listener
{
    private const int ListenPort = 58760;
    private readonly UdpClient _listener = new UdpClient(ListenPort);
    private IMessenger _messenger;
    private readonly TcpServer _tcpServer;

    public Listener(IMessenger messenger, TcpServer tcpServer)
    {
        _messenger = messenger;
        _tcpServer = tcpServer;
    }

    public async Task StartListener()
    {
        var listenerRes = Task.Run(async () => await StartListenerAsync());
        
    }

    private async Task StartListenerAsync()
    {
        IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, ListenPort);
        var server = Task.Run(async () => await _tcpServer.StartTcpServer());
        
        _messenger.Send(new ServerMessage($"Сервер запущен."));
        try
        {
            while (true)
            {
                byte[] bytes = _listener.Receive(ref groupEP);
                var value = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                _messenger.Send(new ServerMessage(
                    $"Received broadcast from {groupEP}: {value}"));

                if (value.Contains("c533298572f02ab76227a31bb04faaf6"))
                {
                    byte[] sendbuf = Encoding.ASCII.GetBytes($"{_tcpServer.Port}");
                    IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, 58760);
                    _listener.Send(sendbuf, ep);
                }
            }
        }
        catch (SocketException ex) when (ex.ErrorCode == 10004)
        {

        }
        catch (SocketException e)
        {
            _messenger.Send(new ServerMessage($"{e}"));
        }
        finally
        {
            _listener.Close();
        }

        
    }

    public async Task StopListener()
    {
        _listener.Close();
        _messenger.Send(new ServerMessage($"Сервер остановлен."));
    }
}