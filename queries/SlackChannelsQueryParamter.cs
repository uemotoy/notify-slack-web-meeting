using System;
using System.Linq.Expressions;
using LinqKit;
using dcinc.api.entities;

namespace dcinc.api.queries
{
  /// <summary>
  /// Slackチャンネル情報を取得する抽出条件パラメータを表す
  /// </summary>
  public class SlackChannelsQueryParameter
  {
    #region プロパティ
    /// <summary>
    /// 一意とするID
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Slackチャンネル名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// SlackチャンネルのWebhook URL
    /// </summary>
    public string WebhookUrl { get; set; }
    /// <summary>
    /// 登録者
    /// </summary>
    public string RegisteredBy { get; set; }

    /// <summary>
    /// Idが指定されているか
    /// </summary>
    public bool HasId
    {
      get
      {
        return !string.IsNullOrEmpty(Id);
      }
    }

    /// <summary>
    /// Slackチャンネル名が指定されているか
    /// </summary>
    public bool HasName
    {
      get
      {
        return !string.IsNullOrEmpty(Name);
      }
    }

    /// <summary>
    /// SlackチャンネルのWebhook URLが指定されているか
    /// </summary>
    public bool HasWebhookUrl
    {
      get
      {
        return !string.IsNullOrEmpty(WebhookUrl);
      }
    }

    /// <summary>
    /// 登録者が指定されているか
    /// </summary>
    public bool HasRegisteredBy
    {
      get
      {
        return !string.IsNullOrEmpty(RegisteredBy);
      }
    }

    #endregion

    #region 公開サービス
    /// <summary>
    /// 抽出条件の式ツリーを取得する
    /// </summary>
    /// <returns>AND条件で結合した抽出条件の式ツリー</returns>
    public Expression<Func<SlackChannel, bool>> GetWhereExpression()
    {
      // パラメータに指定された項目をAND条件で結合する。
      Expression<Func<SlackChannel, bool>> expr = PredicateBuilder.New<SlackChannel>(true);
      var original = expr;
      if (this.HasId)
      {
        expr = expr.And(s => s.Id == this.Id);
      }
      if (this.HasName)
      {
        expr = expr.And(s => s.Name.Contains(this.Name));
      }
      if (this.HasWebhookUrl)
      {
        expr = expr.And(s => s.WebhookUrl.Contains(this.WebhookUrl));
      }
      if (this.HasRegisteredBy)
      {
        expr = expr.And(s => s.RegisteredBy == this.RegisteredBy);
      }
      if (expr == original)
      {
        expr = x => true;
      }

      return expr;
    }
    #endregion
  }
}
