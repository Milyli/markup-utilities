using System;
using System.Text;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public class AuditRecordHelper : IAuditRecordHelper
  {
    private readonly IQuery _query;

    public AuditRecordHelper(IQuery query)
    {
      _query = query;
    }

    public async Task CreateRedactionAuditRecordAsync(IDBContext workspaceDbContext, int auditActionId, int artifactId, int userId, ImportWorkerQueueRecord importWorkerQueueRecord, int markupSetArtifactId, int redactionId, string fileGuid)
    {
      if (auditActionId < 0)
      {
        throw new MarkupUtilityException($"{auditActionId} cannot be negative.");
      }

      if (artifactId < 0)
      {
        throw new MarkupUtilityException($"{artifactId} cannot be negative.");
      }

      if (userId < 0)
      {
        throw new MarkupUtilityException($"{userId} cannot be negative.");
      }

      if (importWorkerQueueRecord == null)
      {
        throw new ArgumentNullException(nameof(importWorkerQueueRecord));
      }

      if (markupSetArtifactId < 0)
      {
        throw new MarkupUtilityException($"{markupSetArtifactId} cannot be negative.");
      }

      if (redactionId < 0)
      {
        throw new MarkupUtilityException($"{redactionId} cannot be negative.");
      }

      if (fileGuid == null)
      {
        throw new ArgumentNullException(nameof(fileGuid));
      }

      string errorContext = $"{Constant.ErrorMessages.CREATE_REDACTION_AUDIT_RECORD_ERROR} [AuditActionId = {auditActionId}, ArtifactId = {artifactId}, UserId = {userId}, MarkupSetArtifactId = {markupSetArtifactId}, RedactionId = {redactionId}, FileGuid = {fileGuid}, RedactionData = {importWorkerQueueRecord.ToStringRedactionData()}]";

      try
      {
        var requestOrigination = await ConstructRequestOrigination();
        var recordOrigination = await ConstructRecordOrigination();
        var valueType = auditActionId == Constant.AuditRecord.AuditAction.REDACTION_CREATED ? Constant.AuditRecord.ValueType.NEW_VALUE : Constant.AuditRecord.ValueType.OLD_VALUE;
        var details = await ConstructDetailsColumn(importWorkerQueueRecord, markupSetArtifactId, redactionId, fileGuid, valueType);

        var redactionAuditRecord = new RedactionAuditRecord(
          artifactId,
          auditActionId,
          details,
          userId,
          DateTime.UtcNow,
          requestOrigination,
          recordOrigination,
          null,
          null
          );

        await _query.CreateAuditRecordAsync(workspaceDbContext, redactionAuditRecord);
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    public async Task CreateRedactionAuditRecordAsync(IDBContext workspaceDbContext, int auditActionId, int artifactId, int userId, string fileGuid, int redactionId, int markupSetArtifactId, int pageNumber)
    {
      string errorContext = $"{Constant.ErrorMessages.CREATE_REDACTION_AUDIT_RECORD_ERROR} [AuditActionId = {auditActionId}, ArtifactId = {artifactId}, UserId = {userId}, MarkupSetArtifactId = {markupSetArtifactId}, RedactionId = {redactionId}, FileGuid = {fileGuid}]";

      try
      {
        var requestOrigination = await ConstructRequestOrigination();
        var recordOrigination = await ConstructRecordOrigination();
        var details = ConstructDetailsColumn(redactionId, markupSetArtifactId, pageNumber);
        var redactionAuditRecord = new RedactionAuditRecord(
          artifactId,
          auditActionId,
          details,
          userId,
          DateTime.UtcNow,
          requestOrigination,
          recordOrigination,
          null,
          null
          );

        await _query.CreateAuditRecordAsync(workspaceDbContext, redactionAuditRecord);
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(errorContext, ex);
      }
    }

    private static async Task<string> ConstructRecordOrigination()
    {
      var retVal = await Task.Run(() =>
      {
        var sb = new StringBuilder();
        sb.Append($@"<auditElement>");
        sb.Append($@"<RecordOrigination>");
        sb.Append($@"<MAC />");
        sb.Append($@"<IP />");
        sb.Append($@"<Server />");
        sb.Append($@"</RecordOrigination>");
        sb.Append($@"</auditElement>");

        return sb.ToString();
      });

      return retVal;
    }

    private static async Task<string> ConstructRequestOrigination()
    {
      var retVal = await Task.Run(() =>
      {
        var sb = new StringBuilder();
        sb.Append($@"<auditElement>");
        sb.Append($@"<RequestOrigination>");
        sb.Append($@"<IP />");
        sb.Append($@"<Prefix />");
        sb.Append($@"<Page>{Constant.AuditRecord.APPLICATION_NAME}</Page>");
        sb.Append($@"</RequestOrigination>");
        sb.Append($@"</auditElement>");

        return sb.ToString();
      });

      return retVal;
    }

    private static async Task<string> ConstructDetailsColumn(ImportWorkerQueueRecord importWorkerQueueRecord, int markupSetArtifactId, int redactionId, string fileGuid, string valueType)
    {
      var retVal = await Task.Run(() =>
      {
        var sb = new StringBuilder();
        sb.Append($@"<auditElement>");
        sb.Append($@"<imageMarkup id=""{redactionId}"" pageNumber=""{importWorkerQueueRecord.FileOrder + 1}"" markupSetArtifactID=""{markupSetArtifactId}"" />");
        sb.Append($@"<field name=""ID""><{valueType}>{redactionId}</{valueType}></field>");
        sb.Append($@"<field name=""FileGuid""><{valueType}>{fileGuid}</{valueType}></field>");
        sb.Append($@"<field name=""X""><{valueType}>{importWorkerQueueRecord.X}</{valueType}></field>");
        sb.Append($@"<field name=""Y""><{valueType}>{importWorkerQueueRecord.Y}</{valueType}></field>");
        sb.Append($@"<field name=""Width""><{valueType}>{importWorkerQueueRecord.Width}</{valueType}></field>");
        sb.Append($@"<field name=""Height""><{valueType}>{importWorkerQueueRecord.Height}</{valueType}></field>");
        sb.Append($@"<field name=""MarkupSetArtifactID""><{valueType}>{markupSetArtifactId}</{valueType}></field>");
        sb.Append($@"<field name=""FillA""><{valueType}>{importWorkerQueueRecord.FillA}</{valueType}></field>");
        sb.Append($@"<field name=""FillR""><{valueType}>{importWorkerQueueRecord.FillR}</{valueType}></field>");
        sb.Append($@"<field name=""FillG""><{valueType}>{importWorkerQueueRecord.FillG}</{valueType}></field>");
        sb.Append($@"<field name=""FillB""><{valueType}>{importWorkerQueueRecord.FillB}</{valueType}></field>");
        sb.Append($@"<field name=""BorderSize""><{valueType}>{importWorkerQueueRecord.BorderSize}</{valueType}></field>");
        sb.Append($@"<field name=""BorderA""><{valueType}>{importWorkerQueueRecord.BorderA}</{valueType}></field>");
        sb.Append($@"<field name=""BorderR""><{valueType}>{importWorkerQueueRecord.BorderR}</{valueType}></field>");
        sb.Append($@"<field name=""BorderG""><{valueType}>{importWorkerQueueRecord.BorderG}</{valueType}></field>");
        sb.Append($@"<field name=""BorderB""><{valueType}>{importWorkerQueueRecord.BorderB}</{valueType}></field>");
        sb.Append($@"<field name=""BorderStyle""><{valueType}>{importWorkerQueueRecord.BorderStyle}</{valueType}></field>");
        sb.Append($@"<field name=""FontName""><{valueType}>{importWorkerQueueRecord.FontName}</{valueType}></field>");
        sb.Append($@"<field name=""FontA""><{valueType}>{importWorkerQueueRecord.FontA}</{valueType}></field>");
        sb.Append($@"<field name=""FontR""><{valueType}>{importWorkerQueueRecord.FontR}</{valueType}></field>");
        sb.Append($@"<field name=""FontG""><{valueType}>{importWorkerQueueRecord.FontG}</{valueType}></field>");
        sb.Append($@"<field name=""FontB""><{valueType}>{importWorkerQueueRecord.FontB}</{valueType}></field>");
        sb.Append($@"<field name=""FontSize""><{valueType}>{importWorkerQueueRecord.FontSize}</{valueType}></field>");
        sb.Append($@"<field name=""FontStyle""><{valueType}>{importWorkerQueueRecord.FontStyle}</{valueType}></field>");
        sb.Append($@"<field name=""Text""><{valueType}>{importWorkerQueueRecord.Text}</{valueType}></field>");
        sb.Append($@"<field name=""ZOrder""><{valueType}>{importWorkerQueueRecord.ZOrder}</{valueType}></field>");
        sb.Append($@"<field name=""DrawCrossLines""><{valueType}>{importWorkerQueueRecord.DrawCrossLines}</{valueType}></field>");
        sb.Append($@"<field name=""MarkupSubType""><{valueType}>{importWorkerQueueRecord.MarkupSubType}</{valueType}></field>");
        sb.Append($@"<field name=""MarkupType""><{valueType}>{importWorkerQueueRecord.MarkupType}</{valueType}></field>");
        sb.Append($@"<field name=""X_d""><{valueType}>{importWorkerQueueRecord.Xd}</{valueType}></field>");
        sb.Append($@"<field name=""Y_d""><{valueType}>{importWorkerQueueRecord.Yd}</{valueType}></field>");
        sb.Append($@"<field name=""Width_d""><{valueType}>{importWorkerQueueRecord.WidthD}</{valueType}></field>");
        sb.Append($@"<field name=""Height_d""><{valueType}>{importWorkerQueueRecord.HeightD}</{valueType}></field>");
        sb.Append($@"</auditElement>");

        return sb.ToString();
      });

      return retVal;
    }

    private static string ConstructDetailsColumn(int redactionId, int markupSetArtifactId, int pageNumber)
    {
      return $@"<auditElement><imageMarkup id=""{redactionId}"" pageNumber=""{pageNumber}"" markupSetArtifactID=""{markupSetArtifactId}"" /></auditElement>";
    }
  }
}