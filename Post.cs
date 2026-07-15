namespace BoardApp;

class Post
{
    public int Id {get;set;}
    public string Author {get;set;}
    public string Message {get;set;}
    public DateTime CreatedAt {get;set;}

    public Post(string author,string message)
    {   
        Author = author;
        Message = message;
    }

    

}
