using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceWebApi.Entities
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        [MaxLength(36)]
        [Required(ErrorMessage = "{0} is required")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "{0} is required")]
        [Column("user_id")]
        [MaxLength(36)]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [Column("product_list", TypeName = "jsonb")]
        public Dictionary<int, int> ProductList { get; set; } = null!;

        [Required(ErrorMessage = "{0} is required")]
        [Column("created")]
        public DateTime Created { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Column("status")]
        public OrderStatus Status { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Successed,
        Canceled
    }
}
