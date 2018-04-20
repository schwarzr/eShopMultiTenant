namespace Microsoft.eShopWeb
{
    public class TenantScope : ITenantScope
    {
        public string TenantKey { get; set; }
        public string ConnectionString { get; set; }
    }
}