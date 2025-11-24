using System.Net.Http.Headers;

namespace CFG2.Utils.HttpLib;

public class HttpRequest
{
    private readonly string _id = Guid.NewGuid().ToString();
    private string _url = "";

    public HttpRequest(string baseUrl)
    {
        _url = baseUrl;
    }

    public string Url
    {
        get
        {
            if ((UrlParams != null) && (UrlParams.Count > 0))
            {
                UriBuilder uriBuilder = new UriBuilder(_url);
                string query = string.Join("&", UrlParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                uriBuilder.Query = query;
                return uriBuilder.Uri.AbsoluteUri;
            }
            else
            {
                return _url;
            }
        }
    }
    public string? AcceptContentType { get; set; }
    public AuthenticationHeaderValue? AuthHeader { get; set; }
    public Dictionary<string, string>? FormParams { get; set; }
    public Dictionary<string, string>? UrlParams { get; set; }
    public string Text { get; set; }
    public string Json { get; set; }
    public string ID { get { return _id; } }

    public HttpResponse Get()
    {
        return HttpUtils.Get(this);
    }
}