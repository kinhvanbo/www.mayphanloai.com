
using Nop.Web.Framework.Themes;
namespace Nop.Plugin.Widgets.BsProductVideo.ViewEngines
{
    class CustomViewEngine : ThemeableRazorViewEngine 
    {
        
        public  CustomViewEngine()
        {



            ViewLocationFormats = new[]
                                             {
                                                 "~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/{0}.cshtml"
                                             };

            PartialViewLocationFormats = new[]
                                             {

                                                 "~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/{0}.cshtml"
                                             };

            
        }
    }
}
