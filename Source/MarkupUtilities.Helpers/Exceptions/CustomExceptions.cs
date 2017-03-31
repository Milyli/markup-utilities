namespace MarkupUtilities.Helpers.Exceptions
{
  [System.Serializable]
  public class MarkupUtilityException : System.Exception
  {
    public MarkupUtilityException() { }

    public MarkupUtilityException(string message) : base(message) { }

    public MarkupUtilityException(string message, System.Exception inner) : base(message, inner) { }

    // A constructor is needed for serialization when an
    // exception propagates from a remoting server to the client. 
    protected MarkupUtilityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
  }
}
