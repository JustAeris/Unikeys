using System.Diagnostics;

namespace Unikeys.Core.FileShredding;

/// <summary>
/// This class uses the SysInternal tool SDelete to securely overwrite a file on the disk, making it impossible to recover in any case.
/// <see href="https://docs.microsoft.com/en-gb/sysinternals/downloads/sdelete">Source here</see>
/// </summary>
public static class SDelete
{
    private static readonly Process SDeleteProcess = new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = Environment.Is64BitOperatingSystem ? "Tools/sdelete64.exe" : "Tools/sdelete.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        }
    };

    /// <summary>
    /// Securely deletes a file using SDelete
    /// </summary>
    /// <param name="file">File to overwrite</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public static async Task DeleteFile(FileInfo file, int passes = 1)
    {
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r \"{file.FullName}\"";
        await SDeleteProcess.WaitForExitAsync();

        var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

        if (output.Contains("Access denied"))
            throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
        if (output.Contains("No files/folders found that match"))
            throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
    }

    /// <summary>
    /// Securely deletes multiple files using SDelete
    /// </summary>
    /// <param name="files">Files to overwrite</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public static async Task DeleteFiles(IEnumerable<FileInfo> files, int passes = 1)
    {
        var paths = string.Join(" ", files.Select(f => $"\"{f.FullName}\""));
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r {paths}";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

        if (output.Contains("Access is denied"))
            throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
        if (output.Contains("No files/folders found that match"))
            throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
    }

    /// <summary>
    /// Securely deletes a directory using SDelete
    /// </summary>
    /// <param name="directory">Directory to delete</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public static async Task DeleteDirectory(DirectoryInfo directory, int passes = 1)
    {
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r \"{directory.FullName}\"";
        await SDeleteProcess.WaitForExitAsync();

        var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

        if (output.Contains("Access denied"))
            throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
        if (output.Contains("No files/folders found that match"))
            throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
    }

    /// <summary>
    /// Securely deletes multiple directories using SDelete
    /// </summary>
    /// <param name="directories">Directories to delete</param>
    /// <param name="passes">Number of passes (overwrites)</param>
    /// <exception cref="SDeleteAccessDeniedException">SDelete failed to access the file, maybe it requires elevated permissions.</exception>
    /// <exception cref="SDeleteNotFoundException">SDelete failed to find the file.</exception>
    public static async Task DeleteDirectories(IEnumerable<DirectoryInfo> directories, int passes = 1)
    {
        var paths = string.Join(" ", directories.Select(d => d.FullName));
        SDeleteProcess.StartInfo.Arguments = $"-p {passes} -r {paths}";
        SDeleteProcess.Start();
        await SDeleteProcess.WaitForExitAsync();

        var output = await SDeleteProcess.StandardOutput.ReadToEndAsync();

        if (output.Contains("Access denied"))
            throw new SDeleteAccessDeniedException("SDelete failed to delete the file, access has been denied.");
        if (output.Contains("No files/folders found that match"))
            throw new SDeleteNotFoundException("SDelete failed to delete the file, because it could not find it");
    }
}
