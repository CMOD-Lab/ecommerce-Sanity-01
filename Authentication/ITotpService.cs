namespace EcommerceWebApi.Authentication
{
    public interface ITotpService
    {
        string GenerateBase32Secret();
        string? GenerateQrCode(string? base32Secret, string? username);
        string? GetSecretFromQrCode(string qrCode);
        // cr-dotnet-0037: Replaced synchronous ValidateTotp with async Task<bool> ValidateTotpAsync
        // to propagate async/await throughout the call chain and eliminate blocking HTTP calls.
        Task<bool> ValidateTotpAsync(string? base32Secret, string totp);
    }
}
