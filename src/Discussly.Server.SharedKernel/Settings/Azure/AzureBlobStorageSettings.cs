namespace Discussly.Server.SharedKernel.Settings.Azure
{
    public class AzureBlobStorageSettings
    {
        public string ConnectionString { get; set; } = default!;

        public string ContainerName { get; set; } = default!;
    }
}