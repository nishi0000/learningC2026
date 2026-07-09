namespace BoardApp;

class Post
{
    int Id {get;set;}
    string? Author {get;set;}

    string? Message {get;set;}

    DateTime CreatedAt {get;set;}

    List<Post> post = [{Id=1,"test","test",DateTime.Now}];

    static void Main()
    {
        Console.WriteLine("Hello, World!");
    }

    void ShowPosts()
    {
        
    }

}
