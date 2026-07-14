using System.Text.Json;

namespace BoardApp;

class Program
{
    static List<Post> posts = [];

    static void Main()
    {
        // インデント（整形）を有効にする
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true 
        };

        string path = @"posts.json";

        if(File.Exists(path))
        {
            var load = JsonSerializer.Deserialize<List<Post>>(File.ReadAllText(path));
            posts = load ?? [];
        }

        do 
        {
            int choice = AskChoice();
            if (choice == 1)
            {
                if (posts.Count() <= 0)
                {
                    
                    Console.WriteLine($"一覧なし！"); 
                }
                else
                {
                    ShowPosts(posts);
                }
            }
            else if(choice == 2)
            {
                string name = InputMessage("名前");
                string message = InputMessage("メッセージ");
                int id;
                if(posts.Count == 0)
                {
                    id = 1;
                }
                else
                {
                    id = posts.Select(x => x.Id).Max() + 1;
                }

                posts.Add(new Post(name,message) {Id=id,CreatedAt=DateTime.Now});
                File.WriteAllText(path,JsonSerializer.Serialize(posts,options));
                ShowPosts(posts);
            }
            else
            {
                break;
            }

        }while(true);
       
    }

    static void ShowPosts(List<Post> posts)
    {
        foreach(Post element in posts){
            Console.WriteLine($"[{element.Id}] {element.CreatedAt.ToString("yyyy/MM/dd HH:mm")} {element.Author}: {element.Message}"); 
        }
        
    }

    static string InputMessage(string item)
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
            
        }while(true);
        
    }

    static int AskChoice()
    {
        int userChoice;

        while (true)
        {
            Console.WriteLine("数値を入力してください（1: 一覧 / 2: 投稿 / 0: 終了）");
            var input = Console.ReadLine();

            if(int.TryParse(input, out int choice))
            {
                if( choice < 0 || 2 < choice)
                {
                    Console.WriteLine("0〜2 を入力してください");
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

}
