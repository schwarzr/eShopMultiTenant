namespace Microsoft.eShopWeb
{
    public interface ITenantScope
    {
        string TenantKey { get; set; }

        string ConnectionString { get; set; }
    }
}