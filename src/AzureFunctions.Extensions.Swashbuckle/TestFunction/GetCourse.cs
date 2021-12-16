using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Synigo.OpenEducationApi.Model.V4;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace TestFunction
{
    public class GetCourse : FunctionBase
    {

        public GetCourse(){

        }

        [ProducesResponseType(typeof(Course[]), (int)HttpStatusCode.OK)]
        [Function("GetCourse")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses/{courseId}")] HttpRequestData req,
            FunctionContext executionContext, Guid courseId)
        {
            var result = await Task.FromResult(new Course());
            return Ok(req, result);
        }
    }
}
