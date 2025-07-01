using System.ComponentModel;

namespace codecrafters_http_server;

internal enum HttpStatusCode
{
    [Description("200 OK")] OK = 200,
    [Description("201 Created")] Created = 201,
    [Description("204 No Content")] NoContent = 204,
    [Description("400 Bad Request")] BadRequest = 400,
    [Description("404 Not Found")] NotFound = 404,
    [Description("405 Method Not Allowed")] MethodNotAllowed = 405,
    [Description("500 Internal Server Error")] InternalServerError = 500,
}