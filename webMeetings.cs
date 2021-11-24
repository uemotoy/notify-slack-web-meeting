using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FluentValidation;
using Newtonsoft.Json.Serialization;

using dcinc.api.entities;
using dcinc.api.queries;
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
          [CosmosDB(
          databaseName: "notify-slack-web-meeting-db",
          collectionName: "WebMeetings",
          ConnectionStringSetting = "CosmosDBConnectionString")]DocumentClient client,
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
            // クエリパラメータから検索条件パラメータを設定
            WebMeetingsQueryParameter queryParameter = new WebMeetingsQueryParameter();
            string fromDateString = req.Query["fromDate"];
            if (!string.IsNullOrEmpty(fromDateString) && !string.IsNullOrWhiteSpace(fromDateString))
            {
              DateTime fromDate = Convert.ToDateTime(fromDateString);
              queryParameter.FromDate = fromDate;
            };
            string toDateString = req.Query["toDate"];
            if (!string.IsNullOrEmpty(toDateString) && !string.IsNullOrWhiteSpace(toDateString))
            {
              DateTime toDate = Convert.ToDateTime(toDateString);
              queryParameter.ToDate = toDate;
            };
            queryParameter.RegisteredBy = req.Query["registeredBy"];
            queryParameter.SlackChannelId = req.Query["slackChannelId"];

            // 入力値チェックを行う
            var queryParameterValidator = new WebMeetingsQueryParameterValidator();
            queryParameterValidator.ValidateAndThrow(queryParameter);

            // Web会議情報を取得
            message = await GetWebMeetings(client, queryParameter);

            break;

          case "POST":
            log.LogInformation("POST web meetings");

            // リクエストボディからパラメータを取得
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // エンティティを作成
            WebMeeting webMeeting = new WebMeeting();
            webMeeting.Name = data?.name;
            webMeeting.StartDateTime = data?.startDateTime ?? DateTime.UnixEpoch;
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


    /// <summary>
    /// Web会議を登録する
    /// </summary>
    /// <param name="documentsOut">CosmosDBのドキュメント</param>
    /// <param name="WebMeeting">Web会議情報</param>
    /// <returns></retruns>
    private static async Task<string> AddWebMeeting(IAsyncCollector<dynamic> documentsOut,
         WebMeeting webMeeting)
    {
      // 登録日時にUTCでの現在日時を設定
      webMeeting.RegisteredAt = DateTime.UtcNow;
      // Add a JSON document to the output container.
      string documentItem = JsonConvert.SerializeObject(webMeeting, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      await documentsOut.AddAsync(documentItem);
      return documentItem;
    }

    /// <summary>
    /// Web会議情報一覧を取得する。
    /// </summary>
    /// <param name="client">CosmosDBのドキュメントクライアント</param>
    /// <param name="queryParameter">抽出条件パラメータ</param>
    /// <returns></returns>
    private static async Task<string> GetWebMeetings(DocumentClient client,
                    WebMeetingsQueryParameter queryParameter)
    {
      // Get a JSON document from the container.
      Uri collectionUri = UriFactory.CreateDocumentCollectionUri("notify-slack-of-web-meeting-db", "WebMeetings");
      IDocumentQuery<WebMeeting> query = client.CreateDocumentQuery<WebMeeting>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, PopulateQueryMetrics = true })
          .Where(w =>
              (queryParameter.HasRegisteredBy ? w.RegisteredBy == queryParameter.RegisteredBy : true)
           && (queryParameter.HasSlackChannelId ? w.SlackChannelId == queryParameter.SlackChannelId : true)
           && (queryParameter.HasFromDate ? queryParameter.FromDateUtcValue <= w.Date : true)
           && (queryParameter.HasToDate ? w.Date <= queryParameter.ToDateUtcValue : true))
          .AsDocumentQuery();

      var documentItems = new List<WebMeeting>();
      while (query.HasMoreResults)
      {
        foreach (var documentItem in await query.ExecuteNextAsync<WebMeeting>())
        {
          documentItems.Add(documentItem);
        }
      }
      return JsonConvert.SerializeObject(documentItems);

    }
  }
}
