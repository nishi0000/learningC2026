using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace BoardApp;

class PostRepository
{
    private readonly List<Post> _posts;
    private readonly string _path = "posts.json";

    // ---- Before ----
    // private void WriteJson()
    // {
    //     var options = new JsonSerializerOptions { WriteIndented = true };
    //     File.WriteAllText(path, JsonSerializer.Serialize(this.posts, options));
    // }
    // → 保存するたびに JsonSerializerOptions を new していた（軽微だが無駄な生成）。
    //   また既定のエンコーダーは日本語などの非ASCII文字を \uXXXX に変換してしまうため、
    //   posts.json を直接開いても日本語が読めない（Station4の確認テスト4「中身が読めるJSON」を
    //   厳密には満たしにくい）。
    //
    // ---- After ----
    // options を static readonly にして使い回し、Encoder を指定して日本語をそのまま出力する。
    // 調べるキーワード: 「JavaScriptEncoder」「UnicodeRanges」「JsonSerializerOptions 使い回し」
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public PostRepository()
    {
        _posts = ReadJson();
    }

    private List<Post> ReadJson()
    {
        if (File.Exists(_path))
        {
            var loaded = JsonSerializer.Deserialize<List<Post>>(File.ReadAllText(_path));
            return loaded ?? [];
        }

        return [];
    }

    // ---- Before ----
    // public List<Post> GetAll()
    // {
    //     return posts;
    // }
    // → 内部の List をそのまま返していた。呼び出し側（ConsoleMenu）が受け取った
    //   List に対して Add/Remove すると、リポジトリの内部状態まで一緒に壊れてしまう
    //   （参照型なので「同じ箱」を渡していることになる）。
    //   ConsoleMenu 側もこの「実は同じ箱」という性質にたまたま助けられて動いていた
    //   （Add後にShowPostsへ渡していた posts 変数が、内部で書き換わるので反映されて見えていた）。
    //
    // ---- After ----
    // ToList() でコピーを返す（防御的コピー）。呼び出し側が何をしても内部状態は守られる。
    // その代わり、ConsoleMenu 側は「追加・削除した後は改めて GetAll() を呼び直す」必要がある
    // （ConsoleMenu.cs のコメントも参照）。
    //
    // 調べるキーワード: 「防御的コピー defensive copy」「カプセル化」
    public List<Post> GetAll()
    {
        return _posts.ToList();
    }

    // ---- Before ----
    // public bool Delete(int id)
    // {
    //     if (posts.FirstOrDefault(x => x.Id == id) is null) return false;
    //     posts.RemoveAll(x => x.Id == id);
    //     WriteJson();
    //     return true;
    // }
    // → 存在確認の FirstOrDefault と削除の RemoveAll で、リストを2回探索していた。
    //
    // ---- After ----
    // RemoveAll は「削除した件数」を返してくれるので、それだけで存在有無も判定できる。
    // 探索が1回で済み、コードも短くなる。
    //
    // 調べるキーワード: 「List<T>.RemoveAll 戻り値」
    public bool Delete(int id)
    {
        int removedCount = _posts.RemoveAll(x => x.Id == id);
        if (removedCount == 0) return false;

        WriteJson();
        return true;
    }

    public void Add(string name, string message)
    {
        int id = _posts.Count == 0 ? 1 : _posts.Max(x => x.Id) + 1;

        // Before: new Post(name, message) { Id = id, CreatedAt = DateTime.Now }
        // After : Post のコンストラクタが4引数必須になったので、その場で全部渡す
        _posts.Add(new Post(id, name, message, DateTime.Now));
        WriteJson();
    }

    public List<Post> Search(string keyword)
    {
        return _posts.Where(x => x.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                  x.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                     .ToList();
    }

    private void WriteJson()
    {
        File.WriteAllText(_path, JsonSerializer.Serialize(_posts, JsonOptions));
    }
}
