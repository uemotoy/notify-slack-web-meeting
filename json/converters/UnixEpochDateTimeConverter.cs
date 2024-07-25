using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace dcinc.json.converters
{
    /// <summary>
    /// Unix エポック時間を使用するJSONコンバーター
    /// </summary>
    /// <remarks>
    /// 参考：https://learn.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support#use-unix-epoch-date-format
    /// </remarks>
    sealed class UnixEpochDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string formatted = reader.GetString()!;
            long unixTime = formatted.FromJsonFormat();

            // Unix エポック時間は UTC 時刻における1970年1月1日午前0時0分0秒（Unix Xエポック）からの経過秒数であり、秒単位で追加する。
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dateTime = epoch.AddSeconds(unixTime);
            return dateTime.ToLocalTime(); // ローカル時刻に変換
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            string formatted = value.ToJsonFormat();
            writer.WriteStringValue(formatted);
        }
    }

    /// <summary>
    /// Unix エポック時間を使用する拡張メソッド
    /// </summary>
    public static class UnixEpochDateTimeExtensions
    {
        private static readonly Regex s_regex = new Regex(@"^\d+$", RegexOptions.CultureInvariant);

        /// <summary>
        /// 日時をUnix エポック時間形式にします。
        /// </summary>
        /// <param name="dateTime">日時</param>
        /// <returns>Unix エポック時間</returns>
        public static long ToUnixTime(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 日時をUnix エポック時間のJSON形式にします。
        /// </summary>
        /// <param name="dateTime">日時</param>
        /// <returns>日時をUnix エポック時間のJSON形式にした文字列</returns>
        public static string ToJsonFormat(this DateTime dateTime)
        {
            return dateTime.ToUnixTime().ToJsonFormat();
        }

        /// <summary>
        /// Unix エポック時間をJSON形式にします。
        /// </summary>
        /// <param name="unixTime">Unix エポック</param>
        /// <returns>Unix エポック時間をJSON形式にした文字列</returns>
        public static string ToJsonFormat(this long unixTime)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{unixTime}");
        }

        /// <summary>
        /// Unix エポック時間のJSON形式文字列からUnix エポック時間にします。
        /// </summary>
        /// <param name="dateTimeString"></param>
        /// <returns>Unix エポック時間</returns>
        public static long FromJsonFormat(this string dateTimeString)
        {

            // DateTimeOffsetに変換
            DateTimeOffset dateTimeOffset;
            bool success = DateTimeOffset.TryParseExact(
                 dateTimeString,
                 "yyyy-MM-ddTHH:mm:sszzz",
                 CultureInfo.InvariantCulture,
                 DateTimeStyles.None,
             out dateTimeOffset);

            if (success)
            {
                DateTimeOffset localDateTimeOffset = dateTimeOffset.ToLocalTime();
                // Unixエポック時間に変換
                long unixTime = localDateTimeOffset.ToUnixTimeSeconds();
                return unixTime;

            }
            else
            {
                // 現在の時刻を取得
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // Unixエポック時間に変換
                long unixTimeSeconds = now.ToUnixTimeSeconds();

                return unixTimeSeconds;
            }

        }
    }
}