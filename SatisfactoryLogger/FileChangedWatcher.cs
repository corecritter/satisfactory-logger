namespace SatisfactoryLogger;

public interface IFileChangedWatcher
{
    Task Start(CancellationToken cancellationToken);
    Task<string> WaitForFileChange(CancellationToken cancellationToken);
}

public class FileChangedWatcher : IFileChangedWatcher
{
    private readonly AppSettings appSettings;
    private readonly List<string> changedFiles = new List<string>();

    public FileChangedWatcher(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        using var watcher = new FileSystemWatcher(appSettings.FileOptions.SatisfactoryLogDirectory);

        watcher.NotifyFilter =
            NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;

        watcher.Changed += this.OnChanged;
        watcher.Created += this.OnCreated;
        watcher.Renamed += this.OnRenamed;

        watcher.Filter = "*.*";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public async Task<string> WaitForFileChange(CancellationToken cancellationToken)
    {
        var changedFile = string.Empty;
        while (!cancellationToken.IsCancellationRequested)
        {
            lock (changedFiles)
            {
                if (this.changedFiles.Any())
                {
                    changedFile = this.changedFiles[0];
                    this.changedFiles.RemoveAt(0);
                    return changedFile;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        return changedFile;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (this.changedFiles)
        {
            this.changedFiles.Add(e.FullPath);
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        lock (this.changedFiles)
        {
            this.changedFiles.Add(e.FullPath);
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        lock (this.changedFiles)
        {
            this.changedFiles.Add(e.FullPath);
        }
    }
}
