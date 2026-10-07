namespace EcommerceWebApi.Authentication
{
    public interface ITotpService
    {
        string GenerateBase32Secret();
        string? GenerateQrCode(string? base32Secret, string? username);
        string? GetSecretFromQrCode(string qrCode);

        // cr-dotnet-0037: Async overload for non-blocking TOTP validation in high-concurrency
        // AWS cloud environments. Callers should prefer ValidateTotpAsync over ValidateTotp.
        Task<bool> ValidateTotpAsync(string? base32Secret, string totp);

        // Synchronous wrapper retained for backward compatibility.
        bool ValidateTotp(string? base32Secret, string totp);

        // cr-dotnet-0037 / cr-dotnet-1000: Async method for non-blocking NIST time retrieval,
        // replacing the blocking .Result call for cloud auto-scaling compatibility.
        static Task<DateTime> GetNistTimeAsync() => TotpService.GetNistTimeAsync();
    }
}
