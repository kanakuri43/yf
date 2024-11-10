using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yf
{
    internal class CompanyInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string MarketCap { get; set; }
        public string DividendYield { get; set; }
        public string PER { get; set; }
        public string ROE { get; set; }
        public string SelfCapitalizationRatio { get; set; }
        public string Feature { get; set; }
        public string FoundedDate { get; set; }
        public string FoundedYear => ExtractYearFromDate(FoundedDate); // 西暦年を取得
        public string PreviousClose { get; set; } // 前日終値



        // 年を抽出するためのヘルパーメソッド
        private static string ExtractYearFromDate(string date)
        {
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                return parsedDate.Year.ToString(); // 年のみを取得
            }
            return "N/A"; // パース失敗時
        }
    }
}

