using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkupUtilities.Helpers.Rsapi
{
  public class MessageFormatter
  {
    //Do not convert to async
    public static string FormatMessage(List<string> results, string message, Boolean success)
    {
      var messageList = "";

      if (success) return messageList;
      messageList = message;
      results.ToList().ForEach(w => messageList += (w));

      return messageList;
    }
  }
}
