namespace UniStart.Application.Exceptions;

/// <summary>
/// Thrown when the LLM provider rejects a request because the account balance is
/// exhausted (HTTP 402 / "Insufficient Balance"). This is a fatal condition for a
/// batch sync: every subsequent file would fail the same way, so the sync should
/// stop immediately instead of marking the whole batch as Failed.
/// </summary>
public class LlmPaymentRequiredException : Exception
{
    public LlmPaymentRequiredException(string message) : base(message) { }
}
