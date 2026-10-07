using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceWebApi.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [MaxLength(36)]
        [Required(ErrorMessage = "{0} is required")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "{0} is required")]
        [Column("username")]
        [MaxLength(255)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [Column("role")]
        [MaxLength(50)]
        public string Role { get; set; } = null!;

        [Column("is_two_factor_auth_activated")]
        public bool IsTwoFactorAuthActivated { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Column("password_salt")]
        public byte[] PasswordSalt { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [Column("password_hash")]
        public byte[] PasswordHash { get; set; } = null!;

        [Column("secret_code")]
        [MaxLength(255)]
        public string? SecretCode { get; set; }

        // Navigation property for EF Core relationship
        public RefreshToken RefreshToken { get; set; } = null!;
    }

    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(36)]
        public string UserId { get; set; } = null!;

        [Column("token")]
        public string? Token { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("expires")]
        public DateTime Expires { get; set; }

        // Navigation property back to User
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
