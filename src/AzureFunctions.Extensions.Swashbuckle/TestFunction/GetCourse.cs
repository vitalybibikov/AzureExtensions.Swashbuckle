using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Synigo.OpenEducationApi.Model.V4;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections.Generic;

namespace TestFunction
{
    public class GetCourse : FunctionBase
    {

        public GetCourse(){

        }

        [ProducesResponseType(typeof(PageResponse<Course>), (int)HttpStatusCode.OK)]
        [QueryStringParameter("","", DataType = typeof(PageRequest<QueryItems>))]
        [Function("GetCourse")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses/{courseId}")] HttpRequestData req,
            FunctionContext executionContext, Guid courseId)
        {
            var result = await Task.FromResult(new Course());
            return Ok(req, result);
        }

        public class PageRequest<T>
        {
            public int Page { get; set; }
            public int Size { get; set; }

            public T Item { get; set; }
        }

        public class QueryItems
        {
            public string Item1 { get; set; }
            public string Item2 { get; set; }
        }

        public class PageResponse<T> 
        {
            public int Index { get; set; }
            public int Size { get; set; }
            public List<T> Items { get; set; }
        }
    }
}
