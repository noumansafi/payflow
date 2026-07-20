namespace PayFlow.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }

    public abstract string Title { get; }
    public abstract int StatusCode { get; }
}

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
    public override string Title => "Validation Failed";
    public override int StatusCode => 400;
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message)
    {
    }

    public override string Title => "Conflict";
    public override int StatusCode => 409;
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Invalid credentials.") : base(message)
    {
    }

    public override string Title => "Unauthorized";
    public override int StatusCode => 401;
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public override string Title => "Not Found";
    public override int StatusCode => 404;
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message)
    {
    }

    public override string Title => "Forbidden";
    public override int StatusCode => 403;
}
