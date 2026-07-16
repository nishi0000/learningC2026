using System.Text;

namespace BoardApp;

class CsvExporter
{
    private const string Header = "Id,投稿者,メッセージ,投稿日時";

    // ---- Before ----
    // public void CreateCsv(List<Post> posts)
    // {
    //     string text = "\"Id\",\"投稿者\",\"メッセージ\",\"投稿日時\"";
    //     foreach (Post post in posts)
    //         text += $"\r\n\"{post.Id}\",\"{EscapeCsv(post.Author)}\",...";
    //     File.WriteAllText(csvPath, text, utf);
    // }
    // → 「CSV文字列を組み立てる」と「ファイルに書き出す」が1つのメソッドに同居していた。
    //   このままだと、組み立て結果だけを単体テストしたい時にファイルシステムが絡んでしまう。
    //   また文字列 += を繰り返すのは、件数が増えるほど新しい文字列を何度も作り直すことになり非効率
    //   （StringBuilder ならバッファに追記していくだけで済む）。
    //
    // ---- After ----
    // 「組み立て（BuildCsv）」と「書き出し（Export）」を分けた。
    // 確認テストの「List<Post>を受け取ってCSV文字列を組み立てる」はBuildCsvだけで完結する。
    //
    // 調べるキーワード: 「StringBuilder」「単一責任の原則」「メソッドを分ける利点」
    public string BuildCsv(List<Post> posts)
    {
        var sb = new StringBuilder();
        sb.Append(Header);

        foreach (Post post in posts)
        {
            sb.Append("\r\n");
            sb.Append(post.Id);
            sb.Append(',');
            sb.Append(EscapeField(post.Author));
            sb.Append(',');
            sb.Append(EscapeField(post.Message));
            sb.Append(',');
            sb.Append(post.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        return sb.ToString();
    }

    public void Export(List<Post> posts, string path = "posts.csv")
    {
        string csv = BuildCsv(posts);
        File.WriteAllText(path, csv, new UTF8Encoding(true));
    }

    // ---- Before ----
    // private string EscapeCsv(string? value)
    // {
    //     if (value == null) return "";
    //     return value.Replace("\"", "\"\"");
    // }
    // → 呼び出し側であらゆるフィールドを常に "" で囲んでいた。
    //   カンマ・改行・"" のどれも含まない、ごく普通のフィールド（例: "de"）まで
    //   律儀にクォートされてしまい、CSVとしては余計なノイズになる
    //   （面談での「常時クォートのデメリットは？」という問いの答えの一つ）。
    //
    // ---- After ----
    // 「カンマ・改行・ダブルクォートのどれかを含む時だけ」クォートで囲む、
    // RFC4180が想定する一般的なCSVの書き方にした。含む場合だけ、中の " を "" に二重化する。
    //
    // 調べるキーワード: 「RFC 4180」「CSV クォート 条件付き」
    private string EscapeField(string? value)
    {
        value ??= "";

        bool needsQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuote) return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
