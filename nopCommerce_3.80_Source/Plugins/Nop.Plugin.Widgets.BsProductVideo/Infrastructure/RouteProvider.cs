using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;
using Nop.Plugin.Widgets.BsProductVideo.ViewEngines;

namespace Nop.Plugin.Widgets.BsProductVideo.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
           // System.Web.Mvc.ViewEngines.Engines.Insert(0, new CustomViewEngine());
           
            routes.MapLocalizedRoute("BsProductVideo.ProductList",
                          "product-video/product-list",
                          new { controller = "ProductVideo", action = "List", area = "" },
                          new[] { "Nop.Plugin.Widgets.BsProductVideo.Controllers" });

            routes.MapLocalizedRoute("BsProductVideo.VideoCreate",
                          "product-video/create/{productId}",
                          new { controller = "ProductVideo", action = "VideoCreate", productId = UrlParameter.Optional, area = "" },
                          new[] { "Nop.Plugin.Widgets.BsProductVideo.Controllers" });
           
        }
        public int Priority
        {
            get
            {
                return 2;
            }
        }
    }
}
