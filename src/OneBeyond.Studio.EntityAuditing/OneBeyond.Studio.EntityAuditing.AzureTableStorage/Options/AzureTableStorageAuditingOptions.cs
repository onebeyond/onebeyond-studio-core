namespace OneBeyond.Studio.EntityAuditing.AzureTableStorage.Options;

public sealed class AzureTableStorageAuditingOptions
{
    /// <summary>
    /// Set the connection string to connect to Azure storage
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Set the prefix to be used for each table entity
    /// Default value is "audit"
    /// </summary>
    public string TablePrefix { get; set; }

    /// <summary>
    /// Set this to true if you want to save the old/new values
    /// in dedicated tables instead that only as JSON field
    /// </summary>
    public bool ExpandValuesInTableColumns { get; set; }
}
