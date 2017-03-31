using System;
using System.Collections;

namespace MarkupUtilities.Helpers.Models
{
  public class MarkupUtilityType : IEnumerable
  {
    public string Name { get; set; }
    public string Category { get; set; }

    public MarkupUtilityType(string name, string category)
    {
      if (name == null)
      {
        throw new ArgumentNullException(nameof(Name));
      }

      if (category == null)
      {
        throw new ArgumentNullException(nameof(Category));
      }

      Name = name;
      Category = category;
    }

    public override bool Equals(object obj)
    {
      var markupUtilityType = (MarkupUtilityType)obj;
      return markupUtilityType != null && (Name.Equals(markupUtilityType.Name) && Category.Equals(markupUtilityType.Category));
    }

    protected bool Equals(MarkupUtilityType other)
    {
      return string.Equals(Name, other.Name) && string.Equals(Category, other.Category);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((Name?.GetHashCode() ?? 0) * 397) ^ (Category?.GetHashCode() ?? 0);
      }
    }

    public IEnumerator GetEnumerator()
    {
      throw new NotImplementedException();
    }
  }
}
