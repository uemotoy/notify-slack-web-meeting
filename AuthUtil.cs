using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using dcinc.api.queries;


namespace dcinc.api
{
  static class AuthUtil
  {
    static public async Task<bool> checkAuthorization(DocumentClient client, HttpRequest req, ILogger log)
    {

      string emailAddress = req.Headers["x-nsw-email-address"];
      string authKey = req.Headers["x-nsw-auth-key"];

      // 認可情報がない場合認可しない
      if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(authKey))
      {
        return false;

      }
      // ユーザーの検索条件を設定し、認可情報と一致するユーザーを取得する
      UsersQueryParameter getUserQueryParameter = new UsersQueryParameter()
      {
        EmailAddress = emailAddress,
        AuthorizationKey = authKey
      };
      var authorizedUsers = await Users.GetUsers(client, getUserQueryParameter, log);

      // 該当するユーザーがいない場合は認可しない
      if (!authorizedUsers.Any())
      {
        return false;
      }

      return true;
    }
  }
}
