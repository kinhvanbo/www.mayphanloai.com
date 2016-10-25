using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Widgets.BsProductVideo.Data;
using Nop.Plugin.Widgets.BsProductVideo.Domain;
using Nop.Plugin.Widgets.BsProductVideo.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.BsProductVideo.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ProductVideoRecordService>().As<IProductVideoRecordService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<ProductVideoObjectContext>(builder, "nop_object_context_Bs_product_video");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ProductVideoRecord>>()
                .As<IRepository<ProductVideoRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_Bs_product_video"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }


       
    }
}
