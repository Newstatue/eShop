namespace Webhook.Options;

sealed record KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// Keycloak 基础地址，形如 https://idp.example.com/ 。
    /// </summary>
    public string BaseAddress { get; init; } = string.Empty;

    /// <summary>
    /// 需要操作的 Realm 名称。
    /// </summary>
    public string Realm { get; init; } = string.Empty;

    /// <summary>
    /// 用于获取管理员访问令牌的客户端 ID。
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// 用于获取管理员访问令牌的客户端密钥。
    /// </summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// 令牌到期前的缓冲秒数，用于提前刷新。
    /// </summary>
    public int RefreshSkewSeconds { get; init; } = 30;
}
