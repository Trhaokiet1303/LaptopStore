using LaptopStore.Application.Features.Products.Queries.GetAllPaged;
using LaptopStore.Application.Requests.Catalog;
using LaptopStore.Client.Extensions;
using LaptopStore.Shared.Constants.Application;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LaptopStore.Application.Features.Products.Queries.GetProductById;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Client.Pages.Admin.Products;
using LaptopStore.Application.Specifications.Catalog;
using Microsoft.AspNetCore.WebUtilities;
using LaptopStore.Domain.Entities.Catalog;
using System.Text.Json;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class ListProducts
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; }

        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private IEnumerable<GetAllPagedProductsResponse> _pagedData;
        private MudTable<GetAllPagedProductsResponse> _table;

        private bool isFilterPanelVisible = false;

        private int _totalItems;
        private int _currentPage;
        private string _searchString = "";
        private bool _loaded;
        private List<Product> allProducts = new List<Product>();

        public List<BrandFilter> _brands = new List<BrandFilter>
        {
            new BrandFilter { Name = "Macbook", LogoPath = "/images/brand/mac-icon.png" },
            new BrandFilter { Name = "Lenovo", LogoPath = "/images/brand/lenovo-icon.png" },
            new BrandFilter { Name = "Asus", LogoPath = "/images/brand/asus-icon.png" },
            new BrandFilter { Name = "MSI", LogoPath = "/images/brand/msi-icon.png" },
            new BrandFilter { Name = "HP", LogoPath = "/images/brand/hp-icon.png" },
            new BrandFilter { Name = "Acer", LogoPath = "/images/brand/acer-icon.png" },
            new BrandFilter { Name = "Samsung", LogoPath = "/images/brand/samsung-icon.png" },
            new BrandFilter { Name = "Dell", LogoPath = "/images/brand/dell-icon.png" }
        };

        public List<DescriptionFilter> _descriptions = new List<DescriptionFilter>
        {
            new DescriptionFilter { Name = "Gaming",DescriptionPath="/images/description/Gaming-Lap.png" },
            new DescriptionFilter { Name = "Office",DescriptionPath="/images/description/Office-Lap.png" },
            new DescriptionFilter { Name = "Ultrabook",DescriptionPath="/images/description/Book-Lap.png" },
            new DescriptionFilter { Name = "AI",DescriptionPath="/images/description/AI-Lap.png" },
            new DescriptionFilter { Name = "Graphic",DescriptionPath="/images/description/Graphic-Lap.png" },
        };

        public List<PriceFilter> _priceRanges = new List<PriceFilter>
        {
            new PriceFilter { Name = "Dưới 10Tr", MinPrice = 0, MaxPrice = 10000000 },
            new PriceFilter { Name = "10Tr - 15Tr", MinPrice = 10000000, MaxPrice = 15000000 },
            new PriceFilter { Name = "15Tr - 20Tr", MinPrice = 15000000, MaxPrice = 20000000 },
            new PriceFilter { Name = "20Tr - 25Tr", MinPrice = 20000000, MaxPrice = 25000000 },
            new PriceFilter { Name = "25Tr - 30Tr", MinPrice = 25000000, MaxPrice = 30000000 },
            new PriceFilter { Name = "Trên 30Tr", MinPrice = 30000000, MaxPrice = int.MaxValue }
        };

        public List<RateFilter> _rateRanges = new List<RateFilter>
        {
            new RateFilter { Name = "4 sao trở lên", MinRate = 4 },
            new RateFilter { Name = "3 sao trở lên", MinRate = 3 },
            new RateFilter { Name = "2 sao trở lên", MinRate = 2 },
            new RateFilter { Name = "1 sao trở lên", MinRate = 1 }
        };

        protected override async Task OnInitializedAsync()
        {
            _loaded = false;
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);

            ApplyFiltersFromQuery(query);

            await LoadData(0, 12, new TableState());
            _loaded = true;
        }

        private void ApplyFiltersFromQuery(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query)
        {
            if (query.TryGetValue("search", out var searchValue))
            {
                _searchString = searchValue.ToString();
            }

            if (query.TryGetValue("brands", out var brandsValue))
            {
                var selectedBrands = brandsValue.ToString().Split(',');
                foreach (var brand in _brands)
                {
                    brand.IsSelected = selectedBrands.Contains(brand.Name);
                }
            }

            if (query.TryGetValue("descriptions", out var descriptionsValue))
            {
                var selectedDescriptions = descriptionsValue.ToString().Split(',');
                foreach (var description in _descriptions)
                {
                    description.IsSelected = selectedDescriptions.Contains(description.Name);
                }
            }

            if (query.TryGetValue("priceRange", out var priceRangeValue))
            {
                var selectedPriceRanges = priceRangeValue.ToString().Split(',');
                foreach (var price in _priceRanges)
                {
                    price.IsSelected = selectedPriceRanges.Contains(price.Name);
                }
            }

            if (query.TryGetValue("rateRange", out var rateRangeValue))
            {
                var selectedRateRanges = rateRangeValue.ToString().Split(',');
                foreach (var rate in _rateRanges)
                {
                    rate.IsSelected = selectedRateRanges.Contains(rate.Name);
                }
            }
        }

        private async Task LoadMoreProducts()
        {
            _currentPage++;
            await LoadData(_currentPage, 12, new TableState());
        }


        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            // Kiểm tra nếu chưa tải dữ liệu tất cả sản phẩm
            if (allProducts.Count == 0)
            {
                var selectedBrands = _brands.Where(b => b.IsSelected).Select(b => b.Name).ToList();
                var selectedDescriptions = _descriptions.Where(d => d.IsSelected).Select(d => d.Name).ToList();
                var selectedPriceRanges = _priceRanges.Where(p => p.IsSelected).ToList();
                var selectedRateRanges = _rateRanges.Where(r => r.IsSelected).ToList();

                var request = new GetAllPagedProductsRequest
                {
                    PageNumber = 1,  
                    PageSize = int.MaxValue, 
                    SearchString = _searchString,
                    BrandFilter = selectedBrands,
                    DescriptionFilter = selectedDescriptions,
                    PriceRangeFilter = selectedPriceRanges.Select(p => p.Name).ToList(),
                    RateRangeFilter = selectedRateRanges.Select(r => r.Name).ToList()
                };

                var response = await ProductManager.GetProductsAsync(request);

                if (response.Succeeded && response.Data != null)
                {
                    allProducts = ConvertToProductList(response.Data);
                    _totalItems = response.TotalCount;
                }
                else
                {
                    allProducts = new List<Product>();
                    _snackBar.Add("No products found.", Severity.Warning);
                }
            }

            // Áp dụng bộ lọc lên tất cả sản phẩm
            var filteredProducts = ApplyFilters(allProducts);

            var newPagedData = ConvertToPagedData(filteredProducts.Skip(pageNumber * pageSize).Take(pageSize).ToList());

            // Nếu đã có dữ liệu trước đó, thì kết hợp chúng lại
            if (_pagedData != null)
            {
                _pagedData = _pagedData.Concat(newPagedData).ToList();
            }
            else
            {
                _pagedData = newPagedData.ToList();
            }

            _currentPage = pageNumber;
        }



        private List<Product> ConvertToProductList(IEnumerable<GetAllPagedProductsResponse> responseData)
        {
            return responseData.Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CPU = p.CPU,
                Screen = p.Screen,
                Card = p.Card,
                Ram = p.Ram,
                Rom = p.Rom,
                Battery = p.Battery,
                Weight = p.Weight,
                Description = p.Description,
                Rate = p.Rate,
                Barcode = p.Barcode,
                Brand = new Brand { Name = p.Brand },
                ImageDataURL = p.ImageDataURL
            }).ToList();
        }

        private IEnumerable<GetAllPagedProductsResponse> ConvertToPagedData(List<Product> products)
        {
            return products.Select(p => new GetAllPagedProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CPU = p.CPU,
                Screen = p.Screen,
                Card = p.Card,
                Ram = p.Ram,
                Rom = p.Rom,
                Battery = p.Battery,
                Weight = p.Weight,
                Description = p.Description,
                Rate = p.Rate,
                Barcode = p.Barcode,
                Brand = p.Brand.Name, 
                ImageDataURL = p.ImageDataURL
            });
        }

        private void ToggleFilterPanel()
        {
            isFilterPanelVisible = !isFilterPanelVisible;
        }
        private void ToggleBrandSelection(BrandFilter brand)
        {
            foreach (var b in _brands)
            {
                b.IsSelected = false;
            }
            brand.IsSelected = true;
            ApplyFiltersAndRedirect();
        }

        

        private string SelectedPriceRange = "all";
        private int CustomPriceRangeStart;
        private int CustomPriceRangeEnd;
        private string SelectedRateRange = "all";

        // Filter the product data based on selected filters
        private void ApplyFiltersAndRedirect()
        {


            var queryParams = new List<string>();

            // Apply filters as usual
            AddQueryParameter(queryParams, "brands", _brands);
            AddQueryParameter(queryParams, "descriptions", _descriptions);
            AddQueryParameter(queryParams, "priceRange", _priceRanges);
            AddQueryParameter(queryParams, "rateRange", _rateRanges);

            var queryString = string.Join("&", queryParams);
            NavigationManager.NavigateTo($"/allproducts?{queryString}", forceLoad: true);
        }

     
        private void AddQueryParameter<T>(List<string> queryParams, string paramName, IEnumerable<T> filters)
        {
            var selectedItems = filters.Where(f => ((dynamic)f).IsSelected)
                                        .Select(f => ((dynamic)f).Name).ToList();

            if (selectedItems.Any())
            {
                queryParams.Add($"{paramName}={string.Join(",", selectedItems)}");
            }
        }

        private List<Product> ApplyFilters(List<Product> products)
        {
            products = FilterByBrand(products);
            products = FilterByDescription(products);
            products = FilterByPrice(products);
            products = FilterByRating(products);

            return products;
        }


        private List<Product> FilterByBrand(List<Product> products)
        {
            var selectedBrands = _brands.Where(b => b.IsSelected).Select(b => b.Name).ToList();

            if (!selectedBrands.Any())
            {
                return products;
            }

            return products.Where(p => selectedBrands.Contains(p.Brand.Name)).ToList();
        }

        private List<Product> FilterByDescription(List<Product> products)
        {
            var selectedDescriptions = _descriptions.Where(d => d.IsSelected).Select(d => d.Name).ToList();

            if (!selectedDescriptions.Any())
            {
                return products;
            }

            return products.Where(p => selectedDescriptions.Contains(p.Description)).ToList();
        }

        private List<Product> FilterByPrice(List<Product> products)
        {
            var selectedPriceRanges = _priceRanges.Where(p => p.IsSelected).ToList();
            if (!selectedPriceRanges.Any())
            {
                return products;
            }
            return products.Where(p => selectedPriceRanges.Any(r => p.Price >= r.MinPrice && p.Price <= r.MaxPrice)).ToList();
        }

        private List<Product> FilterByRating(List<Product> products)
        {
            var selectedRateRanges = _rateRanges.Where(r => r.IsSelected).ToList();
            if (!selectedRateRanges.Any())
            {
                return products;
            }
            return products.Where(p => selectedRateRanges.Any(r => p.Rate >= r.MinRate)).ToList();
        }

        private async Task InvokeModal(int id = 0)
        {
            var parameters = new DialogParameters();
            if (id != 0)
            {
                var product = _pagedData.FirstOrDefault(c => c.Id == id);
                if (product != null)
                {
                    parameters.Add(nameof(ViewProduct.Product), new GetProductByIdResponse
                    {
                        ImageDataURL = product.ImageDataURL,
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                        CPU = product.CPU,
                        Screen = product.Screen,
                        Card = product.Card,
                        Ram = product.Ram,
                        Rom = product.Rom,
                        Battery = product.Battery,
                        Weight = product.Weight,
                        Description = product.Description,
                        Rate = product.Rate,
                        Barcode = product.Barcode,
                    });
                }
            }

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                DisableBackdropClick = true

            };

            var dialog = _dialogService.Show<ViewProduct>("Thông tin sản phẩm", parameters, options);
            var result = await dialog.Result;
        }


        private string GetFilterPanelClass() =>
            isFilterPanelVisible ? "filter-panel-visible" : "filter-panel-hidden";

        private void NavigateToProductDetail(int productId)
        {
            NavigationManager.NavigateTo($"/product/{productId}");
        }

    private List<string> GetStars(decimal rate)
        {
            // Lấy phần nguyên của rate để xác định số sao đầy
            var fullStars = (int)Math.Floor(rate);

            // Kiểm tra xem có nửa sao không
            var halfStar = (rate - fullStars) >= 0.5m; // 'm' để chỉ định là kiểu decimal

            // Tính số sao trống còn lại
            var emptyStars = 5 - fullStars - (halfStar ? 1 : 0);

            // Tạo danh sách chứa các icon sao
            var stars = new List<string>();

            // Thêm icon sao đầy
            stars.AddRange(Enumerable.Repeat(Icons.Material.Filled.Star, fullStars));

            // Thêm icon nửa sao nếu có
            if (halfStar)
            {
                stars.Add(Icons.Material.Filled.StarHalf);
            }

            // Thêm icon sao trống
            stars.AddRange(Enumerable.Repeat(Icons.Material.Outlined.StarOutline, emptyStars));

            return stars;
        }
    }

}
