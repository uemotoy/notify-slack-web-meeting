using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using dcinc.api.entities;
using FluentValidation;

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

      try
      {
        switch (req.Method)
        {
          case "GET":
            log.LogInformation("GET web meetings");
            break;

          case "POST":
            log.LogInformation("POST web meetings");

            // リクエストボディからパラメータを取得
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // エンティティを作成
            WebMeeting webMeeting = new WebMeeting();
            webMeeting.Name = data?.name;
            webMeeting.StartDateTime = data?.startDateTime;
            webMeeting.Url = data?.url;
            webMeeting.RegisteredBy = data?.registeredBy;
            webMeeting.SlackChannelId = data?.slackChannelId;


            // 入力値のチェック
            var validator = new WebMeetingValidator();
            validator.ValidateAndThrow(webMeeting);

            message = await AddWebMeeting(documentsOut, webMeeting);

            break;

          default:
            throw new InvalidOperationException($"Invalid method: method={req.Method}");
        }
      }
      catch (Exception ex)
      {
        return new BadRequestObjectResult(ex.Message);
      }
      return new OkObjectResult($"This HTTP triggered function executed successfully.\n{message}");

    }

    private static async Task<string> AddWebMeeting(IAsyncCollector<dynamic> documentsOut,
             WebMeeting webMeeting)
    {
      // Add a JSON document to the output container.
      var documentItem = new
      {
        // create a random ID
        id = webMeeting.Id,
        name = webMeeting.Name,
        date = webMeeting.StartDateTime.ToString("yyyy-MM-dd"),
        startDateTime = $"{webMeeting.StartDateTime:O}",
        url = webMeeting.Url,
        registeredBy = webMeeting.RegisteredBy,
        slackChannelId = webMeeting.SlackChannelId
      };

      await documentsOut.AddAsync(documentItem);

      return JsonConvert.SerializeObject(documentItem);
    }
  }
}
