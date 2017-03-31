using System;
using System.IO;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers
{
  public interface IImportFileParser
  {
    Task ParseFileContentsAsync(StreamReader fileStreamReader, Func<ImportFileRecord, Task<bool>> processEachLineAsyncFunc, Func<Task<bool>> afterProcessingAllLinesAsyncFunc);
    Task ValidateFileContentsAsync(StreamReader fileStreamReader);
  }
}