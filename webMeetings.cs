using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace dcinc.api
{


  public static class webMeetings
  {
    [FunctionName("webMeetings")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [CosmosDB(
          databaseName: "notify-slack-web-meeting-db",
          collectionName: "WebMeetings",
          ConnectionStringSetting = "CosmosDBConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");
      string message = string.Empty;

      switch (req.Method)
      {
        case "GET":
          log.LogInformation("GET web meetings");
          break;

        case "POST":
          log.LogInformation("POST web meetings");

          string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
          dynamic data = JsonConvert.DeserializeObject(requestBody);

          string name = data?.name;
          // 会議名が未指定の場合はエラーとして処理
          if (string.IsNullOrEmpty(name))
          {
            return new BadRequestObjectResult("name is null or empty");
          }

          DateTime? startDateTime = data?.startDateTime;
          // 日付が未指定もしくは本日よりも前のものであればエラーとして処理
          if (!startDateTime.HasValue || startDateTime <= DateTime.Today)
          {
            return new BadRequestObjectResult("StartDateTime is invalid. Please specify the date and time after tomorrow.");
          }

          message = await AddWebMeeting(documentsOut, name, startDateTime.Value);
          break;

        default:
          throw new InvalidOperationException($"Invalid method: method={req.Method}");
      }


      return new OkObjectResult($"This HTTP triggered function executed successfully.\n{message}");
    }

    private static async Task<string> AddWebMeeting(IAsyncCollector<dynamic> documentsOut,
             string name,
             DateTime startDateTime)
    {
      // Add a JSON document to the output container.
      var documentItem = new
      {
        // create a random ID
        id = System.Guid.NewGuid().ToString(),
        name = name,
        date = startDateTime.Date.ToString("yyyy-MM-dd"),
        startDateTime = $"{startDateTime:O}"
      };

      await documentsOut.AddAsync(documentItem);

      return JsonConvert.SerializeObject(documentItem);
    }
  }
}
