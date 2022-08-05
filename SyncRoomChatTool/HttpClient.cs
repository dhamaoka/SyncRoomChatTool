using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SyncRoomChatTool
{
    /// <summary>
    /// Serviceとの通信を担当する、HTTPクライアントのサンプル。
    /// https://iwasiman.hatenablog.com/entry/20210622-CSharp-HttpClient 丸パクリ。不要部分は削除。VOICEVOXはPOST出来りゃいい。
    /// </summary>
    public class ServiceHttpClient
    {
        /// <summary>
        /// 通信先のアドレス
        /// </summary>
        private readonly string requestAddress;

        /// <summary>
        /// C#側のHttpクライアント
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// デフォルトコンストラクタ。外部からは呼び出せません。
        /// </summary>
        private ServiceHttpClient()
        {
        }

        /// <summary>
        /// 引数付きのコンストラクタ。こちらを使用します。
        /// 引数には正しいURLが入っていることが前提です。
        /// </summary>
        /// <param name="baseUrl">ベースのURL</param>
        public ServiceHttpClient(string requestAddress)
        {
            this.requestAddress = requestAddress;

            // 通信するメソッドでその都度HttpClientをnewすると毎回ソケットを開いてリソースを消費するため、
            // メンバ変数で使い回す手法を取っています。
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// VOICEVOX用に改修。元はキーをJsonにまでしてくれてたけど、それは要らんので。喚び元でやれと。
        /// </summary>
        /// <param name="QueryResponce"></param>
        /// <returns>正常：レスポンスのボディ / 異常：null</returns>
        public HttpResponseMessage Post(ref string QueryResponce, string BinaryFile)
        {
            String requestEndPoint = this.requestAddress;
            var request = this.CreateRequest(HttpMethod.Post, requestEndPoint);

            var content = new StringContent(QueryResponce, Encoding.UTF8, @"application/json");

            request.Content = content;

            string resBodyStr = "";
            HttpStatusCode resStatusCoode = HttpStatusCode.NotFound;

            Task<HttpResponseMessage> response;
            try
            {
                response = httpClient.SendAsync(request);

                if (!string.IsNullOrEmpty(QueryResponce))
                {
                    var stream = response.Result.Content.ReadAsStreamAsync().Result;

                    using (var fileStream = File.Create(BinaryFile))
                    {
                        using (var httpStream = response.Result.Content.ReadAsStreamAsync())
                        {
                            stream.CopyTo(fileStream);
                            fileStream.Flush();
                            resBodyStr = "ファイル出力OK";
                        }
                    }
                }
                else
                {
                    resBodyStr = response.Result.Content.ReadAsStringAsync().Result;
                }
                resStatusCoode = response.Result.StatusCode;
            }
            catch (HttpRequestException e)
            {
                // UNDONE: 通信失敗のエラー処理
                return null;
            }

            if (!resStatusCoode.Equals(HttpStatusCode.OK))
            {
                // UNDONE: レスポンスが200 OK以外の場合のエラー処理
                return null;
            }
            if (String.IsNullOrEmpty(resBodyStr))
            {
                // UNDONE: レスポンスのボディが空の場合のエラー処理
                return null;
            }
            // 中身を取り出したりして終了
            QueryResponce = resBodyStr;
            return response.Result;
        }

        /*
        /// <summary>
        /// オプションを設定します。内部メソッドです。
        /// </summary>
        /// <returns>JsonSerializerOptions型のオプション</returns>
        private JsonSerializerOptions GetJsonOption()
        {
            // ユニコードのレンジ指定で日本語も正しく表示、インデントされるように指定
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            return options;
        }
        */

        /// <summary>
        /// HTTPリクエストメッセージを生成する内部メソッドです。
        /// </summary>
        /// <param name="httpMethod">HTTPメソッドのオブジェクト</param>
        /// <param name="requestEndPoint">通信先のURL</param>
        /// <returns>HttpRequestMessage</returns>
        private HttpRequestMessage CreateRequest(HttpMethod httpMethod, string requestEndPoint)
        {
            var request = new HttpRequestMessage(httpMethod, requestEndPoint);
            return this.AddHeaders(request);
        }

        /// <summary>
        /// HTTPリクエストにヘッダーを追加する内部メソッドです。
        /// </summary>
        /// <param name="request">リクエスト</param>
        /// <returns>HttpRequestMessage</returns>
        private HttpRequestMessage AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Charset", "utf-8");
            // 同じようにして、例えば認証通過後のトークンが "Authorization: Bearer {トークンの文字列}"
            // のように必要なら適宜追加していきます。
            return request;
        }
    }
}
