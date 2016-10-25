using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Plugin.Widgets.BsProductVideo.Data;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.BsProductVideo
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class ProductVideoPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ProductVideoObjectContext _objectContext;

        public ProductVideoPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper, ProductVideoObjectContext objectContext)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._objectContext = objectContext;
        }



        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "productdetails_after_pictures"};
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ProductVideo";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Widgets.BsProductVideo.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "ProductVideo";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.Widgets.BsProductVideo.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //save settings
            
            //_settingService.SaveSetting(settings);


            this.AddOrUpdatePluginLocaleResource("Nop.Plugin.Widgets.BsProductVideo.Button.AddVideo", "Add Video/Audio");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.BsProductVideo.ProductId", "Product Id");

            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.BsProductVideo.ProductName", "Product Name");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.BsProductVideo.EmbedVideoHtmlCode", "Embed Video Audio Html Code");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.BsProductVideo.Picture", "Thumbnail");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.BsProductVideo.DisplayOrder", "Display Order");
            
               //data
            _objectContext.Install();


            base.Install();


           
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            //_settingService.DeleteSetting<SettingsName>();

            //locales
            this.DeletePluginLocaleResource("key");

            this.DeletePluginLocaleResource("Nop.Plugin.Widgets.BsProductVideo.Button.AddVideo");
            this.DeletePluginLocaleResource("Plugin.Widgets.BsProductVideo.ProductId");

            this.DeletePluginLocaleResource("Plugin.Widgets.BsProductVideo.ProductName");
            this.DeletePluginLocaleResource("Plugin.Widgets.BsProductVideo.EmbedVideoHtmlCode");
            this.DeletePluginLocaleResource("Plugin.Widgets.BsProductVideo.Picture");
            this.DeletePluginLocaleResource("Plugin.Widgets.BsProductVideo.DisplayOrder");
            
           //data
            _objectContext.Uninstall();

            base.Uninstall();
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {

           

            var menuItem = new SiteMapNode()
            {
                Visible = true,
                Title = "Product Video/Audio",
                Url = ""
            };

            var menuItemProductList = new SiteMapNode()
            {
                Visible = true,
                Title = "Configure",
                Url = "/product-video/product-list",
                RouteValues = new RouteValueDictionary() { { "Area", "Admin" } }
            };

            menuItem.ChildNodes.Add(menuItemProductList);

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
            {
                var sohel = new SiteMapNode()
                {
                    Visible = true,
                    Title = "SohelPlugins",
                    Url = "",
                    SystemName = "nopSohel"
                };
                sohel.ChildNodes.Add(menuItem);

                rootNode.ChildNodes.Add(sohel);
            }

           // rootNode.ChildNodes.Add(menuItemBuilder);
            
        }
    }
}
