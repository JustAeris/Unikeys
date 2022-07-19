namespace Unikeys.Core.FileShredding;



/// <summary>
/// Base SDelete exception
/// </summary>
public abstract class SDeleteException : Exception
{
    protected SDeleteException(string message) : base(message)
    {
    }
}

/// <summary>
/// Occurs when the file/folder is delete before the SDelete process has finished
/// </summary>
public class SDeleteNotFoundException : SDeleteException
{
    public SDeleteNotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// Occurs when the SDelete process has failed to access the file/folder, most of the time due to a lack of permissions
/// </summary>
public class SDeleteAccessDeniedException : SDeleteException
{
    public SDeleteAccessDeniedException(string message) : base(message)
    {
    }
}
