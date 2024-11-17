﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace yf
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 3)
            {
                return;
            }

            var inputCsvFileName = args[0];
            var minInterval = args[1];
            var maxInterval = args[2];

            var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var outputCsvFileName = Path.Combine(folderPath, "res.csv");
            var companyInfoList = new List<CompanyInfo>();

            // 入力CSVから証券コードを読み取る
            using (var reader = new StreamReader(inputCsvFileName))
            {
                string line;
                reader.ReadLine(); // ヘッダー行をスキップ
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.Split(',');
                    if (columns.Length < 2) continue;
                    if (columns[4].Trim() == "-") continue; // ETF関連はスキップ

                    var currentCode = columns[1].Trim(); // B列からコードを取得
                    var companyInfo = new CompanyInfo();
                    var idr = new InverseDistributionRandom(2, int.Parse(maxInterval));

                    Console.WriteLine($"{currentCode}");


                    // URLとデータ取得情報をまとめたリスト
                    var urlDataMappings = new List<(string url, Dictionary<string, Action<string>> dataMappings)>
                    {
                        (
                            $"https://finance.yahoo.co.jp/quote/{currentCode}.T",
                            new Dictionary<string, Action<string>>
                            {
                                //{ "//title", data => {
                                //    companyInfo.Code = GetTextBetween(data,'【','】');
                                //    companyInfo.Name = GetTextBefore(data, '【');
                                //}},
                                { "//title", data => {companyInfo.Code = GetTextBetween(data,'【','】');}},
                                { "//dt[span[contains(text(), '時価総額')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.MarketCap = data },
                                { "//dt[span[contains(text(), '配当利回り')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.DividendYield = data },
                                { "//dt[span[contains(text(), 'PER')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.PER = data },
                                { "//dt[span[contains(text(), 'ROE')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.ROE = data },
                                { "//dt[span[contains(text(), '自己資本比率')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.SelfCapitalizationRatio = data },
                                { "//dt[span[contains(text(), '前日終値')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']", data => companyInfo.PreviousClose = data }
                            }
                        ),
                        (
                            $"https://finance.yahoo.co.jp/quote/{currentCode}.T/profile",
                            new Dictionary<string, Action<string>>
                            {
                                { "//tr[th[contains(text(), '特色')]]/td", data => companyInfo.Feature = data },
                                { "//tr[th[contains(text(), '上場年月日')]]/td", data => companyInfo.FoundedDate = data }
                            }
                        ),
                        (
                            $"https://www.nikkei.com/nkd/company/kessan/?scode={currentCode}",
                            new Dictionary<string, Action<string>>
                            {
                                // 売上高と営業利益のXPath指定
                                { "//th[contains(text(),'売上高')]/following-sibling::td[last()]", data => companyInfo.Revenue = data },
                                { "//th[contains(text(),'営業利益')]/following-sibling::td[last()]", data => companyInfo.OperatingProfit = data }
                            }
                        ),

                    };

                    // URLとXPathを基にデータ取得
                    foreach (var (url, dataMappings) in urlDataMappings)
                    {
                        await FetchDataAsync(url, dataMappings);
                    }

                    companyInfoList.Add(companyInfo);

                    int waitTime = idr.Next() * 1000;
                    //Console.WriteLine($"wait {waitTime / 1000} sec.");
                    Thread.Sleep(waitTime);
                }
            }

            SaveToCsv(companyInfoList, outputCsvFileName);
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
                    assignAction(node?.InnerText.Trim() ?? "");
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
                    // 証券コードが入っていなければ、このレコードをスキップ
                    var codeProperty = properties.FirstOrDefault(p => p.Name == "Code");
                    var codeValue = codeProperty?.GetValue(company)?.ToString() ?? "";
                    if (string.IsNullOrEmpty(codeValue))
                    {
                        continue;
                    }

                    // プロパティの値を取得し、カンマを除去してCSVに出力
                    var values = properties.Select(p => RemoveCommas(p.GetValue(company)?.ToString() ?? "")).ToList();
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
