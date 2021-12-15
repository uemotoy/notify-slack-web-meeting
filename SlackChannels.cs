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
  public static class SlackChannels
  {
    #region Slackチャンネル情報を登録
    /// <summary>
    /// Slackチャンネル情報を登録する。
    /// </summary>
    /// <returns>登録したSlackチャンネル情報</returns>        
    [FunctionName("AddSlackChannels")]
    public static async Task<IActionResult> AddSlackChannels(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SlackChannels")] HttpRequest req,
        [CosmosDB(
                databaseName: "notify-slack-web-meeting-db",
                collectionName: "SlackChannels",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");
      string message = string.Empty;

      try
      {
        log.LogInformation("POST SlackChannels");

        // リクエストのBODYからパラメータ取得
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        // エンティティに設定
        SlackChannel slackChannel = new SlackChannel()
        {
          Name = data?.name,
          WebhookUrl = data?.webhookUrl,
          RegisteredBy = data?.registeredBy
        };

        // 入力値チェックを行う
        SlackChannelValidator validator = new SlackChannelValidator();
        validator.ValidateAndThrow(slackChannel);

        // Slackチャンネル情報を登録
        message = await AddSlackChannels(documentsOut, slackChannel);
      }
      catch (Exception ex)
      {
        return new BadRequestObjectResult(ex);
      }

      return new OkObjectResult($"This HTTP triggered function executed successfully.\n{message}");
    }

    /// <summary>
    /// Slackチャンネル情報を登録する。
    /// </summary>
    /// <param name="documentsOut">CosmosDBのドキュメント</param>
    /// <param name="slackChannel">Slackチャンネル情報</param>
    /// <returns></returns>
    private static async Task<string> AddSlackChannels(
                IAsyncCollector<dynamic> documentsOut,
                SlackChannel slackChannel
                )
    {
      // 登録日時にUTCでの現在日時を設定
      slackChannel.RegisteredAt = DateTime.UtcNow;
      // Add a JSON document to the output container.
      string documentItem = JsonConvert.SerializeObject(slackChannel, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
      await documentsOut.AddAsync(documentItem);
      return documentItem;
    }
    #endregion

    #region Slackチャンネル情報一覧を取得
    /// <summary>
    /// Slackチャンネル情報一覧を取得する。
    /// </summary>
    /// <param name="req">HTTPリクエスト</param>
    /// <param name="client">CosmosDBのドキュメントクライアント</param>
    /// <param name="log">ロガー</param>
    /// <returns>Slackチャンネル情報一覧</returns>
    [FunctionName("GetSlackChannels")]
    public static async Task<IActionResult> GetSlackChannels(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "SlackChannels")] HttpRequest req,
        [CosmosDB(
                databaseName: "notify-slack-web-meeting-db",
                collectionName: "SlackChannels",
                ConnectionStringSetting = "CosmosDbConnectionString")
                ]DocumentClient client,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");
      string message = string.Empty;

      try
      {
        log.LogInformation("GET SlackChannels");

        // クエリパラメータから検索条件パラメータを設定
        SlackChannelsQueryParameter queryParameter = new SlackChannelsQueryParameter()
        {
          Ids = req.Query["ids"],
          Name = req.Query["name"],
          WebhookUrl = req.Query["webhookUrl"],
          RegisteredBy = req.Query["registeredBy"]
        };

        // Slackチャンネル情報を取得
        message = await GetSlackChannels(client, queryParameter, log);
      }
      catch (Exception ex)
      {
        return new BadRequestObjectResult(ex);
      }

      return new OkObjectResult($"This HTTP triggered function executed successfully.\n{message}");
    }

    /// <summary>
    /// Slackチャンネル情報一覧を取得する。
    /// </summary>
    /// <param name="client">CosmosDBのドキュメントクライアント</param>
    /// <param name="queryParameter">抽出条件パラメータ</param>
    /// <param name="log">ロガー</param>
    /// <returns>Slackチャンネル情報一覧</returns>
    internal static async Task<string> GetSlackChannels(
      DocumentClient client,
      SlackChannelsQueryParameter queryParameter,
      ILogger log
    )
    {
      Uri collectionUri = UriFactory.CreateDocumentCollectionUri("notify-slack-web-meeting-db", "SlackChannels");
      IDocumentQuery<SlackChannel> query = client.CreateDocumentQuery<SlackChannel>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, PopulateQueryMetrics = true })
          .Where(queryParameter.GetWhereExpression())
          .AsDocumentQuery();

      var documentItems = new List<SlackChannel>();
      while (query.HasMoreResults)
      {
        foreach (var documentItem in await query.ExecuteNextAsync<SlackChannel>())
        {
          documentItems.Add(documentItem);
        }
      }

      return JsonConvert.SerializeObject(documentItems);
    }
    #endregion

  }
}