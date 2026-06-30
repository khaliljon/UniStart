namespace UniStart.Application.Exceptions;

/// <summary>
/// Thrown when an email that the user is actively waiting for (e.g. a password
/// reset code) could not be delivered because of an SMTP/configuration failure.
/// Lets the controller surface a real error instead of falsely telling the user
/// the message was sent.
/// </summary>
public class EmailDeliveryException : Exception
{
    public EmailDeliveryException(string message) : base(message) { }

    public EmailDeliveryException(string message, Exception inner) : base(message, inner) { }
}
