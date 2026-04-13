namespace LudaFit.Infrastructure.Gmail.Settings;

public sealed record EmailSettings
{
    public required string OrganisationName { get; set; }
    
    public required string OrganisationEmail { get; set; }
    
    public required string SpecialistName { get; set; }
    
    public required string SpecialistEmail { get; set; }
    
    public required string SmtpServer { get; set; }
    
    public required int Port { get; set; }
    
    public required string Password { get; set; }
}
