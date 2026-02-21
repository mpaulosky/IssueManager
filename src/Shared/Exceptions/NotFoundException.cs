namespace Shared.Exceptions;

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class NotFoundException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="NotFoundException"/> class.
	/// </summary>
	public NotFoundException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NotFoundException"/> class.
	/// </summary>
	public NotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
