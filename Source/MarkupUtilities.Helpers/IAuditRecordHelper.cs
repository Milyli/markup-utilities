using System.Threading.Tasks;
using MarkupUtilities.Helpers.Models;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public interface IAuditRecordHelper
  {
    Task CreateRedactionAuditRecordAsync(IDBContext workspaceDbContext, int auditActionId, int artifactId, int userId, ImportWorkerQueueRecord importWorkerQueueRecord, int markupSetArtifactId, int redactionId, string fileGuid);
    Task CreateRedactionAuditRecordAsync(IDBContext workspaceDbContext, int auditActionId, int artifactId, int userId, string fileGuid, int redactionId, int markupSetArtifactId, int pageNumber);
  }
}