using System;

namespace MarkupUtilities.Helpers.Models
{
  public class ChoiceModel
  {
    public int ArtifactId { get; set; }
    public string Name { get; set; }

    public ChoiceModel(int artifactId, string name)
    {
      if (name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      ArtifactId = artifactId;
      Name = name;
    }
  }
}
