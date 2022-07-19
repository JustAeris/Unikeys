namespace Unikeys.Core.FolderWatcher;

public class CertificateListUpdatedEventArgs : EventArgs
{
    public List<FileInfo> CertificatesList { get; }

    public CertificateListUpdatedEventArgs(List<FileInfo> certificatesList)
    {
        CertificatesList = certificatesList;
    }
}
