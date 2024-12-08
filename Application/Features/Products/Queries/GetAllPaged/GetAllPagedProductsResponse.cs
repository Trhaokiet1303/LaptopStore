namespace LaptopStore.Application.Features.Products.Queries.GetAllPaged
{
    public class GetAllPagedProductsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public int Price { get; set; }
        public string CPU { get; set; }
        public string Screen { get; set; }
        public string Card { get; set; }
        public string Ram { get; set; }
        public string Rom { get; set; }
        public string Battery { get; set; }
        public string Weight { get; set; }
        public string Description { get; set; }
        public decimal Rate { get; set; }
        public bool Featured { get; set; }
        public string Brand { get; set; }
        public string ProductLine { get; set; }
        public int Quantity { get; set; }
        public int BrandId { get; set; }
        public string ImageDataURL { get; set; }

    }
}