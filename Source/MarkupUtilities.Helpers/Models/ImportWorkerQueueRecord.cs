using System;
using System.Data;
using System.Text;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  /// <summary>
  /// Represents a single row in the Import Worker Queue Table 
  /// </summary>
  public class ImportWorkerQueueRecord
  {
    public Int32 Id { get; private set; }
    public DateTime TimeStampUtc { get; private set; }
    public Int32 WorkspaceArtifactId { get; private set; }
    public String DocumentIdentifier { get; private set; }
    public Int32 FileOrder { get; private set; }
    public Int32 QueueStatus { get; private set; }
    public Int32? AgentId { get; private set; }
    public Int32 ImportJobArtifactId { get; private set; }
    public String JobType { get; private set; }
    public Int32 X { get; private set; }
    public Int32 Y { get; private set; }
    public Int32 Width { get; private set; }
    public Int32 Height { get; private set; }
    public Int16 MarkupType { get; private set; }
    public Int16? FillA { get; private set; }
    public Int16? FillR { get; private set; }
    public Int16? FillG { get; private set; }
    public Int16? FillB { get; private set; }
    public Int32? BorderSize { get; private set; }
    public Int16? BorderA { get; private set; }
    public Int16? BorderR { get; private set; }
    public Int16? BorderG { get; private set; }
    public Int16? BorderB { get; private set; }
    public Int16? BorderStyle { get; private set; }
    public String FontName { get; private set; }
    public Int16? FontA { get; private set; }
    public Int16? FontR { get; private set; }
    public Int16? FontG { get; private set; }
    public Int16? FontB { get; private set; }
    public Int32? FontSize { get; private set; }
    public Int16? FontStyle { get; private set; }
    public String Text { get; private set; }
    public Int32 ZOrder { get; private set; }
    public Boolean DrawCrossLines { get; private set; }
    public Int16 MarkupSubType { get; private set; }
    public Int32 ResourceGroupId { get; private set; }
    public Boolean SkipDuplicateRedactions { get; private set; }
    public decimal? Xd { get; private set; }
    public decimal? Yd { get; private set; }
    public decimal? WidthD { get; private set; }
    public decimal? HeightD { get; private set; }

    public ImportWorkerQueueRecord(DataRow dataRow)
    {
      if (dataRow == null)
      {
        throw new ArgumentNullException(nameof(dataRow));
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.TIME_STAMP_UTC] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.TIME_STAMP_UTC} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WORKSPACE_ARTIFACT_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WORKSPACE_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DOCUMENT_IDENTIFIER] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DOCUMENT_IDENTIFIER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILE_ORDER] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILE_ORDER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.QUEUE_STATUS] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.QUEUE_STATUS} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.IMPORT_JOB_ARTIFACT_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.IMPORT_JOB_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.JOB_TYPE] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.JOB_TYPE} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.X] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.X} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Y] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Y} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WIDTH] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WIDTH} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.HEIGHT] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.HEIGHT} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_TYPE] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_TYPE} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Z_ORDER] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Z_ORDER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DRAW_CROSS_LINES] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DRAW_CROSS_LINES} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_SUB_TYPE] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_SUB_TYPE} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.RESOURCE_GROUP_ID] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.RESOURCE_GROUP_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ImportWorkerQueue.Columns.SKIP_DUPLICATE_REDACTIONS] == null)
      {
        throw new MarkupUtilityException($"{Constant.Sql.AdminTables.ImportWorkerQueue.Columns.SKIP_DUPLICATE_REDACTIONS} is NULL");
      }

      Id = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.ID);
      TimeStampUtc = dataRow.Field<DateTime>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.TIME_STAMP_UTC);
      WorkspaceArtifactId = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WORKSPACE_ARTIFACT_ID);
      DocumentIdentifier = dataRow.Field<String>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DOCUMENT_IDENTIFIER);
      FileOrder = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILE_ORDER);
      QueueStatus = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.QUEUE_STATUS);
      AgentId = dataRow.Field<Int32?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.AGENT_ID);
      ImportJobArtifactId = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.IMPORT_JOB_ARTIFACT_ID);
      JobType = dataRow.Field<String>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.JOB_TYPE);
      X = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.X);
      Y = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Y);
      Width = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WIDTH);
      Height = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.HEIGHT);
      MarkupType = dataRow.Field<Int16>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_TYPE);
      FillA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILL_A);
      FillR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILL_R);
      FillG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILL_G);
      FillB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FILL_B);
      BorderSize = dataRow.Field<Int32?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_SIZE);
      BorderA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_A);
      BorderR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_R);
      BorderG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_G);
      BorderB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_B);
      BorderStyle = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.BORDER_STYLE);
      FontName = dataRow.Field<String>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_NAME);
      FontA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_A);
      FontR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_R);
      FontG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_G);
      FontB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_B);
      FontSize = dataRow.Field<Int32?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_SIZE);
      FontStyle = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.FONT_STYLE);
      Text = dataRow.Field<String>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.TEXT);
      ZOrder = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Z_ORDER);
      DrawCrossLines = dataRow.Field<Boolean>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.DRAW_CROSS_LINES);
      MarkupSubType = dataRow.Field<Int16>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.MARKUP_SUB_TYPE);
      ResourceGroupId = dataRow.Field<Int32>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.RESOURCE_GROUP_ID);
      SkipDuplicateRedactions = dataRow.Field<Boolean>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.SKIP_DUPLICATE_REDACTIONS);
      Xd = dataRow.Field<decimal?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.X_D);
      Yd = dataRow.Field<decimal?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.Y_D);
      WidthD = dataRow.Field<decimal?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.WIDTH_D);
      HeightD = dataRow.Field<decimal?>(Constant.Sql.AdminTables.ImportWorkerQueue.Columns.HEIGHT_D);
    }

    public String ToStringRedactionData()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append($"{nameof(FileOrder)}: {FileOrder}, ");
      sb.Append($"{nameof(X)}: {X}, ");
      sb.Append($"{nameof(Y)}: {Y}, ");
      sb.Append($"{nameof(Width)}: {Width}, ");
      sb.Append($"{nameof(Height)}: {Height}, ");
      sb.Append($"{nameof(MarkupType)}: {MarkupType}, ");
      sb.Append($"{nameof(FillA)}: {(FillA == null ? "null" : FillA.ToString())}, ");
      sb.Append($"{nameof(FillR)}: {(FillR == null ? "null" : FillR.ToString())}, ");
      sb.Append($"{nameof(FillG)}: {(FillG == null ? "null" : FillG.ToString())}, ");
      sb.Append($"{nameof(FillB)}: {(FillB == null ? "null" : FillB.ToString())}, ");
      sb.Append($"{nameof(BorderSize)}: {(BorderSize == null ? "null" : BorderSize.ToString())}, ");
      sb.Append($"{nameof(BorderA)}: {(BorderA == null ? "null" : BorderA.ToString())}, ");
      sb.Append($"{nameof(BorderR)}: {(BorderR == null ? "null" : BorderR.ToString())}, ");
      sb.Append($"{nameof(BorderG)}: {(BorderG == null ? "null" : BorderG.ToString())}, ");
      sb.Append($"{nameof(BorderB)}: {(BorderB == null ? "null" : BorderB.ToString())}, ");
      sb.Append($"{nameof(BorderStyle)}: {(BorderStyle == null ? "null" : BorderStyle.ToString())}, ");
      sb.Append($"{nameof(FontName)}: {FontName}, ");
      sb.Append($"{nameof(FontA)}: {(FontA == null ? "null" : FontA.ToString())}, ");
      sb.Append($"{nameof(FontR)}: {(FontR == null ? "null" : FontR.ToString())}, ");
      sb.Append($"{nameof(FontG)}: {(FontG == null ? "null" : FontG.ToString())}, ");
      sb.Append($"{nameof(FontB)}: {(FontB == null ? "null" : FontB.ToString())}, ");
      sb.Append($"{nameof(FontSize)}: {(FontSize == null ? "null" : FontSize.ToString())}, ");
      sb.Append($"{nameof(FontStyle)}: {(FontStyle == null ? "null" : FontStyle.ToString())}, ");
      sb.Append($"{nameof(Text)}: {Text}, ");
      sb.Append($"{nameof(ZOrder)}: {ZOrder}, ");
      sb.Append($"{nameof(DrawCrossLines)}: {DrawCrossLines}, ");
      sb.Append($"{nameof(MarkupSubType)}: {MarkupSubType}, ");
      sb.Append($"{nameof(Xd)}: {Xd}, ");
      sb.Append($"{nameof(Yd)}: {Yd}, ");
      sb.Append($"{nameof(WidthD)}: {WidthD}, ");
      sb.Append($"{nameof(HeightD)}: {HeightD}");

      String retVal = sb.ToString();
      return retVal;
    }
  }
}
