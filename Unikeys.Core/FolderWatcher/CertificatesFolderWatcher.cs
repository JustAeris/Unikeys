namespace Unikeys.Core.FolderWatcher;

public sealed class CertificatesFolderWatcher
{
    public IList<FileInfo> CertificatesList { get; private set; }
    private readonly string _certificatesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Certificates");

    private FileSystemWatcher? FileSystemWatcher { get; set; }

    public CertificatesFolderWatcher()
    {
        CertificatesList = Directory.GetFiles(_certificatesFolder, "*.pfx", SearchOption.AllDirectories)
            .Select(s => new FileInfo(s)).ToList();

        FileSystemWatcher = new FileSystemWatcher
        {
            Path = _certificatesFolder,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            Filter = "*.pfx",
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        FileSystemWatcher.Created += OnFileChanged;
    }

    public event EventHandler<CertificateListUpdatedEventArgs>? CertificatesListUpdated;

    public void Start()
    {
        if (FileSystemWatcher == null) return;
        FileSystemWatcher.EnableRaisingEvents = true;

        while (true)
        {
            var unused = FileSystemWatcher.WaitForChanged(WatcherChangeTypes.All);
            OnFileChanged(null!, EventArgs.Empty);
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private void OnFileChanged(object sender, EventArgs e)
    {
        var dic = Directory.GetFiles(_certificatesFolder, "*.pfx", SearchOption.AllDirectories)
                             .Select(s => new FileInfo(s)).ToList();

        CertificatesList = dic;

        OnCertificatesListUpdated(new CertificateListUpdatedEventArgs(dic));
    }

    private void OnCertificatesListUpdated(CertificateListUpdatedEventArgs e)
    {
        CertificatesListUpdated?.Invoke(this, e);
    }
}
