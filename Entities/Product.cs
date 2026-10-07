using EcommerceWebApi.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceWebApi.Entities
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Searchable]
        [Required(ErrorMessage = "{0} is required")]
        [Column("title")]
        [MaxLength(500)]
        public string Title { get; set; } = null!;

        [Sortable]
        [Required(ErrorMessage = "{0} is required")]
        [Column("price")]
        public float Price { get; set; }

        [Sortable]
        [Range(0, 5, ErrorMessage = "{0} must be between {1} and {2}")]
        [Column("rating")]
        public float Rating { get; set; }

        [Searchable]
        [Required(ErrorMessage = "{0} is required")]
        [Column("brand")]
        [MaxLength(255)]
        public string Brand { get; set; } = null!;

        [Searchable]
        [Column("category")]
        [MaxLength(255)]
        public string Category { get; set; } = null!;

        [Column("thumbnail")]
        public Uri Thumbnail { get; set; } = null!;

        [Sortable]
        [Required(ErrorMessage = "{0} is required")]
        [Column("quantity")]
        public int Quantity { get; set; }
    }
}
