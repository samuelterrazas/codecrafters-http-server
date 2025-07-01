using System.Text;

namespace codecrafters_http_server;

internal sealed class HttpResponseMessage
{
    private const string HttpVersion = "HTTP/1.1";
    private const string CRLF = "\r\n";
    
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string? ContentTypeHeader { get; set; }
    public int? ContentLengthHeader { get; set; }
    public string? ContentEncodingHeader { get; set; }
    public bool CloseConnection { get; set; }
    public byte[] Body { get; set; } = [];

    public byte[] ToBytes()
    {
        var responseBuilder = new StringBuilder();
        
        responseBuilder.Append($"{HttpVersion} ");
        responseBuilder.Append(StatusCode.GetEnumDescription());
        responseBuilder.Append(CRLF);

        if (ContentTypeHeader is not null)
        {
            responseBuilder.Append($"Content-Type: {ContentTypeHeader}");
            responseBuilder.Append(CRLF);
        }
        
        if (ContentLengthHeader.HasValue)
        {
            responseBuilder.Append($"Content-Length: {ContentLengthHeader.Value}");
            responseBuilder.Append(CRLF);
        }

        if (ContentEncodingHeader is not null)
        {
            responseBuilder.Append($"Content-Encoding: {ContentEncodingHeader}");
            responseBuilder.Append(CRLF);
        }
        
        if (CloseConnection)
        {
            responseBuilder.Append("Connection: close");
            responseBuilder.Append(CRLF);
        }
        
        responseBuilder.Append(CRLF);

        byte[] response = Encoding.UTF8.GetBytes(responseBuilder.ToString());
        
        using var memoryStream = new MemoryStream();
        
        memoryStream.Write(response, 0, response.Length);
        if (Body.Length > 0)
        {
            memoryStream.Write(Body, 0, Body.Length);
        }
        
        return memoryStream.ToArray();
    }
}