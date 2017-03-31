using System.Web.Mvc;

namespace MarkupUtilities.CustomPages
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new MyCustomErrorHandler());
		}
	}
}
