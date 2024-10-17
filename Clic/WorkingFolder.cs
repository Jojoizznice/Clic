namespace Clic;

public class WorkingFolder
{
    string path;
    
    public WorkingFolder(string? path)
    {
        path ??= Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        path = FormalizePathString(path);

        if (!Path.Exists(path))
        {
            throw new ArgumentException($"Path {path} doesn't exist", nameof(path));
        }

        this.path = path;
    }

    public Task<bool> SetPath(string path)
    {   
        path = FormalizePathString(path);
        
        if (!Path.Exists(path))
        {
            return Task.FromResult(false);
        }

        this.path = path;
        return Task.FromResult(true);
    }

    public Task<string> GetPath()
    {
        return Task.FromResult(path);
    }

    private static string FormalizePathString(string path)
     => path
        .Replace("/", "\\")
        .Trim()
        [..^(path.EndsWith('\\') ? 1 : 0)];
}
