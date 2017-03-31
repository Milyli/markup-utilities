using System;
using System.Web.Optimization;

namespace MarkupUtilities.CustomPages
{
  public class BundleConfig
  {
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.IgnoreList.Clear();

      var finalBundle = new ScriptBundle("~/JS");
      String[] scriptArray =
      {
        "~/Scripts/jquery-{version}.js",
        "~/Scripts/jquery.validate*",
        "~/Scripts/jquery.unobtrusive*",
        "~/Scripts/bootstrap.js",
        "~/js/workerAgent[index]-v1.0.js",
        "~/js/managerAgent[index]-v1.0.js"
      };
      finalBundle.Include(scriptArray);
      bundles.Add(finalBundle);

      bundles.Add(new StyleBundle("~/Content").Include(
        "~/Content/Site.css"));
      bundles.Add(new StyleBundle("~/Content/Bootstrap").Include(
          "~/Content/bootstrap.min.css",
          "~/Content/bootstrap-theme.min.css"));
    }
  }
}
