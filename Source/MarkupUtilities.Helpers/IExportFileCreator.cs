using System.Collections.Generic;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers
{
  public interface IExportFileCreator
  {
    string ExportFileName { get; }
    string ExportFullFilePath { get; }

    Task<string> CreateExportFileAsync(string exportJobName);
    Task WriteToExportFileAsync(List<ExportResultsRecord> exportResultsRecords);
    Task DeleteExportFileAsync();
  }
}
