namespace Microsoft.Xna.Framework.Net;

/// <summary>The base class for networking failures.</summary>
[Serializable]
public class NetworkException : Exception
{
    public NetworkException()
        : base("A network error occurred.")
    {
    }

    public NetworkException(string message)
        : base(message)
    {
    }

    public NetworkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Raised when no network connection is available.</summary>
[Serializable]
public class NetworkNotAvailableException : NetworkException
{
    public NetworkNotAvailableException()
        : base("The network is not available.")
    {
    }

    public NetworkNotAvailableException(string message)
        : base(message)
    {
    }

    public NetworkNotAvailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Raised when joining a session fails.</summary>
[Serializable]
public class NetworkSessionJoinException : NetworkException
{
    public NetworkSessionJoinException()
    {
    }

    public NetworkSessionJoinException(string message)
        : base(message)
    {
    }

    public NetworkSessionJoinException(string message, NetworkSessionJoinError joinError)
        : base(message)
    {
        JoinError = joinError;
    }

    public NetworkSessionJoinException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NetworkSessionJoinError JoinError { get; set; }
}
