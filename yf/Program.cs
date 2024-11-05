using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;

namespace yf
{
    internal class CompanyInfo
    {
        public string Title { get; set; }
        public string MarketCap { get; set; }
        public string DividendYield { get; set; }
        public string SelfCapitalizationRatio { get; set; }
        public string Feature { get; set; }
        public string FoundedDate { get; set; }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("CSVファイルのファイル名を指定してください。");
                return;
            }

            string csvFileName = args[0];
            var companyInfoList = new List<CompanyInfo>();
            var random = new Random();

            for (int i = 0; i < 10; i++)
            {
                string tickerSymbol = (1332 + i).ToString();
                var companyInfo = new CompanyInfo();

                // URLとデータ取得情報をまとめたリスト
                var urlDataMappings = new List<(string url, Dictionary<string, Action<string>> dataMappings)>
                {
                    (
                        $"https://finance.yahoo.co.jp/quote/{tickerSymbol}.T",
                        new Dictionary<string, Action<string>>
                        {
                            { "//title", data => companyInfo.Title = data },
                            { "//dt[span[contains(text(), '時価総額')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.MarketCap = data },
                            { "//dt[span[contains(text(), '配当利回り')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.DividendYield = data },
                            { "//dt[span[contains(text(), '自己資本比率')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.SelfCapitalizationRatio = data }
                        }
                    ),
                    (
                        $"https://finance.yahoo.co.jp/quote/{tickerSymbol}.T/profile",
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

                int waitTime = random.Next(0, 20000);   // 平均時10秒間隔
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
                    var values = properties.Select(p => RemoveCommas(p.GetValue(company)?.ToString() ?? ""));
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
    }
}
