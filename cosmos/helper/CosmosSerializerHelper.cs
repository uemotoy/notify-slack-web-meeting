using Microsoft.Azure.Cosmos;

namespace dcinc.cosmos.helpers
{
    public class CosmosSerializerHelper
    {
        /// <summary>
        /// Linq シリアル化プロパティを構成するオプションを取得します。
        /// </summary>
        /// <returns>Linq シリアル化プロパティを構成するオプションを返します。</returns>
        /// <remarks>
        /// 参考；https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.azure.cosmos.cosmoslinqserializeroptions?view=azure-dotnet
        /// </remarks>
        public static CosmosLinqSerializerOptions GetCosmosLinqSerializerOptions()
        {
            return new CosmosLinqSerializerOptions()
            {
                // プロパティ名の最初の文字は小文字
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };
        }
    }
}