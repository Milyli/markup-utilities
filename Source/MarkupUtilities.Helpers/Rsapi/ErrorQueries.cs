using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Helpers.Rsapi
{
  public class ErrorQueries : IErrorQueries
  {
    #region Public Methods

    //Do not convert to async
    public void WriteError(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Exception ex)
    {
      using (var client = svcMgr.CreateProxy<IRSAPIClient>(identity))
      {
        client.APIOptions.WorkspaceID = workspaceArtifactId;

        var res = WriteError(client, workspaceArtifactId, ex);
        if (!res.Success)
        {
          throw new MarkupUtilityException(res.Message);
        }
      }
    }
    #endregion

    #region Private Methods

    //Do not convert to async
    private static Response<IEnumerable<Error>> WriteError(IRSAPIClient proxy, int workspaceArtifactId, Exception ex)
    {
      var artifact = new Error
      {
        FullError = ex.StackTrace,
        Message = ex.Message,
        SendNotification = false,
        Server = Environment.MachineName,
        Source = $"{Constant.Names.ApplicationName} [Guid={Constant.Guids.Application.ApplicationGuid}]",
        Workspace = new Workspace(workspaceArtifactId)
      };
      var theseResults = proxy.Repositories.Error.Create(artifact);
      return Response<Error>.CompileWriteResults(theseResults);
    }

    #endregion Private Methods
  }
}
