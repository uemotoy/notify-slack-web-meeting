# Notify Slack of web meeting

[![Build](https://github.com/yamadakou/notify-slack-of-web-meeting/actions/workflows/build.yml/badge.svg)](https://github.com/yamadakou/notify-slack-of-web-meeting/workflows/build.yml)

当日の Web 会議の情報を Slack に通知する Web サービスです。

## 概要

### Notify Slack of web meeting の特徴

- テレワークが増え会議も Web 会議が主流となり、毎日、Web 会議の URL を Outlook から参照する手間が増えました。
- そこで、今日、予定されている Web 会議を毎朝 Slack で確認できるよう、本サービスでは以下の機能を提供します。
  - Web 会議情報を登録・検索・削除する REST API
  - 通知先の Slack チャンネル情報を登録・検索・削除する REST API
  - 朝 9 時に当日の Web 会議情報を指定の Slack チャンネルに通知する定期バッチ
- Web 会議情報や通知先の Slack チャンネル情報の登録などは自由にクライアントを用意することで、 Outlook や Google カレンダーなど好みの予定表から Web 会議情報を抽出し、指定した Slack チャンネルに通知することが可能です。
  - Outlook クライアントからログインユーザーの翌日の Web 会議情報を登録するコンソールアプリは下記リポジトリで提供しており、 Windows タスクスケジューラで毎日実行するよう登録することで、自動的に毎朝 9 時に当日の Web 会議情報を Slack で確認できます。
    - [Notify Slack of web meeting CLI](https://github.com/yamadakou/notify-slack-of-web-meeting.cli)
      - https://github.com/yamadakou/notify-slack-of-web-meeting.cli

### 機能説明

#### Web 会議情報を登録・検索・削除する REST API

- Web 会議情報を登録

  ```js
  POST api/WebMeetings
  {
      "name": <Web会議名>,
      "startDateTime": <翌日以降のWeb会議の開始日時>,
      "url": <Web会議のURL>,
      "registeredBy": <登録者>,
      "slackChannelId": <通知先のSlackチャンネル情報のID>
  }
  ```

  - 全ての項目が必須項目となります。
  - `startDateTime` は翌日以降の日時を指定する必要があります。

  - レスポンス(登録した Web 会議情報を返す)
    ```json
    {
      "id": <Web会議情報ID>,
      "name": <Web会議名>,
      "startDateTime": <Web会議の開始日時>,
      "date": <Web会議の日付(UNIXエポックタイム)>,
      "url": <Web会議のURL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>,
      "slackChannelId": <通知先のSlackチャンネル情報のID>
    }
    ```

- Web 会議情報を検索

  ```js
  GET api/WebMeetings
  ```

  - クエリパラメータ

    | 項目           | 値                                                 | 備考                                                             |
    | :------------- | :------------------------------------------------- | :--------------------------------------------------------------- |
    | ids            | Web 会議情報 ID                                    | 複数指定時はカンマ区切りで指定                                   |
    | fromDate       | Web 会議の日付範囲の開始日（ISO8601 形式の文字列） | Web 会議の開始日と終了日を指定する場合、終了日を含む過去日を指定 |
    | toDate         | Web 会議の日付範囲の終了日（ISO8601 形式の文字列） | Web 会議の開始日と終了日を指定する場合、開始日を含む未来日を指定 |
    | registeredBy   | 登録者                                             | 完全一致                                                         |
    | slackChannelId | 通知先の Slack チャンネル情報 ID                   |

  - レスポンス
    ```json
    [{
      "id": <Web会議情報ID>,
      "name": <Web会議名>,
      "startDateTime": <Web会議の開始日時>,
      "date": <Web会議の日付(UNIXエポックタイム)>,
      "url": <Web会議のURL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>,
      "slackChannelId": <通知先のSlackチャンネル情報のID>
    }]
    ```

- Web 会議情報を取得

  ```js
  GET api/WebMeetings/{Web会議情報ID(複数指定時はカンマ区切りで指定)}
  ```

  - レスポンス
    ```json
    [{
      "id": <Web会議情報ID>,
      "name": <Web会議名>,
      "startDateTime": <Web会議の開始日時>,
      "date": <Web会議の日付(UNIXエポックタイム)>,
      "url": <Web会議のURL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>,
      "slackChannelId": <通知先のSlackチャンネル情報のID>
    }]
    ```

- Web 会議情報を削除

  ```js
  DELETE api/WebMeetings/{Web会議情報ID(複数指定時はカンマ区切りで指定)}
  ```

  - レスポンス(削除した Web 会議情報を返す)
    ```json
    [{
      "id": <Web会議情報ID>,
      "name": <Web会議名>,
      "startDateTime": <Web会議の開始日時>,
      "date": <Web会議の日付(UNIXエポックタイム)>,
      "url": <Web会議のURL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>,
      "slackChannelId": <通知先のSlackチャンネル情報のID>
    }]
    ```

#### 通知先の Slack チャンネル情報を登録・検索・削除する REST API

- Slack チャンネル情報を登録

  ```js
  POST api/SlackChannels
  {
      "name": <Slackチャンネル情報名>,
      "webhookUrl": <SlackチャンネルのWebhook URL>,
      "registeredBy": <登録者>
  }
  ```

  - 全ての項目が必須項目となります。

  - レスポンス(登録した Slack チャンネル情報を返す)
    ```json
    {
      "id": <Slackチャンネル情報ID>,
      "name": <Slackチャンネル情報名>,
      "webhookUrl": <SlackチャンネルのWebhook URL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>
    }
    ```

- Slack チャンネル情報を検索

  ```js
  GET api/SlackChannels
  ```

  - クエリパラメータ

    | 項目         | 値                             | 備考                           |
    | :----------- | :----------------------------- | :----------------------------- |
    | ids          | Slack チャンネル情報 ID        | 複数指定時はカンマ区切りで指定 |
    | name         | Slack チャンネル情報名         | 部分一致                       |
    | webhookUrl   | Slack チャンネルの Webhook URL | 完全一致                       |
    | registeredBy | 登録者                         | 完全一致                       |

  - レスポンス
    ```json
    [{
      "id": <Slackチャンネル情報ID>,
      "name": <Slackチャンネル情報名>,
      "webhookUrl": <SlackチャンネルのWebhook URL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>
    }]
    ```

- Slack チャンネル情報を取得

  ```js
  GET api/SlackChannels/{Slackチャンネル情報ID(単一指定)}
  ```

  - レスポンス
    ```json
    {
      "id": <Slackチャンネル情報ID>,
      "name": <Slackチャンネル情報名>,
      "webhookUrl": <SlackチャンネルのWebhook URL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>
    }
    ```

- Slack チャンネル情報を削除

  ```js
  DELETE api/SlackChannels/{Slackチャンネル情報ID(複数指定時はカンマ区切りで指定)}
  ```

  - レスポンス(削除した Slack チャンネル情報を返す)
    ```json
    [{
      "id": <Slackチャンネル情報ID>,
      "name": <Slackチャンネル情報名>,
      "webhookUrl": <SlackチャンネルのWebhook URL>,
      "registeredBy": <登録者>,
      "registeredAt": <登録日時>
    }]
    ```

#### 当日の Web 会議情報を指定の Slack チャンネルに通知する定期バッチ

- 平日の朝 9 時に実行
- 翌日の Web 会議情報を Web 会議情報に指定されている Slack チャンネル情報ごとに開始時刻順にソートし、Slack チャンネルに通知します。
- Slack チャンネルに通知した Web 会議情報は削除します。

## 利用方法

### Azure 環境

Azure Functions と Azure Cosmos DB を利用します。

- Azure Cosmos DB アカウントに以下の Database および Container を作成する。
  - Database
    - Name: notify-slack-of-web-meeting-db
  - Container
    - Web 会議情報
      - Name: WebMeetings
      - Partition key: /date
    - Slack チャンネル情報
      - Name: SlackChannels
      - Partition key: /id

#### 参考

- クイック スタート:Azure portal を使用して Azure Cosmos のアカウント、データベース、コンテナー、および項目を作成する
  - https://docs.microsoft.com/ja-jp/azure/cosmos-db/sql/create-cosmosdb-resources-portal
- Azure Cosmos DB の Free レベル
  - https://docs.microsoft.com/ja-jp/azure/cosmos-db/free-tier

### ビルド環境

Visual Studio Code で、ビルドと Azure Functions への発行ができるよう、以下の環境を整える。

- .NET Core 3.1 SDK
  - https://dotnet.microsoft.com/en-us/download/dotnet/3.1
- Azure Functions Core Tools バージョン 3.x
  - https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-run-local?tabs=v3%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools
- Visual Studio Code
  - https://code.visualstudio.com/
- Visual Studio Code 用の C# 拡張機能
  - https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp
- Visual Studio Code 用 Azure Functions 拡張機能
  - https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions
- Visual Studio Code 用の Azure データベース拡張機能
  - https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-cosmosdb

#### 参考

- Visual Studio Code を使用して Azure Functions を開発する
  - https://docs.microsoft.com/ja-jp/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process

### ビルド＆デプロイ

1. `gir clone ・・・` などで本プロジェクトをローカルに取得し、 Visual Studio Code で開く。
2. ビルドできるよう、[依存パッケージ](依存パッケージ)を .NET CLI のコマンド `dotnet add package ・・・` で導入する。
3. `dotnet build` で、ビルドが成功することを確認する。
4. 以下の Microsoft Docs を参考に、Azure Cosmos DB への接続情報をアプリの設定に追加する。
   - 関数アプリの設定を更新する
     - https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-add-output-binding-cosmos-db-vs-code?pivots=programming-language-csharp&tabs=in-process#update-your-function-app-settings
5. 以下の Microsoft Docs を参考に、Azure にプロジェクトを発行（デプロイ）する。
   - Azure にプロジェクトを発行する
     - https://docs.microsoft.com/ja-jp/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#publish-the-project-to-azure
6. 日本時間で動作させるために Azure Functions のアプリケーション設定に以下を追加する。
   |名前|値|
   |:--|:--|
   |WEBSITE_TIME_ZONE|Tokyo Standard Time|
   - 参考
     - https://docs.microsoft.com/ja-jp/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-time-zones
7. クライアントから Slack チャンネル情報や Web 会議情報を登録する。
   - コンソールアプリ「notify-slack-of-web-meeting.cli」利用する場合は以下のリポジトリを参照
     - https://github.com/yamadakou/notify-slack-of-web-meeting.cli

#### 依存パッケージ

※ `dotnet list package` の結果から作成
|最上位レベル パッケージ|バージョン|Nuget|
|:--|:--|:--|
| FluentValidation |10.3.4|https://www.nuget.org/packages/FluentValidation/10.3.4|
| LinqKit.Microsoft.EntityFrameworkCore |3.1.27|https://www.nuget.org/packages/LinqKit.Microsoft.EntityFrameworkCore/3.1.27|
| Microsoft.Azure.Functions.Extensions |1.1.0 |https://www.nuget.org/packages/Microsoft.Azure.Functions.Extensions/1.1.0|
| Microsoft.Azure.WebJobs.Extensions.CosmosDB |3.0.10|https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.CosmosDB/3.0.10|
| Microsoft.Extensions.DependencyInjection |3.1.22|https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/3.1.22|
| Microsoft.Extensions.Http |3.1.22|https://www.nuget.org/packages/Microsoft.Extensions.Http/3.1.22|
| Microsoft.Extensions.Http.Polly |3.1.22|https://www.nuget.org/packages/Microsoft.Extensions.Http.Polly/3.1.22|
| Microsoft.NET.Sdk.Functions |3.0.13|https://www.nuget.org/packages/Microsoft.NET.Sdk.Functions/3.0.13|
| SourceLink.Copy.PdbFiles |2.8.3 |https://www.nuget.org/packages/SourceLink.Copy.PdbFiles/2.8.3|

## （関連リポジトリ）

- Notify Slack of web meeting CLI
  - https://github.com/yamadakou/notify-slack-of-web-meeting.cli
