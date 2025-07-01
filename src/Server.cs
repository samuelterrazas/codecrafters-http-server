using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

internal sealed class Server
{
    public static async Task HandleClientConnection(Socket socket)
    {
        byte[] receiveBuffer = new byte[4096];

        int bytesReceived;
        while ((bytesReceived = await socket.ReceiveAsync(receiveBuffer)) > 0)
        {
            try
            {
                string rawRequest = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
            
                Console.WriteLine("REQUEST:\n{0}", rawRequest);
            
                (byte[] Buffer, bool CloseConnection) response = await HandleRequest(rawRequest);
                int bytesSent = await socket.SendAsync(response.Buffer);
                string rawResponse = Encoding.UTF8.GetString(response.Buffer, 0, bytesSent);

                Console.WriteLine("RESPONSE:\n{0}", rawResponse);

                if (!response.CloseConnection)
                {
                    continue;
                }
                
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (SocketException socketEx)
            {
                Console.WriteLine("SocketException: {0}", socketEx);
            }
        }
    }

    private static async Task<(byte[] Buffer, bool CloseConnection)> HandleRequest(string rawRequest)
    {
        var request = new HttpRequestMessage(rawRequest);
        var response = new HttpResponseMessage();

        switch (request.Method)
        {
            case "GET":
                await HandleGetRequest(request, response);
                break;
            case "POST":
                await HandlePostRequest(request, response);
                break;
            default:
                response.StatusCode = HttpStatusCode.MethodNotAllowed;
                break;
        }

        if (request.CloseConnection)
        {
            response.CloseConnection = true;
        }
        
        return (response.ToBytes(), request.CloseConnection);
    }

    private static async Task HandleGetRequest(HttpRequestMessage request, HttpResponseMessage response)
    {
        switch (request.RequestTarget)
        {
            case "/": break;
            case not null when request.RequestTarget.StartsWith("/echo/", StringComparison.Ordinal):
            {
                string message = request.GetRequestTargetSegmentOrDefault(1);
                int contentLength;
                byte[] responseBody;

                string acceptEncodingHeader = request.Headers.GetValueOrDefault("Accept-Encoding", "identity");
                string[] acceptedEncodings = acceptEncodingHeader.Split(',', StringSplitOptions.TrimEntries);
                if (acceptedEncodings.Contains("gzip", StringComparer.Ordinal))
                {
                    response.ContentEncodingHeader = "gzip";

                    byte[] compressedContent;
                    await using (var memoryStream = new MemoryStream())
                    {
                        await using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                        {
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            gzipStream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        
                        compressedContent = memoryStream.ToArray();
                    }
                    
                    contentLength = compressedContent.Length;
                    responseBody = compressedContent;
                }
                else
                {
                    contentLength = message.Length;
                    responseBody = Encoding.UTF8.GetBytes(message);
                }

                response.ContentTypeHeader = "text/plain";
                response.ContentLengthHeader = contentLength;
                response.Body = responseBody;
                break;
            }
            case "/user-agent":
            {
                string userAgentHeader = request.Headers.GetValueOrDefault("User-Agent", string.Empty);

                response.ContentTypeHeader = "text/plain";
                response.ContentLengthHeader = userAgentHeader.Length;
                response.Body = Encoding.UTF8.GetBytes(userAgentHeader);
                break;
            }
            case not null when request.RequestTarget.StartsWith("/files/", StringComparison.Ordinal):
            {
                string fileName = request.GetRequestTargetSegmentOrDefault(1);
                string filePath = BuildPath(fileName);
                if (!File.Exists(filePath))
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    break;
                }
                
                string content = await File.ReadAllTextAsync(filePath);

                response.ContentTypeHeader = "application/octet-stream";
                response.ContentLengthHeader = content.Length;
                response.Body = Encoding.UTF8.GetBytes(content);
                break;
            }
            default:
                response.StatusCode = HttpStatusCode.NotFound;
                break;
        }
    }

    private static async Task HandlePostRequest(HttpRequestMessage request, HttpResponseMessage response)
    {
        switch (request.RequestTarget)
        {
            case not null when request.RequestTarget.StartsWith("/files/", StringComparison.Ordinal):
            {
                string fileName = request.GetRequestTargetSegmentOrDefault(1);
                string filePath = BuildPath(fileName);
                byte[] content = Encoding.UTF8.GetBytes(request.Body);
                        
                await File.WriteAllBytesAsync(filePath, content);

                response.StatusCode = HttpStatusCode.Created;
                break;
            }
            default:
                response.StatusCode = HttpStatusCode.NotFound;
                break;
        }
    }
    
    private static string BuildPath(string fileName)
    {
        string[] paths = [Path.GetTempPath(), "data", "codecrafters.io", "http-server-tester", fileName];
        return Path.Combine(paths);
    }
}