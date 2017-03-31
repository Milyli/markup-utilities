using System.Threading.Tasks;
using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers
{
  public class MarkupTypeHelper : IMarkupTypeHelper
  {
    public async Task<int> GetMarkupSubTypeValueAsync(string markupSubType)
    {
      var retVal = -1;

      await Task.Run(() =>
      {
        switch (markupSubType)
        {
          case "Black":
            retVal = 1;
            break;
          case "Yellow":
            retVal = 2;
            break;
          case "Text":
            retVal = 3;
            break;
          case "Cross":
            retVal = 4;
            break;
          case "White":
            retVal = 5;
            break;
          case "Inverse":
            retVal = 6;
            break;
          case "Green":
            retVal = 7;
            break;
          case "Blue":
            retVal = 8;
            break;
          case "Orange":
            retVal = 9;
            break;
          case "Pink":
            retVal = 10;
            break;
          case "Purple":
            retVal = 11;
            break;
          default:
            throw new MarkupUtilityException($"Invalid MarkupSubType({markupSubType})");
        }
      });

      return retVal;
    }

    public async Task<string> GetMarkupSubTypeNameAsync(int markupSubType)
    {
      var retVal = string.Empty;

      await Task.Run(() =>
      {
        switch (markupSubType)
        {
          case 1:
            retVal = "Black";
            break;
          case 2:
            retVal = "Yellow";
            break;
          case 3:
            retVal = "Text";
            break;
          case 4:
            retVal = "Cross";
            break;
          case 5:
            retVal = "White";
            break;
          case 6:
            retVal = "Inverse";
            break;
          case 7:
            retVal = "Green";
            break;
          case 8:
            retVal = "Blue";
            break;
          case 9:
            retVal = "Orange";
            break;
          case 10:
            retVal = "Pink";
            break;
          case 11:
            retVal = "Purple";
            break;
          default:
            throw new MarkupUtilityException($"Invalid MarkupSubType({markupSubType})");
        }
      });

      return retVal;
    }
  }
}