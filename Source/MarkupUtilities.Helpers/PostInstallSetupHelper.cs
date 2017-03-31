using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using MarkupUtilities.Helpers.Rsapi.Interfaces;
using Relativity.API;

namespace MarkupUtilities.Helpers
{
  public class PostInstallSetupHelper : IPostInstallSetupHelper
  {
    private readonly IQuery _query;
    private readonly IArtifactQueries _artifactQueries;

    public PostInstallSetupHelper(IQuery query, IArtifactQueries artifactQueries)
    {
      if (query == null)
      {
        throw new ArgumentNullException(nameof(query));
      }

      if (artifactQueries == null)
      {
        throw new ArgumentNullException(nameof(artifactQueries));
      }

      _query = query;
      _artifactQueries = artifactQueries;
    }

    public async Task CreateRecordsForMarkupUtilityTypeRdoAsync(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, IDBContext workspaceDbContext)
    {
      try
      {
        var markupTypeDataTable = await _query.RetrieveMarkupTypesAsync(workspaceDbContext);
        var markupSubTypeDataTable = await _query.RetrieveMarkupSubTypesAsync(workspaceDbContext);

        if (markupTypeDataTable == null || markupSubTypeDataTable == null)
        {
          throw new MarkupUtilityException($"{Constant.Sql.WorkspaceTables.RedactionMarkupType.NAME} or {Constant.Sql.WorkspaceTables.RedactionMarkupSubType.NAME} tables does not have any records.");
        }

        var markupSubTypes = markupSubTypeDataTable
          .AsEnumerable()
          .Select(x => new MarkupSubType(x))
          .ToList();
        var reactionsList = markupSubTypes
          .Where(x => Constant.MarkupSubTypeCategory.RedactionsList.Contains(x.Id))
          .ToList();
        var highlightsList = markupSubTypes
          .Where(x => Constant.MarkupSubTypeCategory.HighlightsList.Contains(x.Id))
          .ToList();

        var markupUtilityTypes = new List<MarkupUtilityType>();

        markupUtilityTypes.AddRange(reactionsList.Select(x => new MarkupUtilityType(x.SubType, Constant.MarkupType.Redaction.NAME)));
        markupUtilityTypes.AddRange(highlightsList.Select(x => new MarkupUtilityType(x.SubType, Constant.MarkupType.Highlight.NAME)));

        var existingRedactionTypes = await _artifactQueries.QueryMarkupUtilityTypeRdoRecordAsync(svcMgr, identity, workspaceArtifactId, Constant.Guids.ObjectType.MarkupUtilityType);

        foreach (var markupUtilityType in markupUtilityTypes)
        {
          var categoryChoiceGuid = markupUtilityType.Category == Constant.MarkupType.Redaction.NAME ? Constant.Guids.Choices.MarkupUtilityType.Category.Redaction : Constant.Guids.Choices.MarkupUtilityType.Category.Highlight;

          if (!existingRedactionTypes.Contains(markupUtilityType))
          {
            //Create record for redaction or highlight if it does not already exist
            await _artifactQueries.CreateMarkupUtilityTypeRdoRecordAsync(svcMgr, identity, workspaceArtifactId, markupUtilityType.Name, categoryChoiceGuid);
          }
        }
      }
      catch (Exception)
      {
        throw new MarkupUtilityException($"An error occured when creating {Constant.Names.Rdos.MARKUP_UTILITY_TYPE} records.");
      }
    }
  }
}
