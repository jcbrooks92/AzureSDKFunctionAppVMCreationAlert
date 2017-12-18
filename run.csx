#r "Newtonsoft.Json"

using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");
    var privateIPvalue = "null";

    log.Info($"Authenticating");
    log.Info(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
    var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
    var azure = Azure.Configure().WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic).Authenticate(credentials).WithDefaultSubscription();
    log.Info(credentials.ToString());

    log.Info(azure.ToString());
    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);
    JObject parsed = JObject.Parse(jsonContent);
    log.Info($"Parsing Json");

    foreach (var pair in parsed)
        {
            log.Info($"{pair.Key}: {pair.Value}");
        }
    log.Info($"Parsing Complete...Creating Variables");

    string vmId = data.context.activityLog.resourceId;
    log.Info($"Resource ID: {vmId}");
    char[] delimiterChars = {'/'};
    string[] VM = vmId.Split(delimiterChars);
    var vmName = VM[8];
    log.Info($"VM Name: {vmName}");

    string groupName = data.context.activityLog.resourceGroupName;
    log.Info($"RG Name: {groupName}");

    log.Info($"Getting VM info using VM and RG...");

    var vm = azure.VirtualMachines.GetByResourceGroup(groupName, vmName);
    try{
   
   log.Info($"VM Info: {vm.ToString()}");
    }
      catch (Exception e)
   {
       log.Info(e.ToString());
   }

   log.Info($"Getting the IP Address...");
   try{
   var privateIP = vm.GetPrimaryNetworkInterface().IPConfigurations.Values;
   
    foreach (var x in privateIP)
            {
                privateIPvalue = x.PrivateIPAddress;
                log.Info($"Private IP Address: {x.PrivateIPAddress}");
            }
   }
   catch (Exception e)
   {
       log.Info(e.ToString());
   }


      return req.CreateResponse(HttpStatusCode.OK, new
    {
        
        greeting = privateIPvalue
    });
}
