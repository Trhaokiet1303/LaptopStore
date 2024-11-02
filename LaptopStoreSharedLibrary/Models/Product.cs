using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LaptopStoreSharedLibrary.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }
        
        [Required]
        public string? Description { get; set; }
        
        [Required, Range(0.1, 99999.99)]
        public string? Manufacturer { get; set; }
        
        [Required]
        public string? ManufacturerName { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required, DisplayName("Product Image")]
        public string? Base64img { get; set; }

        public int Quantity { get; set; }

        public bool Featured { get; set; }

        public DateTime dateUpload { get; set; } = DateTime.Now;
    }
}
