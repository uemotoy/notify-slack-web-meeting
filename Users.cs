using System;
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

using dcinc.api.entities;
using dcinc.api.queries;
namespace dcinc.api
{
  public static class Users
  {


    #region ユーザーを取得する
    // [FunctionName("GetUsers")]
    // public static async Task<IActionResult> GetUsers(
    //     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Users")] HttpRequest req,
    //     [CosmosDB(
    //       databaseName: "notify-slack-web-meeting-db",
    //       collectionName: "Users",
    //       ConnectionStringSetting = "CosmosDBConnectionString")]DocumentClient client,
    //     ILogger log)
    // {
    //   log.LogInformation("C# HTTP trigger function processed a request.");
    //   string message = string.Empty;

    //   try
    //   {
    //     log.LogInformation("GET Users");

    //     // クエリパラメータから検索条件パラメータを設定
    //     UsersQueryParameter queryParameter = new UsersQueryParameter()
    //     {
    //       Ids = req.Query["ids"],
    //       Name = req.Query["name"],
    //       EmailAddress = req.Query["emailAddress"],
    //     };

    //     // ユーザー情報を取得
    //     message = JsonConvert.SerializeObject(await GetUsers(client, queryParameter, log));
    //   }
    //   catch (Exception ex)
    //   {
    //     return new BadRequestObjectResult(ex);
    //   }

    //   return new OkObjectResult(message);
    // }


    internal static async Task<IEnumerable<User>> GetUsers(
       DocumentClient client,
       UsersQueryParameter queryParameter,
       ILogger log
     )
    {
      Uri collectionUri = UriFactory.CreateDocumentCollectionUri("notify-slack-web-meeting-db", "Users");
      IDocumentQuery<User> query = client.CreateDocumentQuery<User>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true, PopulateQueryMetrics = true })
          .Where(queryParameter.GetWhereExpression())
          .AsDocumentQuery();
      log.LogInformation(query.ToString());

      var documentItems = new List<User>();
      while (query.HasMoreResults)
      {
        foreach (var documentItem in await query.ExecuteNextAsync<User>())
        {
          documentItems.Add(documentItem);
        }
      }

      return documentItems;
    }
    #endregion
  }
}
