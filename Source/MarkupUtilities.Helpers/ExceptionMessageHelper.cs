using System;
using System.Threading.Tasks;

namespace MarkupUtilities.Helpers
{
  public class ExceptionMessageHelper
  {
    public static async Task<string> GetInnerMostExceptionMessageAsync(Exception exception)
    {
      var retVal = string.Empty;

      await Task.Run(() =>
      {
        if (exception == null)
        {
          retVal = string.Empty;
        }
        else
        {
          var currentException = exception;
          while (currentException.InnerException != null)
          {
            currentException = currentException.InnerException;
          }

          retVal = currentException.Message;
        }
      });

      return retVal;
    }
  }
}
