using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace project_web2.Models
{
    public class Products
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        [Required]
        [Range(1, 5)]
        public int CategoryId { get; set; }

        [Range(0, 100000)]
        public int Stock { get; set; }

        public string? ImageFile { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
