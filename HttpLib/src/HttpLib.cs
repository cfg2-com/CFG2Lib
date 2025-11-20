using System.Net.Http.Headers;
using System.Text;
using CFG2.Utils.LogLib;

namespace CFG2.Utils.HttpLib;

public class HttpLib
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static Dictionary<string, HttpResponse> reqResults = new Dictionary<string, HttpResponse>();

    public static HttpResponse Get(HttpRequest req)
    {
        DoGet(req);
        HttpResponse httpResponse = reqResults[req.ID];
        reqResults.Remove(req.ID);
        return httpResponse;
    }

    private static async Task DoGet(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            if (req.AcceptContentType != null)
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(req.AcceptContentType));
            }
            if (req.AuthHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = req.AuthHeader;
            }

            using HttpResponseMessage response = await httpClient.GetAsync(req.Url);
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

        reqResults[req.ID] = resp;
    }

    public static HttpResponse PostForm(HttpRequest req)
    {
        DoFormPost(req);
        HttpResponse httpResponse = reqResults[req.ID];
        reqResults.Remove(req.ID);
        return httpResponse;
    }

    private static async Task DoFormPost(HttpRequest req)
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

        reqResults[req.ID] = resp;
    }

    public static HttpResponse PostText(HttpRequest req)
    {
        DoTextPost(req);
        HttpResponse httpResponse = reqResults[req.ID];
        reqResults.Remove(req.ID);
        return httpResponse;
    }

    private static async Task DoTextPost(HttpRequest req)
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

        reqResults[req.ID] = resp;
    }

    public static HttpResponse PostJson(HttpRequest req)
    {
        DoJsonPost(req);
        HttpResponse httpResponse = reqResults[req.ID];
        reqResults.Remove(req.ID);
        return httpResponse;
    }

    private static async Task DoJsonPost(HttpRequest req)
    {
        HttpResponse resp = new HttpResponse();
        try
        {
            if (req.AuthHeader != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = req.AuthHeader;
            }
            StringContent httpContent = new StringContent(req.Json, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await httpClient.PostAsync(req.Url, httpContent);
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

            Logger.Trace(resp.Content);
        }
        catch (Exception ex)
        {
            resp.Success = false;
            resp.DebugInfo = ex.Message + "\n" + ex.ToString();
            Logger.Trace("Error: "+resp.DebugInfo);
        }

        reqResults[req.ID] = resp;
    }
}