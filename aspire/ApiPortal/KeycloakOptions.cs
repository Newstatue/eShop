namespace ApiPortal;

public sealed class KeycloakOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public KeycloakDefaultUserOptions DefaultUser { get; set; } = new();
}

public sealed class KeycloakDefaultUserOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
