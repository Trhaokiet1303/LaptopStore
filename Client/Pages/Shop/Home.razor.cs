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
using System.Timers; 
using LaptopStore.Application.Features.Products.Commands.AddEdit;
using LaptopStore.Client.Infrastructure.Managers.Catalog.Product;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using LaptopStore.Client.Pages.Admin.Products;
using LaptopStore.Application.Features.Products.Queries.GetProductById;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class Home : IDisposable // Implement IDisposable to manage resources
    {
        [Inject] private IProductManager ProductManager { get; set; }
        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private IEnumerable<GetAllPagedProductsResponse> _pagedData;
        private MudTable<GetAllPagedProductsResponse> _table;
        [Parameter] public AddEditProductCommand ProductIMG { get; set; } = new();

        private bool isFilterPanelVisible = false;
        private int _totalItems;
        private int _currentPage;
        private string _searchString = "";
        private bool _loaded;
        private IEnumerable<GetAllPagedProductsResponse> _featuredProducts;
        private IEnumerable<GetAllPagedProductsResponse> _RatedProducts;
        private int currentIndex = 0; 
        private int productsPerPage = 5; // Số lượng sản phẩm trên mỗi trang
        protected override async Task OnInitializedAsync()
        {
            _loaded = false;
            await LoadData(0, 10, new TableState()); // Example page size of 10
            ApplyFilters(); // Call ApplyFilters to initialize filtered lists
            _loaded = true;

            // Initialize and start the timer for automatic rotation
            bannerTimer = new Timer(3000);
            bannerTimer.Elapsed += (s, e) => InvokeAsync(ShowNextImage);
            bannerTimer.Start();
        }

        private void ToggleFilterPanel()
        {
            isFilterPanelVisible = !isFilterPanelVisible;
        }

        private async Task<TableData<GetAllPagedProductsResponse>> ServerReload(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);
            return new TableData<GetAllPagedProductsResponse> { TotalItems = _totalItems, Items = _pagedData };
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            var request = new GetAllPagedProductsRequest
            {
                PageNumber = pageNumber + 1, // Convert to 1-based index if necessary
                PageSize = pageSize,
                SearchString = _searchString
            };
            var response = await ProductManager.GetProductsAsync(request);
            if (response.Succeeded)
            {
                _pagedData = response.Data;
                _totalItems = response.TotalCount;
            }
        }

        private async Task LoadImageAsync()
        {
            var data = await ProductManager.GetProductImageAsync(ProductIMG.Id);
            if (data.Succeeded)
            {
                var imageData = data.Data;
                if (!string.IsNullOrEmpty(imageData))
                {
                    ProductIMG.ImageDataURL = imageData;
                }
            }
        }

      

        // Define filter classes for Brand and Description
        private class BrandFilter
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
            public string LogoPath { get; set; }
        }

        private class DescriptionFilter
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
            public string DescriptionPath { get; set; }
        }

        // Initialize filter options for brands and descriptions
        private List<BrandFilter> _brands = new List<BrandFilter>
{
            new BrandFilter { Name = "Apple", LogoPath = "/images/brand/mac-icon.png" },
            new BrandFilter { Name = "Lenovo", LogoPath = "/images/brand/lenovo-icon.png" },
            new BrandFilter { Name = "Asus", LogoPath = "/images/brand/asus-icon.png" },
            new BrandFilter { Name = "MSI", LogoPath = "/images/brand/msi-icon.png" },
            new BrandFilter { Name = "HP", LogoPath = "/images/brand/hp-icon.png" },
            new BrandFilter { Name = "Acer", LogoPath = "/images/brand/acer-icon.png" },
            new BrandFilter { Name = "Samsung", LogoPath = "/images/brand/samsung-icon.png" },
            new BrandFilter { Name = "Dell", LogoPath = "/images/brand/dell-icon.png" }
        };

        private List<DescriptionFilter> _descriptions = new List<DescriptionFilter>
        {
            new DescriptionFilter { Name = "Gaming",DescriptionPath="/images/description/Gaming-Lap.png" },
            new DescriptionFilter { Name = "Office",DescriptionPath="/images/description/Office-Lap.png" },
            new DescriptionFilter { Name = "Ultrabook",DescriptionPath="/images/description/Book-Lap.png" },
            new DescriptionFilter { Name = "AI",DescriptionPath="/images/description/AI-Lap.png" },
            new DescriptionFilter { Name = "Graphic",DescriptionPath="/images/description/Graphic-Lap.png" },
        };

        private string SelectedPriceRange = "all";
        private int CustomPriceRangeStart;
        private int CustomPriceRangeEnd;
        private string SelectedRateRange = "all";

        // Filter the product data based on selected filters
        private void ApplyFilters()
        {
            if (_pagedData == null) return;

            var selectedBrands = _brands.Where(b => b.IsSelected).Select(b => b.Name).ToList();
            var selectedDescriptions = _descriptions.Where(d => d.IsSelected).Select(d => d.Name).ToList();

            _featuredProducts = _pagedData.Where(p =>
                p.Featured == true && // Chỉ lấy sản phẩm có Feature = true
                (selectedBrands.Count == 0 || selectedBrands.Contains(p.Brand)) &&
                (selectedDescriptions.Count == 0 || selectedDescriptions.Contains(p.Description)) &&
                (SelectedRateRange == "all" ||
                 (SelectedRateRange == "4andAbove" && p.Rate >= 4) ||
                 (SelectedRateRange == "3andAbove" && p.Rate >= 3) ||
                 (SelectedRateRange == "2andAbove" && p.Rate >= 2) ||
                 (SelectedRateRange == "1andAbove" && p.Rate >= 1))
            ).ToList();

            // Lọc _RatedProducts chỉ lấy các sản phẩm có Rate >= 4
            _RatedProducts = _pagedData.Where(p => p.Rate >= 4).ToList();
        }

        // List of banner images
        private List<string> bannerImages = new List<string>
        {
            "/images/banner/1.png",
            "/images/banner/2.png",
            "/images/banner/3.png",
            "/images/banner/4.png",
            "/images/banner/5.png"
        };

        private int currentImageIndex = 0;
        private int secondImageIndex => (currentImageIndex + 1) % bannerImages.Count; // Lấy chỉ số ảnh thứ hai
        private Timer bannerTimer;

        private void ShowNextImage()
        {
            currentImageIndex = (currentImageIndex + 1) % bannerImages.Count;
            StateHasChanged();
        }

        private void ShowPreviousImage()
        {
            currentImageIndex = (currentImageIndex - 1 + bannerImages.Count) % bannerImages.Count;
            StateHasChanged();
        }

        public void Dispose()
        {
            bannerTimer?.Dispose();
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
        private string GetProductGridContainerClass() =>
        isFilterPanelVisible ? "product-grid-container with-filter" : "product-grid-container full-screen";

        private string GetFilterPanelClass() =>
            isFilterPanelVisible ? "filter-panel-visible" : "filter-panel-hidden";
        private void NavigateToProductDetail(int productId)
        {
            NavigationManager.NavigateTo($"/product/{productId}");
        }

        private int featuredProductCurrentIndex = 0; // Index hiện tại của danh sách Featured Products
        private int featuredProductsPerPage = 5;    // Số lượng sản phẩm hiển thị mỗi lần

        // Hàm chuyển qua danh sách sản phẩm tiếp theo
        private void ShowNextFeaturedProducts()
        {
            if (_featuredProducts != null && _featuredProducts.Any())
            {
                featuredProductCurrentIndex += featuredProductsPerPage;
                if (featuredProductCurrentIndex >= _featuredProducts.Count())
                {
                    featuredProductCurrentIndex = 0; // Quay lại từ đầu nếu hết sản phẩm
                }
                StateHasChanged();
            }
        }

        // Hàm chuyển về danh sách sản phẩm trước đó
        private void ShowPreviousFeaturedProducts()
        {
            if (_featuredProducts != null && _featuredProducts.Any())
            {
                featuredProductCurrentIndex -= featuredProductsPerPage;
                if (featuredProductCurrentIndex < 0)
                {
                    featuredProductCurrentIndex = Math.Max(0, _featuredProducts.Count() - featuredProductsPerPage);
                }
                StateHasChanged();
            }
        }

        // Lấy danh sách sản phẩm hiện tại để hiển thị
        private IEnumerable<GetAllPagedProductsResponse> GetCurrentFeaturedProducts()
        {
            return _featuredProducts
                ?.Skip(featuredProductCurrentIndex)
                .Take(featuredProductsPerPage)
                ?? Enumerable.Empty<GetAllPagedProductsResponse>();
        }

    }
}