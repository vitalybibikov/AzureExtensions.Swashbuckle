using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using SampleFunction;

[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]
namespace SampleFunction
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            //Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());

        }
    }
}
