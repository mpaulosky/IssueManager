namespace Shared.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with the current state of a resource.
/// </summary>
public class ConflictException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ConflictException"/> class.
	/// </summary>
	public ConflictException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ConflictException"/> class.
	/// </summary>
	public ConflictException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
