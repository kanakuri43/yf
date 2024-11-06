using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;

namespace yf
{


    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }

            string firstCode = args[0];
            string csvFileName = args[1];
            var companyInfoList = new List<CompanyInfo>();
            var random = new Random();

            for (int i = 0; i < 10; i++)
            {
                var currentCode = (int.Parse(firstCode) + i).ToString();
                var companyInfo = new CompanyInfo();

                // URLとデータ取得情報をまとめたリスト
                var urlDataMappings = new List<(string url, Dictionary<string, Action<string>> dataMappings)>
                {
                    (
                        $"https://finance.yahoo.co.jp/quote/{currentCode}.T",
                        new Dictionary<string, Action<string>>
                        {
                            { "//title", data => {
                                companyInfo.Code = GetTextBetween(data,'【','】');
                                companyInfo.Name = GetTextBefore(data, '【'); // Nameの取得
                            }},
                            { "//dt[span[contains(text(), '時価総額')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.MarketCap = data },
                            { "//dt[span[contains(text(), '配当利回り')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.DividendYield = data },
                            { "//dt[span[contains(text(), '自己資本比率')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.SelfCapitalizationRatio = data }
                        }
                    ),
                    (
                        $"https://finance.yahoo.co.jp/quote/{currentCode}.T/profile",
                        new Dictionary<string, Action<string>>
                        {
                            { "//tr[th[contains(text(), '特色')]]/td", data => companyInfo.Feature = data },
                            { "//tr[th[contains(text(), '上場年月日')]]/td", data => companyInfo.FoundedDate = data }
                        }
                    )
                };

                // URLとXPathを基にデータ取得
                foreach (var (url, dataMappings) in urlDataMappings)
                {
                    await FetchDataAsync(url, dataMappings);
                }

                companyInfoList.Add(companyInfo);

                int waitTime = random.Next(0, 2000);   // 平均1秒間隔
                Thread.Sleep(waitTime);
            }

            SaveToCsv(companyInfoList, csvFileName);
        }

        static async Task FetchDataAsync(string url, Dictionary<string, Action<string>> dataMappings)
        {
            var html = await GetHtmlAsync(url);
            if (html != null)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                foreach (var (xpath, assignAction) in dataMappings)
                {
                    var node = htmlDoc.DocumentNode.SelectSingleNode(xpath);
                    assignAction(node?.InnerText.Trim() ?? "N/A");
                }
            }
        }

        static async Task<string> GetHtmlAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();

                // ユーザーエージェントを設定
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");

                return await httpClient.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine($"リクエストエラー（{url}）: {e.Message}");
            }
            return null;
        }

        static void SaveToCsv(List<CompanyInfo> companyInfoList, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                // プロパティ名を取得してヘッダー行を出力
                var properties = typeof(CompanyInfo).GetProperties();
                writer.WriteLine(string.Join(",", properties.Select(p => p.Name)));

                // 各オブジェクトのプロパティ値を取得してCSVに出力
                foreach (var company in companyInfoList)
                {
                    // すべてのプロパティがnullまたは空であるかを確認
                    var values = properties.Select(p => RemoveCommas(p.GetValue(company)?.ToString() ?? "")).ToList();
                    if (values.All(string.IsNullOrEmpty))
                    {
                        // すべてがnullまたは空の場合、このレコードをスキップ
                        continue;
                    }

                    writer.WriteLine(string.Join(",", values));
                }
            }
            Console.WriteLine($"CSVファイル '{fileName}' に出力しました。");
        }

        // カンマを除去するヘルパーメソッド
        static string RemoveCommas(string input)
        {
            return string.IsNullOrEmpty(input) ? input : input.Replace(",", "");
        }

        // 指定した文字に囲まれた中身を取得する
        public static string GetTextBetween(string input, char startChar, char endChar)
        {
            int startIndex = input.IndexOf(startChar);
            int endIndex = input.IndexOf(endChar, startIndex + 1);

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                return input.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            return string.Empty; // 囲まれた文字列がない場合は空文字を返す
        }

        // 指定した文字の左側を取得する
        public static string GetTextBefore(string input, char targetChar)
        {
            int index = input.IndexOf(targetChar);

            if (index != -1)
            {
                return input.Substring(0, index);
            }

            return input; // 指定の文字が見つからない場合は元の文字列を返す
        }
    }
}
