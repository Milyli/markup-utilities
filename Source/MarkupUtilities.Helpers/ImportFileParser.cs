using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers
{
  public class ImportFileParser : IImportFileParser
  {
    public async Task ParseFileContentsAsync(StreamReader fileStreamReader, Func<ImportFileRecord, Task<bool>> processEachLineAsyncFunc, Func<Task<bool>> afterProcessingAllLinesAsyncFunc)
    {
      if (fileStreamReader == null)
      {
        throw new ArgumentNullException(nameof(fileStreamReader));
      }

      if (processEachLineAsyncFunc == null)
      {
        throw new ArgumentNullException(nameof(processEachLineAsyncFunc));
      }

      if (afterProcessingAllLinesAsyncFunc == null)
      {
        throw new ArgumentNullException(nameof(afterProcessingAllLinesAsyncFunc));
      }

      try
      {
        using (fileStreamReader)
        {
          var count = 1;
          while (fileStreamReader.Peek() > 0)
          {
            var line = await fileStreamReader.ReadLineAsync();
            if (line.Trim() == string.Empty)
            {
              continue; //skip empty lines
            }

            var columns = line.Split(Constant.ImportFile.COLUMN_SEPARATOR).Select(x => x.Trim()).ToList();

            //validate column count
            if (columns.Count != Constant.ImportFile.COLUMN_COUNT)
            {
              throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_COUNT_MISMATCH);
            }

            if (count++ == 1)
            {
              //validate column names
              if (columns.Any(column => !Constant.ExportFile.ColumnsList.Contains(column.Trim())))
              {
                throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_NAME_MISMATCH);
              }

              continue; //skip column headers column
            }

            ImportFileRecord newImportFileRecord = new ImportFileRecord(
              columns[0],
              columns[1],
              columns[2],
              columns[3],
              columns[4],
              columns[5],
              columns[6],
              columns[7],
              columns[8],
              columns[9],
              columns[10],
              columns[11],
              columns[12],
              columns[13],
              columns[14],
              columns[15],
              columns[16],
              columns[17],
              columns[18],
              columns[19],
              columns[20],
              columns[21],
              columns[22],
              columns[23],
              columns[24],
              columns[25],
              columns[26],
              columns[27],
              columns[28],
              columns[29],
              columns[30],
              columns[31]
              );

            await processEachLineAsyncFunc(newImportFileRecord);
          }

          await afterProcessingAllLinesAsyncFunc();
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(Constant.ErrorMessages.PARSE_FILE_CONTENTS_ERROR, ex);
      }
    }

    public async Task ValidateFileContentsAsync(StreamReader fileStreamReader)
    {
      if (fileStreamReader == null)
      {
        throw new ArgumentNullException(nameof(fileStreamReader));
      }

      try
      {
        using (fileStreamReader)
        {
          var count = 1;
          while (fileStreamReader.Peek() > 0)
          {
            var line = await fileStreamReader.ReadLineAsync();
            if (line.Trim() == string.Empty)
            {
              continue; //skip empty lines
            }

            var columns = line.Split(Constant.ImportFile.COLUMN_SEPARATOR).Select(x => x.Trim()).ToList();

            //validate column count
            await ValidateColumnCountAsync(columns);

            if (count++ != 1) continue;

            //validate column names
            await ValidateColumnNamesAsync(columns);

            //validate columns order
            await ValidateColumnOrderAsync(columns);
          }
        }
      }
      catch (Exception ex)
      {
        throw new MarkupUtilityException(Constant.ErrorMessages.VALIDATE_FILE_CONTENTS_ERROR, ex);
      }
    }

    private async Task ValidateColumnCountAsync(List<string> columns)
    {
      if (columns == null)
      {
        throw new ArgumentNullException(nameof(columns));
      }

      await Task.Run(() =>
      {
        if (columns.Count != Constant.ImportFile.COLUMN_COUNT)
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_COUNT_MISMATCH);
        }
      });
    }

    private async Task ValidateColumnNamesAsync(List<string> columns)
    {
      if (columns == null)
      {
        throw new ArgumentNullException(nameof(columns));
      }

      await Task.Run(() =>
      {
        if (columns.Any(column => !Constant.ExportFile.ColumnsList.Contains(column.Trim())))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_NAME_MISMATCH);
        }
      });
    }

    private async Task ValidateColumnOrderAsync(List<string> columns)
    {
      if (columns == null)
      {
        throw new ArgumentNullException(nameof(columns));
      }

      //validate column count
      if (columns.Count != Constant.ImportFile.COLUMN_COUNT)
      {
        throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_COUNT_MISMATCH);
      }

      await Task.Run(() =>
      {
        if (!string.Equals(columns[0], Constant.ExportFile.Columns.DOCUMENT_IDENTIFIER, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[1], Constant.ExportFile.Columns.FILE_ORDER, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[2], Constant.ExportFile.Columns.X, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[3], Constant.ExportFile.Columns.Y, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[4], Constant.ExportFile.Columns.WIDTH, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[5], Constant.ExportFile.Columns.HEIGHT, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[6], Constant.ExportFile.Columns.MARKUP_TYPE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[7], Constant.ExportFile.Columns.FILL_A, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[8], Constant.ExportFile.Columns.FILL_R, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[9], Constant.ExportFile.Columns.FILL_G, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[10], Constant.ExportFile.Columns.FILL_B, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[11], Constant.ExportFile.Columns.BORDER_SIZE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[12], Constant.ExportFile.Columns.BORDER_A, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[13], Constant.ExportFile.Columns.BORDER_R, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[14], Constant.ExportFile.Columns.BORDER_G, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[15], Constant.ExportFile.Columns.BORDER_B, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[16], Constant.ExportFile.Columns.BORDER_STYLE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[17], Constant.ExportFile.Columns.FONT_NAME, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[18], Constant.ExportFile.Columns.FONT_A, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[19], Constant.ExportFile.Columns.FONT_R, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[20], Constant.ExportFile.Columns.FONT_G, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[21], Constant.ExportFile.Columns.FONT_B, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[22], Constant.ExportFile.Columns.FONT_SIZE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[23], Constant.ExportFile.Columns.FONT_STYLE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[24], Constant.ExportFile.Columns.TEXT, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[25], Constant.ExportFile.Columns.Z_ORDER, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[26], Constant.ExportFile.Columns.DRAW_CROSS_LINES, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[27], Constant.ExportFile.Columns.MARKUP_SUB_TYPE, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[28], Constant.ExportFile.Columns.X_D, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[29], Constant.ExportFile.Columns.Y_D, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[30], Constant.ExportFile.Columns.WIDTH_D, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
        if (!string.Equals(columns[31], Constant.ExportFile.Columns.HEIGHT_D, StringComparison.CurrentCultureIgnoreCase))
        {
          throw new MarkupUtilityException(Constant.ErrorMessages.COLUMN_ORDER_MISMATCH);
        }
      });
    }
  }
}
