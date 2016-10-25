using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Plugin.Widgets.BsProductVideo.Models;
using Nop.Plugin.Widgets.BsProductVideo.Services;
using Nop.Plugin.Widgets.BsProductVideo.Domain;
using Nop.Admin.Extensions;
using Nop.Admin.Controllers;
using Nop.Admin.Models.Catalog;
using Nop.Services.Shipping;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services;

namespace Nop.Plugin.Widgets.BsProductVideo.Controllers
{

    public class ProductVideoController : BasePluginController
    {
        private readonly IProductVideoRecordService _productVideoRecordService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly ProductVideoSettings _productVideoSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IShippingService _shippingService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;

        public ProductVideoController(IProductVideoRecordService productVideoRecordService,
            IProductService productService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPluginFinder pluginFinder,
            ILogger logger,
            IWebHelper webHelper,
            IStoreService storeService,
            ProductVideoSettings productVideoSettings,
            ISettingService settingService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IVendorService vendorService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IWorkContext workContext,
            IShippingService shippingService,
            IProductAttributeService productAttributeService,
            ICustomerService customerService,
            CustomerSettings customerSettings, MediaSettings mediaSettings)
        {
            this._productVideoRecordService = productVideoRecordService;
            this._productService = productService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._productVideoSettings = productVideoSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;

            this._categoryService = categoryService;
            this._vendorService = vendorService;
            this._manufacturerService = manufacturerService;
            this._workContext = workContext;
            this._shippingService = shippingService;
            this._productAttributeService = productAttributeService;
            this._customerService = customerService;
            this._customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
        }

        #region Utilities
        [NonAction]
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            var categoriesIds = new List<int>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        } 
        #endregion

        #region Methods
        [ChildActionOnly]

        public ActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            

            var model = new ProductListModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var wh in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            //return View(model);

            return View("~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/Configure.cshtml", model);
        }

        //[HttpPost]

        //[ChildActionOnly]
        //[FormValueRequired("save")]
        //public ActionResult Configure(ProductVideoRecordModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Configure();
        //    }

        //    var productVideoRecord = new ProductVideoRecord
        //    {
        //        ProductId = model.ProductId,
        //        EmbedVideoHtmlCode = model.EmbedVideoHtmlCode,
        //        VideoThumbId = model.VideoThumbId

        //    };
        //    _productVideoRecordService.InsertProductVideoRecord(productVideoRecord);

        //    SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

        //    //redisplay the form
        //    return Configure();
        //}

        
        [AdminAuthorize]
        public ActionResult VideoCreate(int productId = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var model = new ProductVideoRecordModel();
            model.ProductId = productId;

            return View("~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/VideoCreate.cshtml", model);
        }
           [AdminAuthorize]
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult VideoCreate(ProductVideoRecordModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            if (!ModelState.IsValid)
            {
                return Configure();
            }

            var productVideoRecord = new ProductVideoRecord
            {
                ProductId = model.ProductId,
                EmbedVideoHtmlCode = model.EmbedVideoHtmlCode,
                VideoThumbId = model.VideoThumbId,
                DisplayOrder = model.DisplayOrder

            };
            _productVideoRecordService.InsertProductVideoRecord(productVideoRecord);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            //redisplay the form
            return View("~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/VideoCreate.cshtml", model);
        }

        [HttpPost]

        [AdminAntiForgery]
        public ActionResult ProductVideoRecordList(DataSourceRequest command, int productId = 0)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var productVideoRecords = _productVideoRecordService.GetProductVideoRecords(pageIndex: command.Page - 1,
                pageSize: command.PageSize, productId: productId);
            var productsModel = productVideoRecords
                .Select(x =>
                {
                    var model = new ProductVideoRecordModel()
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        EmbedVideoHtmlCode = x.EmbedVideoHtmlCode,
                        VideoThumbId = x.VideoThumbId,
                        VideoThumbUrl = _pictureService.GetPictureUrl(x.VideoThumbId, 100),
                        DisplayOrder = x.DisplayOrder

                    };
                    var product = _productService.GetProductById(x.ProductId);
                    if (product != null)
                    {
                        model.ProductName = product.Name;

                    }

                    return model;
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productsModel,
                Total = productVideoRecords.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]

        [AdminAntiForgery]
        public ActionResult ProductVideoRecordUpdate(ProductVideoRecordModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var productVideoRecor = _productVideoRecordService.GetById(model.Id);
            if (productVideoRecor != null)
            {
                //productVideoRecor.ProductId = model.ProductId;
                productVideoRecor.EmbedVideoHtmlCode = model.EmbedVideoHtmlCode;
                productVideoRecor.DisplayOrder = model.DisplayOrder;

                _productVideoRecordService.UpdateProductVideoRecord(productVideoRecor);

            }

            return new NullJsonResult();
        }

        [HttpPost]

        public ActionResult ProductVideoRecordDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied"); ;

            var productVideoRecord = _productVideoRecordService.GetById(id);
            if (productVideoRecord == null)
                throw new ArgumentException("No record found with the specified id");
            var picture = _pictureService.GetPictureById(productVideoRecord.VideoThumbId);
            if (picture != null)
                _pictureService.DeletePicture(picture);
            _productVideoRecordService.DeleteProductVideoRecord(productVideoRecord);


            return new NullJsonResult();
        } 
        #endregion

        #region Product List

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            var model = new ProductListModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var wh in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = "0" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            return View("~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/List.cshtml", model);
        }

        [HttpPost]
        public ActionResult ProductList(DataSourceRequest command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: overridePublished
            );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                //little hack here:
                //ensure that product full descriptions are not returned
                //otherwise, we can get the following error if products have too long descriptions:
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                productModel.FullDescription = "";

                productModel.DownloadExpirationDays = _productVideoRecordService.GetByProductId(x.Id).Count();
                
                //picture
                var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture, 75, true);
                //product type
                productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                //friendly stock qantity
                //if a simple product AND "manage inventory" is "Track inventory", then display
                if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();

                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

       
        #endregion

        

        #region public part
        //[ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {

            var wZone = widgetZone;
            
            if (wZone == "productdetails_after_pictures" && additionalData != null)
            {
                var product = _productService.GetProductById((int)additionalData);
                var model = new PublicInfoModel()
                {
                    ProductId = product.Id
                };
                var productVideoRecords = _productVideoRecordService.GetByProductId(product.Id);
                if (productVideoRecords.Count == 0) 
                    return Content("");
                foreach (var embedVideoModel in productVideoRecords.Select(productVideoRecord => new PublicInfoModel.EmbedVideoModel()
                {
                    Id = productVideoRecord.Id,
                    VideoThumbUrl = _pictureService.GetPictureUrl(productVideoRecord.VideoThumbId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                    EmbedVideoHtmlCode = productVideoRecord.EmbedVideoHtmlCode
                }))
                {
                    model.EmbedVideoRecordModels.Add(embedVideoModel);
                }

                return View("~/Plugins/Widgets.BsProductVideo/Views/ProductVideo/PublicInfo.cshtml", model);
            }
           
         return Content("");
        }

        #endregion
    }
}
