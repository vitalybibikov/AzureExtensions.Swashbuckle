using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Primitives;

namespace TestFunction
{
    public class FunctionBase
    {
        public Dictionary<string, StringValues> GetQueryParameters(string urlQuery) => QueryHelpers.ParseQuery(urlQuery);

        public HttpResponseData Ok(HttpRequestData req, object result)
        {
            var response = req.CreateResponse(result != null ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.NotFound);
            response.WriteString(JsonSerializer.Serialize(result));
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            return response;
        }
    }
}
