using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;

namespace MSDevSummitDemo
{
    public static class WhoAmI
    {
        [FunctionName("WhoAmI")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "SampleFunctions" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var environmentUrl = Environment.GetEnvironmentVariable("EnvironmentUrl");

            var managedIdentity = new DefaultAzureCredential();

            // use with user assigned identities

            //var clientId = Environment.GetEnvironmentVariable("ClientId");

            //var userManagedIdentity = new DefaultAzureCredential(
            //    new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId });

            ServiceClient service = new ServiceClient(
                        tokenProviderFunction: func => GetToken(environmentUrl, managedIdentity),
                        instanceUrl: new Uri(environmentUrl),
                        useUniqueInstance: true);

            var whoAmIResponse = await service.ExecuteAsync(new WhoAmIRequest());

            string responseMessage = $"Hello, my userid is: { ((WhoAmIResponse)whoAmIResponse).UserId}";

            return new OkObjectResult(responseMessage);
        }
        private static async Task<string> GetToken(string environment, DefaultAzureCredential credential)
        {
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" }));
            return accessToken.Token;
        }
    }
}

