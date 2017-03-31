using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using MarkupUtilities.Helpers.Models;
using NUnit.Framework;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class ExportFileCreatorTests
  {
    private IExportFileCreator _exportFileCreator;
    private const string ExportFileNameBase = "ExportFileCreatorTestFile_";
    private string _exportFullFilePath;

    [SetUp]
    public void Setup()
    {
      _exportFileCreator = new ExportFileCreator();
      var exportJobName = GetRandomExportJobName();

      //create temp export file
      _exportFullFilePath = _exportFileCreator.CreateExportFileAsync(exportJobName).Result;
    }

    [TearDown]
    public void Teardown()
    {
      //delete temp export file
      _exportFileCreator.DeleteExportFileAsync().Wait();
      _exportFileCreator = null;
    }

    #region Tests

    [Description("When a list of ExportResultsRecords are passed, it should successfully write it to the export file")]
    [Test]
    public void WriteToExportFileAsyncTest()
    {
      //Arrange
      var exportResultsRecords = GetExportResultsRecords();

      //Act
      _exportFileCreator.WriteToExportFileAsync(exportResultsRecords).Wait();

      //Assert
      var actualFileContents = ReadFileToEnd();
      var expectedFileContents = ConstructExpectedFileContents(exportResultsRecords);
      Assert.That(actualFileContents, Is.EqualTo(expectedFileContents));
    }

    #endregion

    #region Test Helpers

    private static string ConstructExpectedFileContents(IEnumerable<ExportResultsRecord> exportResultsRecords)
    {
      var fileHeaderRow = string.Join(",", Constant.ExportFile.ColumnsList);
      string expectedFileContents = $"{fileHeaderRow}";

      return exportResultsRecords.Select(currentExportResultsRecord => currentExportResultsRecord.ToString()).Aggregate(expectedFileContents, (current, exportResult) => current + $"{exportResult}");
    }

    private string ReadFileToEnd()
    {
      var sb = new StringBuilder();
      var exportFileInfo = new FileInfo(_exportFullFilePath);

      using (var streamReader = exportFileInfo.OpenText())
      {
        string line;
        while ((line = streamReader.ReadLine()) != null)
        {
          sb.Append(line);
        }
      }

      var fileContents = sb.ToString();
      return fileContents;
    }

    private List<ExportResultsRecord> GetExportResultsRecords()
    {
      var exportResultsRecords = new List<ExportResultsRecord>();
      var dataTable = GetExportResultsDataTableWithOneRecord();
      var dataRow = dataTable.Rows[0];
      var newExportResultsRecord = new ExportResultsRecord(dataRow);
      exportResultsRecords.Add(newExportResultsRecord);

      return exportResultsRecords;
    }

    private static DataTable GetExportResultsDataTableWithOneRecord()
    {
      var dataTable = new DataTable("Test Import Worker Table");

      dataTable.Columns.Add(new DataColumn("ID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("DocumentIdentifier", typeof(String)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("ExportJobArtifactId", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("FileOrder", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("X", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Y", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Width", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("Height", typeof(Int32)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("MarkupType", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FillB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderSize", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("BorderStyle", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontName", typeof(String)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontA", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontR", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontG", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontB", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontSize", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("FontStyle", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Text", typeof(String)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("ZOrder", typeof(Int32)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("DrawCrossLines", typeof(Boolean)) { AllowDBNull = false });
      dataTable.Columns.Add(new DataColumn("MarkupSubType", typeof(Int16)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("X_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Y_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Width_d", typeof(decimal)) { AllowDBNull = true });
      dataTable.Columns.Add(new DataColumn("Height_d", typeof(decimal)) { AllowDBNull = true });

      dataTable.Rows.Add(
        123, //ID
        DateTime.UtcNow, //TimeStampUTC
        123, //WorkspaceArtifactID
        "documentIdentifier", //DocumentIdentifier
        123, //ExportJobArtifactId
        123, //FileOrder
        123, // X
        123, // Y
        123, //Width
        123, //Height
        1, //MarkupType
        123, //FillA
        123, //FillR
        123, //FillG
        123, //FillB
        123, //BorderSize
        123, //BorderA
        123, //BorderR
        123, //BorderG
        123, //BorderB
        123, //BorderStyle
        "fontName", //FontName
        123, //FontA
        123, //FontR
        123, //FontG
        123, //FontB
        123, //FontSize
        123, //FontStyle
        "text", //Text
        123, //ZOrder
        false, //DrawCrossLines
        123, //MarkupSubType
        123.1, // X_d
        123.1, // Y_d
        123.1, //Width_d
        123.1 //Height_d
      );

      return dataTable;
    }

    private static string GetRandomExportJobName()
    {
      var random = new Random();
      var newRandomNumber = random.Next(1, 9999);
      string retVal = $"{ExportFileNameBase}{newRandomNumber}_";
      return retVal;
    }

    #endregion
  }
}
