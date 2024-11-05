using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yf
{
    internal class StockModel
    {
        // プロパティ: 証券コード
        public string TickerSymbol { get; set; }

        // プロパティ: 企業名
        public string CompanyName { get; set; }

        // プロパティ: 時価総額
        public decimal MarketCap { get; set; }

        // プロパティ: 自己資本比率（%）
        public double EquityRatio { get; set; }

        // プロパティ: 上場年
        public int ListingYear { get; set; }


        //// コンストラクタ
        //public Stock(string tickerSymbol, string companyName, decimal marketCap, double equityRatio, int listingYear)
        //{
        //    TickerSymbol = tickerSymbol;
        //    CompanyName = companyName;
        //    MarketCap = marketCap;
        //    EquityRatio = equityRatio;
        //    ListingYear = listingYear;
        //}

        //// 会社の概要を表示するメソッド
        //public void DisplaySummary()
        //{
        //    Console.WriteLine($"Ticker Symbol: {TickerSymbol}");
        //    Console.WriteLine($"Company Name: {CompanyName}");
        //    Console.WriteLine($"Market Cap: {MarketCap} USD");
        //    Console.WriteLine($"Equity Ratio: {EquityRatio}%");
        //    Console.WriteLine($"Listing Year: {ListingYear}");
        //    Console.WriteLine($"Industry: {Industry}");
        //}
    }
}

