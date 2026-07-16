namespace BoardApp;

// ---- Before ----
// switch (choice) { case 1: ... case 2: ... }
// AskChoice() の中で "if (choice < 0 || 5 < choice)" のように 0〜5 という
// マジックナンバーが2箇所（メニュー表示の文言・範囲チェック）に散らばっていた。
// メニューを1つ増減するたびに、複数箇所を直し忘れるリスクがある。
//
// ---- After ----
// 選択肢を enum にまとめた。switch も enum に対して書けるので可読性が上がる。
// （個数チェックは Enum.IsDefined で済ませられる）
//
// 調べるキーワード: 「enum」「Enum.IsDefined」
enum MenuChoice
{
    Exit = 0,
    ShowList = 1,
    AddPost = 2,
    Search = 3,
    Delete = 4,
    ExportCsv = 5,
}

class ConsoleMenu
{
    private readonly PostRepository _postRepository;
    private readonly CsvExporter _csvExporter;

    public ConsoleMenu(PostRepository postRepository, CsvExporter csvExporter)
    {
        _postRepository = postRepository;
        _csvExporter = csvExporter;
    }

    public void ShowMenu()
    {
        // ---- Before ----
        // do
        // {
        //     List<Post> posts = _postRepository.GetAll();
        //     int choice = AskChoice();
        //     switch (choice)
        //     {
        //         case 2: ... _postRepository.Add(name, message); ShowPosts(posts); break; // ← posts は古いまま
        //
        // → GetAll() をループ先頭で1回だけ呼び、その posts をAdd/Delete後の表示にも使い回していた。
        //   これまでは PostRepository.GetAll() が内部Listをそのまま返していた（参照共有）ため、
        //   たまたま最新の状態が見えていただけ。PostRepository側でToList()による防御的コピーに
        //   変えると、この posts はもう更新されず「表示だけ古いまま」になってしまう。
        //
        // ---- After ----
        // 「一覧表示に使うposts」は、表示する直前に毎回 GetAll() で取り直す。
        // 「古いリストを使い回さない」というルールに統一した。
        //
        // 調べるキーワード: 「参照型と値の共有」「防御的コピーとその影響」
        while (true)
        {
            var choice = AskChoice();

            switch (choice)
            {
                case MenuChoice.ShowList:
                    var posts = _postRepository.GetAll();
                    if (posts.Count == 0)
                    {
                        Console.WriteLine("一覧なし！");
                    }
                    else
                    {
                        ShowPosts(posts);
                    }
                    break;

                case MenuChoice.AddPost:
                    string name = InputMessage("名前");
                    string message = InputMessage("メッセージ");
                    _postRepository.Add(name, message);
                    ShowPosts(_postRepository.GetAll());
                    break;

                case MenuChoice.Search:
                    string inputKeyword = InputMessage("検索したい語句");
                    List<Post> searchPosts = _postRepository.Search(inputKeyword);

                    if (searchPosts.Count == 0)
                    {
                        Console.WriteLine("見つかりませんでした");
                    }
                    else
                    {
                        ShowPosts(searchPosts);
                    }
                    break;

                case MenuChoice.Delete:
                    HandleDelete();
                    break;

                case MenuChoice.ExportCsv:
                    Console.WriteLine("CSV出力します");
                    _csvExporter.Export(_postRepository.GetAll());
                    break;

                case MenuChoice.Exit:
                    return;
            }
        }
    }

    // ---- Before ----
    // case 4:
    //     while (true)
    //     {
    //         string inputId = InputMessage("削除したいID");
    //         if (int.TryParse(inputId, out int id))
    //         {
    //             Console.WriteLine("本当に削除しますか？ y/n ...");
    //             string? answer = Console.ReadLine();
    //             if (answer == "y") { ... break; }
    //             else if (answer == "n") { }
    //             else { Console.WriteLine("yかnを入力してください"); }
    //         }
    //         else { Console.WriteLine("数値を入力してください"); break; }
    //     }
    //     break;
    // → 「IDの入力」と「y/n確認」が同じwhileループの中に混ざっていた。
    //   そのため「y でも n でもない答え」をした時、もう一度y/nを聞き直したいはずが、
    //   ループの先頭に戻って「削除したいID」から聞き直してしまう（軽微な挙動のクセ）。
    //   また「数値じゃない入力」のときだけbreakして抜けてしまい、他の異常系と扱いが不揃い。
    //
    // ---- After ----
    // 「IDを聞く」「y/n を聞く」を別々の小さいメソッドに分けた。
    // それぞれが「1つのことだけ」を繰り返すシンプルなループになる。
    //
    // 調べるキーワード: 「メソッドを小さく分ける利点」「関心の分離」
    //
    // ---- 追加の指摘への対応 ----
    // 「IDだけ見せられても、それが本当に消したい投稿か分からない」という指摘。
    // → 確認を出す前に、対象の投稿を1件だけ取得して中身（投稿者・メッセージ・日時）を
    //   表示するようにした。ここで対象が見つからなければ、y/nを聞くまでもなく
    //   「存在しないIDです」で即終了できる（無駄な確認を1つ減らせる）。
    //
    // 調べるキーワード: 「FirstOrDefault」「早期リターン（ガード節）」
    private void HandleDelete()
    {
        int id = InputId("削除したいID");

        Post? target = _postRepository.GetAll().FirstOrDefault(p => p.Id == id);
        if (target is null)
        {
            Console.WriteLine("存在しないIDです");
            return;
        }

        Console.WriteLine("この投稿を削除します。");
        ShowPosts([target]);

        if (!Confirm($"本当に削除しますか？ y/n（削除ID：{id}）"))
        {
            return;
        }

        if (_postRepository.Delete(id))
        {
            ShowPosts(_postRepository.GetAll());
        }
        else
        {
            // 通常はここに来ない（直前でtargetの存在を確認済みのため）。
            // ただし「確認しているうちに他の経路で削除された」ような状況にも
            // 対応できるよう、念のため残している。
            Console.WriteLine("存在しないIDです");
        }
    }

    private int InputId(string item)
    {
        while (true)
        {
            Console.WriteLine($"{item}を入力してください。");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int id))
            {
                return id;
            }

            Console.WriteLine("数値を入力してください");
        }
    }

    private bool Confirm(string message)
    {
        while (true)
        {
            Console.WriteLine(message);
            string? answer = Console.ReadLine();

            if (answer == "y") return true;
            if (answer == "n") return false;

            Console.WriteLine("yかnを入力してください");
        }
    }

    private MenuChoice AskChoice()
    {
        while (true)
        {
            Console.WriteLine("数値を入力してください（1: 一覧 / 2: 投稿 / 3: 検索 / 4: 削除 / 5: CSV出力 / 0: 終了）");
            var input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && Enum.IsDefined(typeof(MenuChoice), choice))
            {
                return (MenuChoice)choice;
            }

            Console.WriteLine("0〜5 を入力してください");
        }
    }

    private void ShowPosts(List<Post> posts)
    {
        foreach (Post post in posts)
        {
            Console.WriteLine($"[{post.Id}] {post.CreatedAt:yyyy/MM/dd HH:mm} {post.Author}: {post.Message}");
        }
    }

    private string InputMessage(string item)
    {
        while (true)
        {
            Console.WriteLine($"{item}を入力してください。");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("入力は必須です。");
            }
            else
            {
                return input;
            }
        }
    }
}
