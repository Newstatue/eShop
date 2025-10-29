using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using Webhook.Models;
using Webhook.Services;

namespace Webhook.Endpoints;

sealed class KeycloakWebhookEndpoint(
    IOptions<WebhookOptions> webhookOptions,
    ILogger<KeycloakWebhookEndpoint> logger,
    IEnumerable<IWebhookEventProcessor> processors)
{
    private const string DefaultSignatureHeader = "X-Keycloak-Signature";

    private readonly WebhookOptions _options = webhookOptions.Value;
    private readonly IReadOnlyList<IWebhookEventProcessor> _processors = processors.ToList();
    private readonly ConcurrentDictionary<string, byte> _missingSignatureWarnings = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IResult> HandleAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await request.Body.CopyToAsync(buffer, cancellationToken);

        var payloadBytes = buffer.ToArray();
        if (payloadBytes.Length == 0)
        {
            return Results.BadRequest(new { error = "请求正文不能为空。" });
        }

        var headerSnapshot = request.Headers.ToDictionary(static h => h.Key, static h => h.Value.ToString());
        var secret = _options.Secret;

        if (!string.IsNullOrWhiteSpace(secret))
        {
            var headerName = string.IsNullOrWhiteSpace(_options.SignatureHeader)
                ? DefaultSignatureHeader
                : _options.SignatureHeader;

            if (!request.Headers.TryGetValue(headerName, out var signatureValues))
            {
                if (_missingSignatureWarnings.TryAdd(headerName, 0))
                {
                    logger.LogWarning("缺少签名头 {Header}，已拒绝本次请求。当前请求头：{Headers}",
                        headerName, headerSnapshot);
                }
                else
                {
                    logger.LogWarning("缺少签名头 {Header}，已拒绝本次请求。", headerName);
                }

                return Results.Unauthorized();
            }

            var signature = signatureValues.ToString();
            if (!ValidateSignature(signature, payloadBytes, secret))
            {
                logger.LogWarning("签名校验未通过，已拒绝本次请求。当前请求头：{Headers}", headerSnapshot);
                return Results.Unauthorized();
            }
        }
        else
        {
            logger.LogWarning("未配置签名密钥，已跳过签名校验。当前请求头：{Headers}", headerSnapshot);
        }

        using var payloadDocument = JsonDocument.Parse(payloadBytes);
        var webhookEvent = KeycloakWebhookEvent.From(
            payloadDocument.RootElement,
            Encoding.UTF8.GetString(payloadBytes),
            headerSnapshot);

        foreach (var processor in _processors)
        {
            await processor.ProcessAsync(webhookEvent, cancellationToken).ConfigureAwait(false);
        }

        return Results.Ok(new { status = "已接收" });
    }

    private static bool ValidateSignature(string? header, byte[] body, string secret)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return false;
        }

        var trimmed = header.Trim();
        const string prefix = "sha256=";
        if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[prefix.Length..];
        }

        byte[] headerBytes;
        try
        {
            headerBytes = Convert.FromHexString(trimmed);
        }
        catch (FormatException)
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computed = hmac.ComputeHash(body);

        return CryptographicOperations.FixedTimeEquals(headerBytes, computed);
    }
}
