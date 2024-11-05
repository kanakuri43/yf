using System;
using System.Collections.Generic;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace yf
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string url = "";
            var random = new Random();
                

            string tickerSymbol = "";

            for (int i = 0; i < 3; i++)
            {
                tickerSymbol = (6223 + i).ToString();
                //////////
                // 財務状況

                url = $"https://finance.yahoo.co.jp/quote/{tickerSymbol}.T";
                var financeHtml = await GetHtmlAsync(url);

                if (financeHtml != null)
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(financeHtml);

                    // タイトルの取得
                    var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                    Console.WriteLine("Title: " + titleNode?.InnerText);

                    // 時価総額の取得
                    var MarketCap = htmlDoc.DocumentNode.SelectSingleNode("//dt[span[contains(text(), '時価総額')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']");
                    var issuedSharesNode = htmlDoc.DocumentNode.SelectSingleNode("//dt[span[contains(text(), '発行済株式数')]]/following-sibling::dd//span[@class='StyledNumber__value__3rXW DataListItem__value__11kV']");

                    Console.WriteLine("時価総額: " + MarketCap?.InnerText.Trim());
                    Console.WriteLine("発行済株式数: " + issuedSharesNode?.InnerText.Trim());

                }
                else
                {
                    Console.WriteLine($"URL: {url} のHTMLの取得に失敗しました。");
                }

                //////////
                // 企業概要

                url = $"https://finance.yahoo.co.jp/quote/{tickerSymbol}.T/profile";
                var outlineHtml = await GetHtmlAsync(url);

                if (outlineHtml != null)
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(outlineHtml);

                    // 特色の取得
                    var featureNode = htmlDoc.DocumentNode.SelectSingleNode("//tr[th[contains(text(), '特色')]]/td");
                    Console.WriteLine("特色: " + featureNode?.InnerText.Trim());

                    // 上場年月日の取得
                    var foundedNode = htmlDoc.DocumentNode.SelectSingleNode("//tr[th[contains(text(), '上場年月日')]]/td");
                    Console.WriteLine("設立年月日: " + foundedNode?.InnerText.Trim());
                }
                else
                {
                    Console.WriteLine($"URL: {url} のHTMLの取得に失敗しました。");
                }

                int waitTime = random.Next(0, 10000); // 0～10000ミリ秒
                Console.WriteLine($"次のリクエストまで {waitTime} ミリ秒待機します...");
                Thread.Sleep(waitTime);

                Console.WriteLine(); // URL間の区切り
            }
        }

        static async Task<string> GetHtmlAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"リクエストエラー（{url}）: {e.Message}");
            }
            catch (TaskCanceledException e)
            {
                // 存在しない証券コードの時はここ

                Console.WriteLine($"リクエストがタイムアウトしました（{url}）。");
            }
            catch (Exception e)
            {
                Console.WriteLine($"予期しないエラーが発生しました（{url}）: {e.Message}");
            }
            return null;
        }
    }
}
