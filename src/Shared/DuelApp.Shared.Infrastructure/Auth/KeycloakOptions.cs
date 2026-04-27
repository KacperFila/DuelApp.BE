namespace DuelApp.Shared.Infrastructure.Auth;

public class KeycloakOptions
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public string? MetadataAddress { get; set; }
    public bool RequireHttpsMetadata { get; set; }
}