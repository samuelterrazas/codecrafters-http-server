namespace codecrafters_http_server;

internal sealed class HttpRequestMessage
{
    private const string CRLF = "\r\n";
    private readonly string[] _requestTargetSegments;
    
    public string Method { get; private set; }
    public string RequestTarget { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    public bool CloseConnection { get; set; }
    public string Body { get; private set; }
    
    public HttpRequestMessage(string rawRequest)
    {
        string[] requestLines = rawRequest.Split(CRLF);
        string[] startLine = requestLines[0].Split(' ');
        
        Method = startLine[0];
        RequestTarget = startLine[1];
        Headers = requestLines[1..^1]
            .Where(h => h.Length > 0)
            .Select(h => h.Split(':', 2, StringSplitOptions.TrimEntries))
            .ToDictionary(parts => parts[0], parts => parts[1]);
        CloseConnection = Headers.TryGetValue("Connection", out string? connectionHeader)
            && connectionHeader.Equals("close", StringComparison.Ordinal);
        Body = requestLines[^1];
        
        _requestTargetSegments = RequestTarget.Split('/', StringSplitOptions.RemoveEmptyEntries);
    }
    
    public string GetRequestTargetSegmentOrDefault(int index)
    {
        return index >= 0 && index < _requestTargetSegments.Length
            ? _requestTargetSegments[index]
            : string.Empty;
    }
}