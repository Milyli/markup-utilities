using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;
using NUnit.Framework;

namespace MarkupUtilities.Helpers.NUnit
{
  [TestFixture]
  public class ImportFileParserTests
  {
    private readonly string _goodImportFile = "TestImportFile-GoodData.csv";
    private readonly string _badImportFile = "TestImportFile-BadData.csv";
    private IImportFileParser _importFileParser;
    private int _lineCount;
    private int _totalLineCount;

    [SetUp]
    public void Setup()
    {
      _importFileParser = new ImportFileParser();
      _lineCount = 0;
      _totalLineCount = 0;
    }

    [TearDown]
    public void Teardown()
    {
      _importFileParser = null;
      _lineCount = 0;
      _totalLineCount = 0;
    }

    [Test]
    public void ValidateFileContentsTest()
    {
      //Arrange
      var fileStreamReader = new StreamReader(GetFilePath(_goodImportFile));

      //Act

      //Assert
      Assert.DoesNotThrow(() => { _importFileParser.ValidateFileContentsAsync(fileStreamReader).Wait(); });
    }

    [Test]
    public void ValidateFileContentsTest_BadImportFileData()
    {
      //Arrange
      var fileStreamReader = new StreamReader(GetFilePath(_badImportFile));

      //Act
      var validateFileContentsAsyncTask = _importFileParser.ValidateFileContentsAsync(fileStreamReader);
      var aggregateException = Assert.Throws<AggregateException>(() =>
      {
        validateFileContentsAsyncTask.Wait();
      });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(MarkupUtilityException)) as MarkupUtilityException;
      Assert.That(actualException, Is.Not.Null);
      if (actualException == null) return;
      Assert.That(actualException.Message, Is.EqualTo(Constant.ErrorMessages.VALIDATE_FILE_CONTENTS_ERROR));
      Assert.That(actualException.InnerException.Message, Is.EqualTo(Constant.ErrorMessages.COLUMN_COUNT_MISMATCH));
    }

    [Test]
    public void ValidateFileContentsTest_WrongInput()
    {
      //Arrange

      //Act
      var validateFileContentsAsyncTask = _importFileParser.ValidateFileContentsAsync(null);
      var aggregateException = Assert.Throws<AggregateException>(() => { validateFileContentsAsyncTask.Wait(); });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(ArgumentNullException)) as ArgumentNullException;
      Assert.That(actualException, Is.Not.Null);
      if (actualException != null)
      {
        StringAssert.Contains("Value cannot be null.", actualException.Message);
      }
    }

    [Test]
    public void ParseFileContentsTest()
    {
      //Arrange
      var lines = File.ReadLines(GetFilePath(_goodImportFile)).ToList();
      var fileStreamReader = new StreamReader(GetFilePath(_goodImportFile));

      //Act
      _importFileParser.ParseFileContentsAsync(fileStreamReader, ProcessEachLineAsync, AfterProcessingAllLinesAsync).Wait();

      //Assert
      var dataRowsCount = lines.Count - 1;
      Assert.That(_lineCount, Is.EqualTo(dataRowsCount));
      Assert.That(_totalLineCount, Is.EqualTo(1));
    }

    [Test]
    public void ParseFileContentsTest_BadImportFileData()
    {
      //Arrange
      var fileStreamReader = new StreamReader(GetFilePath(_badImportFile));

      //Act
      var parseFileContentsAsyncAsyncTask = _importFileParser.ParseFileContentsAsync(fileStreamReader, ProcessEachLineAsync, AfterProcessingAllLinesAsync);
      var aggregateException = Assert.Throws<AggregateException>(() =>
      {
        parseFileContentsAsyncAsyncTask.Wait();
      });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(MarkupUtilityException)) as MarkupUtilityException;
      Assert.That(actualException, Is.Not.Null);
      if (actualException != null)
      {
        Assert.That(actualException.Message, Is.EqualTo(Constant.ErrorMessages.PARSE_FILE_CONTENTS_ERROR));
        Assert.That(actualException.InnerException.Message, Is.EqualTo(Constant.ErrorMessages.COLUMN_COUNT_MISMATCH));
      }

      Assert.That(_lineCount, Is.EqualTo(0));
      Assert.That(_totalLineCount, Is.EqualTo(0));
    }

    [Test]
    public void ParseFileContentsTest_WrongInput1()
    {
      //Arrange
      var fileStreamReader = new StreamReader(GetFilePath(_badImportFile));

      //Act
      var parseFileContentsAsyncAsyncTask = _importFileParser.ParseFileContentsAsync(fileStreamReader, null, AfterProcessingAllLinesAsync);
      var aggregateException = Assert.Throws<AggregateException>(() =>
      {
        parseFileContentsAsyncAsyncTask.Wait();
      });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(ArgumentNullException)) as ArgumentNullException;
      Assert.That(actualException, Is.Not.Null);
      if (actualException != null)
      {
        StringAssert.Contains("Value cannot be null.", actualException.Message);
      }

      Assert.That(_lineCount, Is.EqualTo(0));
      Assert.That(_totalLineCount, Is.EqualTo(0));
    }

    [Test]
    public void ParseFileContentsTest_WrongInput2()
    {
      //Arrange

      //Act
      var parseFileContentsAsyncAsyncTask = _importFileParser.ParseFileContentsAsync(null, ProcessEachLineAsync, null);
      var aggregateException = Assert.Throws<AggregateException>(() =>
      {
        parseFileContentsAsyncAsyncTask.Wait();
      });

      //Assert
      var actualException = aggregateException.InnerExceptions.First(x => x.GetType() == typeof(ArgumentNullException)) as ArgumentNullException;
      Assert.That(actualException, Is.Not.Null);
      if (actualException != null)
      {
        StringAssert.Contains("Value cannot be null.", actualException.Message);
      }

      Assert.That(_lineCount, Is.EqualTo(0));
      Assert.That(_totalLineCount, Is.EqualTo(0));
    }

    private async Task<bool> ProcessEachLineAsync(ImportFileRecord importFileRecord)
    {
      return await Task.Run(() =>
      {
        if (importFileRecord == null) return false;
        _lineCount++;
        return true;
      });
    }

    private async Task<bool> AfterProcessingAllLinesAsync()
    {
      return await Task.Run(() =>
      {
        _totalLineCount++;
        return true;
      });
    }

    private static string GetFilePath(string fileName)
    {
      var directoryName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
      {
        var rootDirectory = Path.Combine(directoryName, "TestData");
        return Path.Combine(rootDirectory, fileName);
      }
    }
  }
}
