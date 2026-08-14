namespace UniStart.Application.Exceptions;

public class LlmPaymentRequiredException : Exception
{
    public LlmPaymentRequiredException(string message) : base(message) { }
}
