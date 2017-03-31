using System;
using System.Data;
using System.Text;

namespace MarkupUtilities.Helpers.Models
{
  public class ExportResultsRecord
  {
    public Int32 Id { get; private set; }
    public DateTime TimeStampUtc { get; private set; }
    public Int32 WorkspaceArtifactId { get; private set; }
    public Int32 ExportJobArtifactId { get; private set; }
    public String DocumentIdentifier { get; private set; }
    public Int32 FileOrder { get; private set; }
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
    public Decimal? Xd { get; private set; }
    public Decimal? Yd { get; private set; }
    public Decimal? WidthD { get; private set; }
    public Decimal? HeightD { get; private set; }

    public ExportResultsRecord(DataRow dataRow)
    {
      if (dataRow == null)
      {
        throw new ArgumentNullException(nameof(dataRow));
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.ID] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.TIME_STAMP_UTC] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.TIME_STAMP_UTC} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.WORKSPACE_ARTIFACT_ID] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.WORKSPACE_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.DOCUMENT_IDENTIFIER] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.DOCUMENT_IDENTIFIER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.EXPORT_JOB_ARTIFACT_ID] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.EXPORT_JOB_ARTIFACT_ID} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.FILE_ORDER] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.FILE_ORDER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.X] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.X} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.Y] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.Y} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.WIDTH] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.WIDTH} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.HEIGHT] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.HEIGHT} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_TYPE] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_TYPE} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.Z_ORDER] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.Z_ORDER} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.DRAW_CROSS_LINES] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.DRAW_CROSS_LINES} is NULL");
      }

      if (dataRow[Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_SUB_TYPE] == null)
      {
        throw new Exception($"{Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_SUB_TYPE} is NULL");
      }

      Id = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.ID);
      TimeStampUtc = dataRow.Field<DateTime>(Constant.Sql.AdminTables.ExportResults.Columns.TIME_STAMP_UTC);
      WorkspaceArtifactId = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.WORKSPACE_ARTIFACT_ID);
      DocumentIdentifier = dataRow.Field<String>(Constant.Sql.AdminTables.ExportResults.Columns.DOCUMENT_IDENTIFIER);
      ExportJobArtifactId = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.EXPORT_JOB_ARTIFACT_ID);
      FileOrder = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.FILE_ORDER);
      X = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.X);
      Y = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.Y);
      Width = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.WIDTH);
      Height = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.HEIGHT);
      MarkupType = dataRow.Field<Int16>(Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_TYPE);
      FillA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FILL_A);
      FillR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FILL_R);
      FillG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FILL_G);
      FillB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FILL_B);
      BorderSize = dataRow.Field<Int32?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_SIZE);
      BorderA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_A);
      BorderR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_R);
      BorderG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_G);
      BorderB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_B);
      BorderStyle = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.BORDER_STYLE);
      FontName = dataRow.Field<String>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_NAME);
      FontA = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_A);
      FontR = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_R);
      FontG = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_G);
      FontB = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_B);
      FontSize = dataRow.Field<Int32?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_SIZE);
      FontStyle = dataRow.Field<Int16?>(Constant.Sql.AdminTables.ExportResults.Columns.FONT_STYLE);
      Text = dataRow.Field<String>(Constant.Sql.AdminTables.ExportResults.Columns.TEXT);
      ZOrder = dataRow.Field<Int32>(Constant.Sql.AdminTables.ExportResults.Columns.Z_ORDER);
      DrawCrossLines = dataRow.Field<Boolean>(Constant.Sql.AdminTables.ExportResults.Columns.DRAW_CROSS_LINES);
      MarkupSubType = dataRow.Field<Int16>(Constant.Sql.AdminTables.ExportResults.Columns.MARKUP_SUB_TYPE);
      Xd = dataRow.Field<Decimal?>(Constant.Sql.AdminTables.ExportResults.Columns.X_D);
      Yd = dataRow.Field<Decimal?>(Constant.Sql.AdminTables.ExportResults.Columns.Y_D);
      WidthD = dataRow.Field<Decimal?>(Constant.Sql.AdminTables.ExportResults.Columns.WIDTH_D);
      HeightD = dataRow.Field<Decimal?>(Constant.Sql.AdminTables.ExportResults.Columns.HEIGHT_D);
    }

    public override String ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.Append($"{DocumentIdentifier},");
      sb.Append($"{FileOrder},");
      sb.Append($"{X},");
      sb.Append($"{Y},");
      sb.Append($"{Width},");
      sb.Append($"{Height},");
      sb.Append($"{MarkupType},");
      sb.Append($"{(FillA == null ? "null" : FillA.ToString())},");
      sb.Append($"{(FillR == null ? "null" : FillR.ToString())},");
      sb.Append($"{(FillG == null ? "null" : FillG.ToString())},");
      sb.Append($"{(FillB == null ? "null" : FillB.ToString())},");
      sb.Append($"{(BorderSize == null ? "null" : BorderSize.ToString())},");
      sb.Append($"{(BorderA == null ? "null" : BorderA.ToString())},");
      sb.Append($"{(BorderR == null ? "null" : BorderR.ToString())},");
      sb.Append($"{(BorderG == null ? "null" : BorderG.ToString())},");
      sb.Append($"{(BorderB == null ? "null" : BorderB.ToString())},");
      sb.Append($"{(BorderStyle == null ? "null" : BorderStyle.ToString())},");
      sb.Append($"{FontName},");
      sb.Append($"{(FontA == null ? "null" : FontA.ToString())},");
      sb.Append($"{(FontR == null ? "null" : FontR.ToString())},");
      sb.Append($"{(FontG == null ? "null" : FontG.ToString())},");
      sb.Append($"{(FontB == null ? "null" : FontB.ToString())},");
      sb.Append($"{(FontSize == null ? "null" : FontSize.ToString())},");
      sb.Append($"{(FontStyle == null ? "null" : FontStyle.ToString())},");
      sb.Append($"{Text},");
      sb.Append($"{ZOrder},");
      String drawCrossLinesValue = DrawCrossLines ? "1" : "0";
      sb.Append($"{drawCrossLinesValue},");
      sb.Append($"{MarkupSubType},");
      sb.Append($"{(Xd == null ? "null" : Xd.ToString())},");
      sb.Append($"{(Yd == null ? "null" : Yd.ToString())},");
      sb.Append($"{(WidthD == null ? "null" : WidthD.ToString())},");
      sb.Append($"{(HeightD == null ? "null" : HeightD.ToString())}");

      String retVal = sb.ToString();
      return retVal;
    }
  }
}
