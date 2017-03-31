using System.Data;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  public class MarkupSubType
  {
    public int Id { get; set; }
    public string SubType { get; set; }

    public MarkupSubType(DataRow dataRow)
    {
      int idValue;
      var id = dataRow[Constant.Sql.WorkspaceTables.RedactionMarkupSubType.Columns.ID].ToString();

      if (int.TryParse(id, out idValue))
      {
        Id = idValue;
      }
      else
      {
        throw new MarkupUtilityException($"{Constant.Sql.WorkspaceTables.RedactionMarkupSubType.NAME} table: {Constant.Sql.WorkspaceTables.RedactionMarkupSubType.Columns.ID} value is not valid.");
      }

      var subType = (dataRow[Constant.Sql.WorkspaceTables.RedactionMarkupSubType.Columns.SUB_TYPE] ?? string.Empty).ToString();
      if (string.IsNullOrWhiteSpace(subType))
      {
        throw new MarkupUtilityException($"{Constant.Sql.WorkspaceTables.RedactionMarkupSubType.NAME} table: {Constant.Sql.WorkspaceTables.RedactionMarkupSubType.Columns.SUB_TYPE} value is not valid.");
      }

      SubType = subType;
    }
  }
}
