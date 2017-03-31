using System.Threading.Tasks;
using Relativity.API;

namespace MarkupUtilities.Helpers.Rsapi.Interfaces
{
  public interface IWorkspaceQueries
  {
    Task<int> GetResourcePoolAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId);
  }
}
