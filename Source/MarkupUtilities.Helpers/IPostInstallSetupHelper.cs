using System.Threading.Tasks;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public interface IPostInstallSetupHelper
  {
    Task CreateRecordsForMarkupUtilityTypeRdoAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, IDBContext workspaceDbContext);
  }
}