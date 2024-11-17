using System;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RegexReplace
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // コマンドライン引数のチェック
            if (args.Length != 4)
            {
                Console.WriteLine("引数が不足しています。");
                return;
            }

            string csvFileName = args[0];
            string pattern = args[1]; // 正規表現パターン
            string replacementText = args[2];
            string resultFileName = args[3];

            try
            {
                // CSVファイルの存在確認
                if (!File.Exists(csvFileName))
                {
                    Console.WriteLine($"指定されたCSVファイル '{csvFileName}' は存在しません。");
                    return;
                }

                // CSVファイルを読み込み、置換処理を実行
                using (var reader = new StreamReader(csvFileName))
                using (var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false // ヘッダーがない場合はこれを設定
                }))
                using (var writer = new StreamWriter(resultFileName))
                using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false
                }))
                {
                    Regex regex = new Regex(pattern);

                    while (csvReader.Read())
                    {
                        // 現在の行を取得
                        var row = csvReader.Parser.Record;

                        // 各セルに正規表現を適用
                        for (int i = 0; i < row.Length; i++)
                        {
                            row[i] = regex.Replace(row[i], replacementText);
                        }

                        // 行をリストとして書き込み
                        csvWriter.WriteField(row); // フィールドごとに書き込む
                        csvWriter.NextRecord();    // 次のレコードに移動
                    }
                }

                Console.WriteLine($"置換処理が完了しました。結果は '{resultFileName}' に保存されました。");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("無効な正規表現が指定されました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");
            }
        }
    }
}
