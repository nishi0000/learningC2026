using System.Text;
using System.Text.Json;

namespace BoardApp;

class CsvExporter
{

    public void CreateCsv(List<Post> posts)
    {
        string csvPath = @"posts.csv";
        UTF8Encoding utf = new UTF8Encoding(true);

        string text = "\"Id\",\"投稿者\",\"メッセージ\",\"投稿日時\"";

        if(posts is null) return;

        foreach (Post post in posts)
        {
            text += $"\r\n\"{post.Id}\",\"{EscapeCsv(post.Author)}\",\"{EscapeCsv(post.Message)}\",\"{post.CreatedAt}\"";
            
        }
        
        File.WriteAllText(csvPath, text,utf);
    }

    private string EscapeCsv(string? value)
    {
        if (value == null) return "";

        return value.Replace("\"", "\"\"");
    }
    

}
