using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;


namespace SampleFunction
{
    public class ProductController
    {
        /// <summary>
        /// Get Products
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ProductModel[]))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        [FunctionName("Api_GetItems")]
        public async Task<IActionResult> GetItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", "product")]HttpRequest request)
        {
            return new OkObjectResult(new List<ProductModel>());
        }

        /// <summary>
        /// Create Products
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ProductModel))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.PreconditionFailed, Type = typeof(Error))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(Error))]
        [RequestHttpHeader("Idempotency-Key", isRequired: false)]
        [RequestHttpHeader("Authorization", isRequired: true)]

        [FunctionName("Api_AddItems")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", "product")]
            [RequestBodyType(typeof(ProductCreateRequest), "product request")]HttpRequest request)
        {
            return new OkObjectResult(new ProductModel());
        }
    }

    public class Error
    {
        public string Title { get; set; }

        public string Description { get; set; }
    }

    /// <summary>Product create request</summary>
    public class ProductCreateRequest
    {
        /// <summary>
        /// Sku
        /// </summary>
        [MaxLength(32)]
        [Required]
        [JsonProperty("sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [MaxLength(100)]
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Product Amount
        /// </summary>
        [Required]
        [JsonProperty("amount")]
        public int? Amount { get; set; }

        /// <summary>
        /// Stock
        /// </summary>
        [JsonProperty("stock")]
        public int? Stock { get; set; }
    }

    /// <summary>
    /// Product
    /// </summary>
    public class ProductModel
    {
        /// <summary>
        /// product id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        /// <summary>
        /// Sku
        /// </summary>
        [JsonProperty("sku")]
        public string Sku { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Product Amount
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        /// Stock
        /// </summary>
        [JsonProperty("stock")]
        public int Stock { get; set; }
    }
}
