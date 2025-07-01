using System.Net;
using System.Net.Sockets;
using codecrafters_http_server;

const int port = 4221;

TcpListener? listener = null;
try
{
    listener = new TcpListener(IPAddress.Any, port);
    listener.Start();
    
    while (true)
    {
        Console.WriteLine("Waiting for connection...");
        
        Socket socket = await listener.AcceptSocketAsync();
        Console.WriteLine("Connection accepted from {0}", socket.RemoteEndPoint);
        
        _ = Task.Run(() => Server.HandleClientConnection(socket));
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
finally
{
    listener?.Stop();
}