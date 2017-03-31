using System;
using Relativity.API;

namespace MarkupUtilities.Helpers.Rsapi.Interfaces
{
  public interface IErrorQueries
  {
    void WriteError(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, Exception ex);
  }
}
