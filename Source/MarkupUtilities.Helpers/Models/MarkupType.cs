using System.Data;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  public class MarkupType
  {
    public int Id { get; set; }
    public string Type { get; set; }

    public MarkupType(DataRow dataRow)
    {
      int idValue;
      var id = dataRow[Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.ID].ToString();

      if (int.TryParse(id, out idValue))
      {
        Id = idValue;
      }
      else
      {
        throw new MarkupUtilityException($"{Constant.Sql.WorkspaceTables.RedactionMarkupType.NAME} table: {Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.ID} value is not valid.");
      }

      var type = (dataRow[Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.TYPE] ?? string.Empty).ToString();
      if (string.IsNullOrWhiteSpace(type))
      {
        throw new MarkupUtilityException($"{Constant.Sql.WorkspaceTables.RedactionMarkupType.NAME} table: {Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.TYPE} value is not valid.");
      }

      Type = type;
    }
  }
}
