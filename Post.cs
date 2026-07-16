namespace BoardApp;

/// <summary>
/// 掲示板の1件の投稿を表す。
/// </summary>
class Post
{
    public int Id { get; init; }
    public string Author { get; init; }
    public string Message { get; init; }
    public DateTime CreatedAt { get; init; }

    // ---- Before（元コード） ----
    // public Post(string author, string message)
    // {
    //     Author = author;
    //     Message = message;
    // }
    // → Id と CreatedAt は呼び出し側が
    //   new Post(name, message) { Id = id, CreatedAt = DateTime.Now }
    //   のようにオブジェクト初期化子で後から埋めていた。
    //   「Id を設定し忘れる」「CreatedAt を設定し忘れる」といったミスがあっても
    //   コンパイルは通ってしまう（実行時に初期値のまま気づかない、というバグになりやすい）。
    //
    // ---- After（このファイル） ----
    // 4つ全部をコンストラクタの必須パラメータにした。
    // これで「設定し忘れ」はコンパイルエラーになる。
    // また get だけでなく init にして、生成後にプロパティを書き換えられないようにした
    // （投稿は「作られた後に中身が変わらない」データなので、書き換え不可を型で表現できる）。
    //
    // 調べるキーワード: 「init アクセサー」「イミュータブル オブジェクト」
    public Post(int id, string author, string message, DateTime createdAt)
    {
        Id = id;
        Author = author;
        Message = message;
        CreatedAt = createdAt;
    }

    // 補足: System.Text.Json は「コンストラクタが1つだけ」なら、
    // プロパティ名とパラメータ名を突き合わせて自動でこのコンストラクタを使って
    // デシリアライズしてくれる（.NET Core 3.0以降）。
    // そのため PostRepository 側の JSON 読み込みコードは変更不要。
}
