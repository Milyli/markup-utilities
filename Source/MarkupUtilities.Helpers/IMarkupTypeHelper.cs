using System.Threading.Tasks;

namespace MarkupUtilities.Helpers
{
  public interface IMarkupTypeHelper
  {
    Task<int> GetMarkupSubTypeValueAsync(string markupSubType);
    Task<string> GetMarkupSubTypeNameAsync(int markupSubType);
  }
}