using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace MarkupUtilities.Helpers.Rsapi
{
  public class Response<TResultType>
  {
    public string Message { get; set; }
    public bool Success { get; set; }
    public TResultType Results { get; set; }

    //Do not convert to async
    public static Response<IEnumerable<RDO>> CompileQuerySubsets(IRSAPIClient proxy, QueryResultSet<RDO> theseResults)
    {
      var success = true;
      var message = "";
      var resultList = new List<Result<RDO>>();
      var iterator = 0;

      message += theseResults.Message;

      if (!theseResults.Success)
      {
        success = false;
      }

      resultList.AddRange(theseResults.Results);

      if (!string.IsNullOrEmpty(theseResults.QueryToken))
      {
        var queryToken = theseResults.QueryToken;
        var batchSize = theseResults.Results.Count();
        iterator += batchSize;
        do
        {
          theseResults = proxy.Repositories.RDO.QuerySubset(queryToken, iterator + 1, batchSize);
          resultList.AddRange(theseResults.Results);
          message += theseResults.Message;
          if (!theseResults.Success)
          {
            success = false;
          }
          iterator += batchSize;
        } while (iterator < theseResults.TotalCount);
      }

      var res = new Response<IEnumerable<RDO>>
      {
        Results = resultList.Select(x => x.Artifact),
        Success = success,
        Message = MessageFormatter.FormatMessage(resultList.Select(x => x.Message).ToList(), message, success)
      };

      return res;
    }

    //Do not convert to async
    public static Response<IEnumerable<RDO>> CompileWriteResults(WriteResultSet<RDO> theseResults)
    {
      var success = true;
      var message = "";

      message += theseResults.Message;

      if (!theseResults.Success)
      {
        success = false;
      }

      var res = new Response<IEnumerable<RDO>>
      {
        Results = theseResults.Results.Select(x => x.Artifact),
        Success = success,
        Message = MessageFormatter.FormatMessage(theseResults.Results.Select(x => x.Message).ToList(), message, success)
      };

      return res;
    }

    //Do not convert to async
    public static Response<IEnumerable<Error>> CompileWriteResults(WriteResultSet<Error> theseResults)
    {
      var success = true;
      var message = "";

      message += theseResults.Message;

      if (!theseResults.Success)
      {
        success = false;
      }

      var res = new Response<IEnumerable<Error>>
      {
        Results = theseResults.Results.Select(x => x.Artifact),
        Success = success,
        Message = MessageFormatter.FormatMessage(theseResults.Results.Select(x => x.Message).ToList(), message, success)
      };

      return res;
    }
  }
}
