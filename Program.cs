
namespace BoardApp;

class Program
{
    static void Main()
    {
        var postRepository = new PostRepository();
        var consoleMenu = new ConsoleMenu(postRepository);
        consoleMenu.ShowMenu();
    }

}
