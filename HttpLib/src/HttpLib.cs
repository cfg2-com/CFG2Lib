using System.Net.Http.Headers;
using System.Text;
using CFG2.Utils.LogLib;

namespace CFG2.Utils.HttpLib;

public class HttpLib
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static HttpResponse Get(HttpRequest req)
    {
        return DoGet(req).GetAwaiter().GetResult();
    }

    private static async Task<HttpResponse> DoGet(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, req.Url);
            if (req.AuthHeader != null)
            {
                requestMessage.Headers.Authorization = req.AuthHeader;
            }
            if (!string.IsNullOrEmpty(req.AcceptContentType))
            {
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(req.AcceptContentType));
            }

            using HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            resp.Content = await response.Content.ReadAsStringAsync();
            Logger.Trace(resp.Content);
            resp.HttpStatusCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                resp.Success = false;
            }
            else
            {
                resp.Success = true;
            }
            resp.DebugInfo = req.Url;
        }
        catch (Exception ex)
        {
            Exception e = ex;
            resp.Success = false;
            resp.DebugInfo = e.Message + "\n" + e.ToString();
        }

        return resp;
    }

    public static HttpResponse PostForm(HttpRequest req)
    {
        return DoFormPost(req).GetAwaiter().GetResult();
    }

    private static async Task<HttpResponse> DoFormPost(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            if (req.FormParams == null)
            {
                throw new ArgumentNullException(nameof(req.FormParams), "FormParams cannot be null for a form post.");
            }
            FormUrlEncodedContent formContent = new FormUrlEncodedContent(req.FormParams.ToArray());
            using HttpResponseMessage response = await httpClient.PostAsync(req.Url, formContent);
            resp.Content = await response.Content.ReadAsStringAsync();
            resp.HttpStatusCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                resp.Success = false;
            }
            else
            {
                resp.Success = true;
            }
        }
        catch (Exception ex)
        {
            Exception e = ex;
            resp.Success = false;
            resp.DebugInfo = e.Message + "\n" + e.ToString();
        }

        return resp;
    }

    public static HttpResponse PostText(HttpRequest req)
    {
        return DoTextPost(req).GetAwaiter().GetResult();
    }

    private static async Task<HttpResponse> DoTextPost(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            var content = new StringContent(req.Text, Encoding.UTF8, "text/plain");
            var response = await httpClient.PostAsync(req.Url, content);
            response.EnsureSuccessStatusCode(); // Throw an exception if the status code is not successful
            Logger.Trace($"Response: {response.StatusCode}");
            resp.Content = await response.Content.ReadAsStringAsync();
            resp.HttpStatusCode = response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                resp.Success = false;
            }
            else
            {
                resp.Success = true;
            }
        }
        catch (Exception ex)
        {
            Exception e = ex;
            resp.Success = false;
            resp.DebugInfo = e.Message + "\n" + e.ToString();
        }

        return resp;
    }

    public static HttpResponse PostJson(HttpRequest req)
    {
        return DoJsonPost(req).GetAwaiter().GetResult();
    }

    private static async Task<HttpResponse> DoJsonPost(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, req.Url);
            if (req.AuthHeader != null)
            {
                requestMessage.Headers.Authorization = req.AuthHeader;
            }

            requestMessage.Content = new StringContent(req.Json ?? "", Encoding.UTF8, "application/json");
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
            resp.Content = await response.Content.ReadAsStringAsync();
            resp.HttpStatusCode = response.StatusCode;
            resp.Success = response.IsSuccessStatusCode;

            Logger.Trace(resp.Content);
        }
        catch (Exception ex)
        {
            resp.Success = false;
            resp.DebugInfo = ex.Message + "\n" + ex.ToString();
            Logger.Trace("Error: "+resp.DebugInfo);
        }

        return resp;
    }
}