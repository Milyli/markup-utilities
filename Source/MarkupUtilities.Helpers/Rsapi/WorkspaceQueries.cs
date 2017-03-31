using System.Threading.Tasks;
using kCura.Relativity.Client;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Helpers.Rsapi
{
  public class WorkspaceQueries : IWorkspaceQueries
  {
    public async Task<int> GetResourcePoolAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId)
    {
      return await Task.Run(() =>
      {
        var resourcePoolId = 0;
        using (var proxy = svcMgr.CreateProxy<IRSAPIClient>(identity))
        {
          var result = proxy.Repositories.Workspace.ReadSingle(workspaceArtifactId).ResourcePoolID;

          if (result.HasValue)
          {
            resourcePoolId = result.Value;
          }

          return resourcePoolId;
        }
      });
    }
  }
}
