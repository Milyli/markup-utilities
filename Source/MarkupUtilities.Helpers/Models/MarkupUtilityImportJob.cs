using System;
using System.Collections.Generic;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  public class MarkupUtilityImportJob
  {
    public int ArtifactId { get; set; }
    public string Name { get; set; }
    public int MarkupSetArtifactId { get; set; }
    public List<MarkupUtilityType> MarkupUtilityTypes { get; set; }
    public bool SkipDuplicateRedactions { get; set; }
    public int FileArtifactId { get; set; }
    public string Status { get; set; }
    public string Details { get; set; }
    public int TotalRedactionCount { get; set; }
    public int ImportedRedactionCount { get; set; }
    public string JobType { get; set; }
    public int CreatedBy { get; set; }

    public MarkupUtilityImportJob(int artifactId, string name, int markupSetArtifactId, List<MarkupUtilityType> markupUtilityTypes, bool? skipDuplicateRedactions, int fileArtifactId, string status, string details, int totalRedactionCount, int importedRedactionCount, string jobType, int createdBy)
    {
      if (artifactId < 0)
      {
        throw new MarkupUtilityException($"{nameof(ArtifactId)} cannot be negative.");
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

      if (skipDuplicateRedactions == null)
      {
        throw new ArgumentNullException(nameof(SkipDuplicateRedactions));
      }

      if (status == null)
      {
        throw new ArgumentNullException(nameof(Status));
      }

      if (totalRedactionCount < 0)
      {
        throw new MarkupUtilityException($"{nameof(TotalRedactionCount)} cannot be negative.");
      }

      if (importedRedactionCount < 0)
      {
        throw new MarkupUtilityException($"{nameof(ImportedRedactionCount)} cannot be negative.");
      }

      if (jobType == null)
      {
        throw new ArgumentNullException(nameof(JobType));
      }

      if (jobType != Constant.ImportJobType.IMPORT && jobType != Constant.ImportJobType.REVERT)
      {
        throw new MarkupUtilityException("Not a valid job type.");
      }

      ArtifactId = artifactId;
      Name = name;
      MarkupSetArtifactId = markupSetArtifactId;
      MarkupUtilityTypes = markupUtilityTypes;
      SkipDuplicateRedactions = skipDuplicateRedactions.Value;
      FileArtifactId = fileArtifactId;
      Status = status;
      Details = details ?? string.Empty;
      TotalRedactionCount = totalRedactionCount;
      ImportedRedactionCount = importedRedactionCount;
      JobType = jobType;
      CreatedBy = createdBy;
    }
  }
}
