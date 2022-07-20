using System.Net;
using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using FluentValidation;

using dcinc.api;
using dcinc.api.entities;
using dcinc.api.queries;

namespace dcinc.jobs
{
  public class NotifySlack
  {

    private readonly HttpClient _httpClient;
    public NotifySlack(IHttpClientFactory httpClientFactory)
    {
      _httpClient = httpClientFactory.CreateClient();
    }

    // 参考：https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions
    // RELEASE："0 0 9 * * 1-5"
    // DEBUG："0 */5 * * * *"
    // アプリケーション設定：WEBSITE_TIME_ZONE=Tokyo Standard Time
    [FunctionName("NotifySlack")]
    public async Task Run([TimerTrigger("0 0 7,9 * * 1-5")] TimerInfo myTimer,
     [CosmosDB(
                databaseName: "notify-slack-web-meeting-db",
                collectionName: "WebMeetings",
                ConnectionStringSetting = "CosmosDbConnectionString")
                ]DocumentClient client,
                ILogger log)
    {

      // 本日の会議一覧を取得する
      var today = DateTime.Now.Date.ToString("yyyy-MM-dd");
      var param = new WebMeetingsQueryParameter
      {
        FromDate = today,
        ToDate = today
      };
      var webMeetings = await WebMeetings.GetWebMeetings(client, param, log);

      log.LogInformation($"Notify count: {webMeetings.Count()}");

      if (!webMeetings.Any())
      {
        return;
      }

      var webMeetingsBySlackChannelMap = webMeetings.OrderBy(w => w.StartDateTime).GroupBy(w => w.SlackChannelId).ToDictionary(g => g.Key, ws => ws.OrderBy(w => w.StartDateTime).ToList());

      // Slackのチャンネルを取得する
      var slackChannelIds = webMeetingsBySlackChannelMap.Keys;
      log.LogInformation($"Slack channels count: {slackChannelIds.Count()}");

      if (!slackChannelIds.Any())
      {
        return;
      }

      var slackChannelParam = new SlackChannelsQueryParameter
      {
        Ids = string.Join(", ", slackChannelIds)
      };
      var slackChannels = await SlackChannels.GetSlackChannels(client, slackChannelParam, log);

      // Slackに通知する
      foreach (var slackChannel in slackChannels)
      {
        var message = new StringBuilder($"{DateTime.Today.ToString("yyyy/MM/dd")}のWeb会議情報\n");
        foreach (var webMeeting in webMeetingsBySlackChannelMap[slackChannel.Id])
        {
          message.AppendLine($"{webMeeting.StartDateTime.ToString("HH:mm")}~:<{webMeeting.Url}|{webMeeting.Name}>");
        }

        log.LogInformation(slackChannel.WebhookUrl);
        log.LogInformation(message.ToString());
        var content = new StringContent(JsonConvert.SerializeObject(new { text = message.ToString() }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(slackChannel.WebhookUrl, content);
        log.LogInformation(response.ToString());

        if (response.StatusCode == HttpStatusCode.OK)
        {
          // 通知したWeb会議情報を削除する
          await WebMeetings.DeleteWebMeetingById(client, string.Join(", ", webMeetingsBySlackChannelMap[slackChannel.Id].Select(w => w.Id)), log);
        }
      }

      log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
    }

  }
}
