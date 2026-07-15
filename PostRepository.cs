using System.Text.Json;

namespace BoardApp;

class PostRepository
{
    private List<Post> posts = [];
    string path = @"posts.json";

    public PostRepository()
    {
        posts = ReadJson();
    }

    private List<Post> ReadJson()
    {
        if (File.Exists(path))
        {
            var load = JsonSerializer.Deserialize<List<Post>>(File.ReadAllText(path));
            return load ?? [];
        }

        return [];
    }

    public List<Post> GetAll()
    {
        return posts;
    }

    public bool Delete(int id)
    {
        if (posts.FirstOrDefault(x => x.Id == id) is null)
        {
            return false;
        }
        else
        {
            posts.RemoveAll(x => x.Id == id);
            WriteJson();
            return true;
        }
    }

    public void Add(string name, string message)
    {
        int id;
        if (posts.Count == 0)
        {
            id = 1;
        }
        else
        {
            id = posts.Select(x => x.Id).Max() + 1;
        }

        posts.Add(new Post(name, message) { Id = id, CreatedAt = DateTime.Now });
        WriteJson();

    }
    public List<Post> Search(string keyword)
    {
        return posts.Where(x => x.Author.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                    x.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
    }

    private void WriteJson()
    {
        // インデント（整形）を有効にする
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(path, JsonSerializer.Serialize(this.posts, options));

    }

}
