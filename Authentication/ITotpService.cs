namespace EcommerceWebApi.Authentication
{
    public interface ITotpService
    {
        string GenerateBase32Secret();
        string? GenerateQrCode(string? base32Secret, string? username);
        string? GetSecretFromQrCode(string qrCode);

        /// <summary>
        /// Asynchronously validates a TOTP code against the provided base-32 secret.
        /// Replaces the synchronous ValidateTotp to avoid blocking thread-pool threads
        /// in high-concurrency cloud environments (AWS ECS, Lambda, etc.).
        /// </summary>
        Task<bool> ValidateTotpAsync(string? base32Secret, string totp);
    }
}
