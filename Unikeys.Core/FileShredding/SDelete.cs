using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace Unikeys.Core.FileShredding;

/// <summary>
/// This class uses the SysInternal tool SDelete to securely overwrite a file on the disk, making it impossible to recover in any case.
/// <see href="https://docs.microsoft.com/en-gb/sysinternals/downloads/sdelete">Source here</see>
/// </summary>
[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class SDelete : IDisposable
{
    private Process SDeleteProcess { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SDelete"/> class.
    /// </summary>
    /// <param name="runAsAdmin">If true, will try run SDelete with administrator privileges but this will disable specific errors detections.</param>
    public SDelete(bool runAsAdmin = false)
    {
        SDeleteProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.CurrentDirectory, $"{(Environment.Is64BitOperatingSystem ? $"Tools{Path.DirectorySeparatorChar}sdelete64.exe" : $"Tools{Path.DirectorySeparatorChar}sdelete.exe")}"),
                UseShellExecute = runAsAdmin,
                CreateNoWindow = true,
                RedirectStandardOutput = !runAsAdmin,
                Verb = runAsAdmin ? "runas" : "",
            }
        };

        var acceptedEula = (int)(Registry.GetValue(@"HKEY_CURRENT_USER\Software\Sysinternals\SDelete", "EulaAccepted", 0) ?? 0);

        if (acceptedEula != 0) return;

        SDeleteProcess.StartInfo.Arguments = "/accepteula";
        SDeleteProcess.Start();
        SDeleteProcess.WaitForExit();
    }

    /// <summary>
    /// Securely deletes a file using SDelete
    /// </summary>
    /// <param name="file">File to overwrite</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public async Task DeleteFile(FileInfo file, int passes = 1)
    {
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r \"{file.FullName}\"";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        if (SDeleteProcess.StartInfo.RedirectStandardOutput)
        {
            var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

            if (output.Contains("Access denied"))
                throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
            if (output.Contains("No files/folders found that match"))
                throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
        }
    }

    /// <summary>
    /// Securely deletes multiple files using SDelete
    /// </summary>
    /// <param name="files">Files to overwrite</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public async Task DeleteFiles(IEnumerable<FileInfo> files, int passes = 1)
    {
        var paths = string.Join(" ", files.Select(f => $"\"{f.FullName}\""));
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r {paths}";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        if (SDeleteProcess.StartInfo.RedirectStandardOutput)
        {
            var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

            if (output.Contains("Access is denied"))
                throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
            if (output.Contains("No files/folders found that match"))
                throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
        }
    }

    /// <summary>
    /// Securely deletes a directory using SDelete
    /// </summary>
    /// <param name="directory">Directory to delete</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public async Task DeleteDirectory(DirectoryInfo directory, int passes = 1)
    {
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r \"{directory.FullName}\"";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        if (SDeleteProcess.StartInfo.RedirectStandardOutput)
        {
            var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

            if (output.Contains("Access denied"))
                throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
            if (output.Contains("No files/folders found that match"))
                throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
        }
    }

    /// <summary>
    /// Securely deletes multiple directories using SDelete
    /// </summary>
    /// <param name="directories">Directories to delete</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public async Task DeleteDirectories(IEnumerable<DirectoryInfo> directories, int passes = 1)
    {
        var paths = string.Join(" ", directories.Select(d => d.FullName));
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r {paths}";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        if (SDeleteProcess.StartInfo.RedirectStandardOutput)
        {
            var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

            if (output.Contains("Access denied"))
                throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
            if (output.Contains("No files/folders found that match"))
                throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
        }
    }

    public void Dispose() => SDeleteProcess.Dispose();
}
