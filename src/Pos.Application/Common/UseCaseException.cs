namespace Pos.Application.Common;

public sealed class UseCaseException(string message) : Exception(message);
