using System.Net;

namespace CFG2.Utils.HttpLib;

public class HttpResponse
{
    public bool Success { get; set; }

    public string? Content { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public string? DebugInfo { get; set; }
}