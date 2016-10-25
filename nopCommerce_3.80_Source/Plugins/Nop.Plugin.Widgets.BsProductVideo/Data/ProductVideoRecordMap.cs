using Nop.Data.Mapping;
using Nop.Plugin.Widgets.BsProductVideo.Domain;

namespace Nop.Plugin.Widgets.BsProductVideo.Data
{
    public partial class ProductVideoRecordMap : NopEntityTypeConfiguration<ProductVideoRecord>
    {
        public ProductVideoRecordMap()
        {
            this.ToTable("Bs_ProductVideoRecord");
            this.HasKey(x => x.Id);
        }
    }
}