
namespace BoardApp;

class ConsoleMenu
{
    PostRepository _postRepository;

    public ConsoleMenu(PostRepository postRepository)
    {

        _postRepository = postRepository;

    }

    public void ShowMenu()
    {
        List<Post> posts = _postRepository.GetAll();

        do
        {
            int choice = AskChoice();

            switch (choice)
            {
                case 1:
                    if (posts.Count() <= 0)
                    {
                        Console.WriteLine($"一覧なし！");
                        break;
                    }
                    else
                    {
                        ShowPosts(posts);
                        break;
                    }

                case 2:
                    string name = InputMessage("名前");
                    string message = InputMessage("メッセージ");

                    _postRepository.Add(name, message);
                    ShowPosts(posts);
                    break;

                case 3:
                    string inputKeyword = InputMessage("検索したい語句");
                    List<Post> searchPosts = _postRepository.Search(inputKeyword);

                    if (searchPosts.Count() == 0)
                    {
                        Console.WriteLine("見つかりませんでした");
                    }
                    else
                    {
                        ShowPosts(searchPosts);
                    }
                    break;

                case 4:
                    while (true)
                    {
                        string inputId = InputMessage("削除したいID");

                        if (int.TryParse(inputId, out int id))
                        {
                            Console.WriteLine($"本当に削除しますか？ y/n 削除ID：{inputId}");
                            string? answer = Console.ReadLine();

                            if (answer == "y")
                            {
                                if (_postRepository.Delete(id))
                                {
                                    ShowPosts(posts);
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("存在しないIDです");
                                    break;
                                }
                            }
                            else if (answer == "n")
                            {

                            }
                            else
                            {
                                Console.WriteLine($"yかnを入力してください");
                            }

                        }
                        else
                        {
                            Console.WriteLine("数値を入力してください");
                            break;
                        }
                    }
                    break;

                default:
                    return;
            }

        } while (true);
    }
    private int AskChoice()
    {
        int userChoice;

        while (true)
        {
            Console.WriteLine("数値を入力してください（1: 一覧 / 2: 投稿 / 3: 検索 / 4: 削除 / 0: 終了）");
            var input = Console.ReadLine();

            if (int.TryParse(input, out int choice))
            {
                if (choice < 0 || 4 < choice)
                {
                    Console.WriteLine("0〜4 を入力してください");
                }
                else
                {
                    userChoice = choice;
                    break;
                }
            }
            else
            {
                Console.WriteLine("数値を入力してください");
            }
        }

        return userChoice;
    }

    private void ShowPosts(List<Post> posts)
    {
        foreach (Post element in posts)
        {
            Console.WriteLine($"[{element.Id}] {element.CreatedAt.ToString("yyyy/MM/dd HH:mm")} {element.Author}: {element.Message}");
        }

    }
    private string InputMessage(string item)
    {
        do
        {
            Console.WriteLine($"{item}を入力してください。");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"入力は必須です。");
            }
            else
            {
                return input;
            }

        } while (true);

    }

}
