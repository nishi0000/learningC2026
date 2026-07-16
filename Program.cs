namespace BoardApp;

class Program
{
    static void Main()
    {
        var postRepository = new PostRepository();
        var csvExporter = new CsvExporter();
        var consoleMenu = new ConsoleMenu(postRepository, csvExporter);
        consoleMenu.ShowMenu();
    }
}
