using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers
{
  public class ExportFileCreator : IExportFileCreator
  {
    public string ExportFileName { get; private set; }
    public string ExportFullFilePath { get; private set; }
    private FileInfo _exportFileInfo;

    public ExportFileCreator()
    {
      ExportFileName = null;
      ExportFullFilePath = null;
    }

    public async Task<string> CreateExportFileAsync(string exportJobName)
    {
      try
      {
        //generate temp file name
        ExportFullFilePath = await GenerateExportFileFullNameAsync(exportJobName);

        //create export file
        _exportFileInfo = await CreateExportFileAsync();

        //return export full file path
        return ExportFullFilePath;
      }
      catch (Exception ex)
      {
        throw new Exception("An error occured when creating Export file.", ex);
      }
    }

    public async Task WriteToExportFileAsync(List<ExportResultsRecord> exportResultsRecords)
    {
      if (exportResultsRecords.Count > 0)
      {
        foreach (var exportResultsRecord in exportResultsRecords)
        {
          await WriteToExportFileAsync(exportResultsRecord);
        }
      }
    }

    public async Task DeleteExportFileAsync()
    {
      try
      {
        //Delete export file
        await Task.Run(() =>
        {
          _exportFileInfo.Refresh();
          if (_exportFileInfo.Exists)
          {
            _exportFileInfo.Delete();
          }
          else
          {
            throw new Exception("Export file does not exist.");
          }
        });
      }
      catch (Exception ex)
      {
        throw new Exception("An error occured when deleting Export file.", ex);
      }
    }

    #region private methods

    private async Task WriteToExportFileAsync(ExportResultsRecord exportResultsRecord)
    {
      try
      {
        await Task.Run(() =>
        {
          using (var streamWriter = _exportFileInfo.AppendText())
          {
            var newLine = exportResultsRecord.ToString();
            streamWriter.WriteLine(newLine);
          }
        });
      }
      catch (Exception ex)
      {
        throw new Exception("An error occured when writing to the Export file.", ex);
      }
    }

    private async Task<string> GenerateExportFileFullNameAsync(string exportJobName)
    {
      var retVal = await Task.Run(async () =>
      {
        var tempPath = Path.GetTempPath();
        ExportFileName = await GenerateExportFileNameAsync(exportJobName);
        string exportFileNameWithExtension = $@"{ExportFileName}{Constant.ExportFile.EXPORT_FILE_EXTENSION}";
        string exportFileFullName = $@"{tempPath}{exportFileNameWithExtension}";

        return exportFileFullName;
      });

      return retVal;
    }

    private static async Task<string> GenerateExportFileNameAsync(string exportJobName)
    {
      var retVal = await Task.Run(() =>
      {
        var currentDateTime = DateTime.UtcNow.ToString("yyyy_MM_dd_hh_mm_ss_fff");
        string exportFileName = $@"{exportJobName}_ExportFile_{currentDateTime}";
        return exportFileName;
      });

      return retVal;
    }

    private static async Task<string> ConstructExportFileHeaderRowAsync()
    {
      var retVal = await Task.Run(() =>
      {
        var headerRow = string.Join(",", Constant.ExportFile.ColumnsList);
        return headerRow;
      });

      return retVal;
    }

    private async Task<FileInfo> CreateExportFileAsync()
    {
      try
      {
        //create export file info object
        _exportFileInfo = new FileInfo(ExportFullFilePath);

        if (_exportFileInfo.Exists)
        {
          throw new Exception("File already exists");
        }

        //create csv file
        using (var streamWriter = _exportFileInfo.AppendText())
        {
          var header = await ConstructExportFileHeaderRowAsync();
          streamWriter.WriteLine(header);
        }

        //delete cache for fileinfo
        _exportFileInfo.Refresh();

        return _exportFileInfo;
      }
      catch (Exception ex)
      {
        throw new Exception("An error occured when creating export file.", ex);
      }
    }

    #endregion
  }
}
