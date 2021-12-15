using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using FluentValidation;

using dcinc.api;
using dcinc.api.entities;
using dcinc.api.queries;

namespace dcinc.jobs
{
  public static class NotifySlack
  {
    [FunctionName("NotifySlack")]
    internal static void Run([TimerTrigger("0 0 9 * * 1-5")] TimerInfo myTimer,
     [CosmosDB(
                databaseName: "notify-slack-web-meeting-db",
                collectionName: "WebMeetings",
                ConnectionStringSetting = "CosmosDbConnectionString")
                ]DocumentClient client,
                ILogger log)
    {

      // 本日の会議一覧を取得する
      var today = DateTime.UtcNow.Date.ToString("YYYY-MM-DD");
      var param = new WebMeetingsQueryParameter
      {
        FromDate = today,
        ToDate = today
      };
      var webMeetings = WebMeetings.GetWebMeetings(client, param, log);

      // Slackのチャンネルを取得する
      var slackChannelIds = webMeetings.Result.Select(w => w.SlackChannelId).Distinct();
      var slackChannelParam = new SlackChannelsQueryParameter
      {
        Ids = string.Join(",", slackChannelIds)
      };
      var slackChannels = SlackChannels.GetSlackChannels(client, slackChannelParam, log);

      // Slackに通知する 

      log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    }

  }
}
