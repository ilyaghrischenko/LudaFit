namespace LudaFit.Infrastructure.AzureBlobStorage;

public sealed class AzureBlobContainerName
{
    private string Value { get; set; }
    
    public static AzureBlobContainerName SocialNetwork => new("social-networks");
    
    public static AzureBlobContainerName Specialist => new("specialists");

    private AzureBlobContainerName(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(AzureBlobContainerName containerName)
        => containerName.Value;
}
