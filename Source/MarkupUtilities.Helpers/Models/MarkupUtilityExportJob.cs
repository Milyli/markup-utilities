using System;
using System.Collections.Generic;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  public class MarkupUtilityExportJob
  {
    public int ArtifactId { get; set; }
    public string Name { get; set; }
    public int MarkupSetArtifactId { get; set; }
    public List<MarkupUtilityType> MarkupUtilityTypes { get; set; }
    public int FileArtifactId { get; set; }
    public int SavedSearchArtifactId { get; set; }
    public string Status { get; set; }
    public string Details { get; set; }
    public int ExportedRedactionCount { get; set; }

    public MarkupUtilityExportJob(int artifactId, string name, int markupSetArtifactId, List<MarkupUtilityType> markupUtilityTypes, int savedSearchArtifactId, int fileArtifactId, string status, string details, int exportedRedactionCount)
    {
      if (artifactId <= 0)
      {
        throw new MarkupUtilityException($"{nameof(ArtifactId)} cannot be <= 0.");
      }

      if (name == null)
      {
        throw new ArgumentNullException(nameof(Name));
      }

      if (markupUtilityTypes == null)
      {
        throw new ArgumentNullException(nameof(MarkupUtilityTypes));
      }

      if (markupUtilityTypes.Count == 0)
      {
        throw new MarkupUtilityException($"{nameof(MarkupUtilityTypes)} should have at least one type specified.");
      }

      if (status == null)
      {
        throw new ArgumentNullException(nameof(Status));
      }

      if (savedSearchArtifactId <= 0)
      {
        throw new MarkupUtilityException($"{nameof(SavedSearchArtifactId)} cannot be <= 0.");
      }

      if (exportedRedactionCount < 0)
      {
        throw new MarkupUtilityException($"{nameof(ExportedRedactionCount)} cannot be negative.");
      }

      ArtifactId = artifactId;
      Name = name;
      MarkupSetArtifactId = markupSetArtifactId;
      MarkupUtilityTypes = markupUtilityTypes;
      FileArtifactId = fileArtifactId;
      SavedSearchArtifactId = savedSearchArtifactId;
      Status = status;
      Details = details ?? string.Empty;
      ExportedRedactionCount = exportedRedactionCount;
    }
  }
}
