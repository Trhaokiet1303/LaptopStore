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
using Microsoft.AspNetCore.WebUtilities;
using LaptopStore.Application.Specifications.Catalog;

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
            
            await LoadData(0, int.MaxValue, new TableState());
            ApplyFilters(); 
            _loaded = true;

            // Initialize and start the timer for automatic rotation
            bannerTimer = new Timer(3000);
            bannerTimer.Elapsed += (s, e) => InvokeAsync(ShowNextImage);
            bannerTimer.Start();
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

        // Initialize filter options for brands
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


        private string SelectedRateRange = "all";

        // Filter the product data based on selected filters
        private void ApplyFilters()
        {
            if (_pagedData == null) return;

            _featuredProducts = _pagedData.Where(p => p.Featured == true).ToList();
            _RatedProducts = _pagedData.Where(p => p.Rate >= 4.2m).ToList();
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
        private int secondImageIndex => (currentImageIndex + 1) % bannerImages.Count; 
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
        private void NavigateToAllProducts(string brand)
        {
            // Chuyển hướng tới trang /allproducts với tham số lọc thương hiệu
            NavigationManager.NavigateTo($"/allproducts?brands={Uri.EscapeDataString(brand)}");
        }

        private void NavigateToProductDetail(int productId)
        {
            NavigationManager.NavigateTo($"/product/{productId}");
        }

        private int featuredProductCurrentIndex = 0; // Index hiện tại của danh sách Featured Products
        private int featuredProductsPerPage = 8;    // Số lượng sản phẩm hiển thị mỗi lần

        // Hàm chuyển qua danh sách sản phẩm tiếp theo
        private void ShowNextFeaturedProducts()
        {
            if (_featuredProducts != null && _featuredProducts.Any())
            {
                // Tăng chỉ số lên 1 để chuyển sang sản phẩm tiếp theo
                featuredProductCurrentIndex++;

                // Kiểm tra nếu đã đến cuối danh sách, quay lại đầu
                if (featuredProductCurrentIndex >= _featuredProducts.Count())
                {
                    featuredProductCurrentIndex = 0;
                }

                StateHasChanged();
            }
        }



        // Hàm chuyển về danh sách sản phẩm trước đó
        private void ShowPreviousFeaturedProducts()
        {
            if (_featuredProducts != null && _featuredProducts.Any())
            {
                // Giảm chỉ số xuống 1 để quay lại sản phẩm trước đó
                featuredProductCurrentIndex--;

                // Kiểm tra nếu đã đến đầu danh sách, quay lại cuối
                if (featuredProductCurrentIndex < 0)
                {
                    featuredProductCurrentIndex = _featuredProducts.Count() - 1;
                }

                StateHasChanged();
            }
        }



        // Lấy danh sách sản phẩm hiện tại để hiển thị
        private IEnumerable<GetAllPagedProductsResponse> GetCurrentFeaturedProducts()
        {
            return _featuredProducts
                ?.Skip(featuredProductCurrentIndex)
                .Take(featuredProductsPerPage)  // Hiển thị 8 sản phẩm mỗi lần
                ?? Enumerable.Empty<GetAllPagedProductsResponse>();
        }




        /*thong tin*/
        private bool IsExpanded { get; set; } = false;

        private string ButtonLabel => IsExpanded ? "Ẩn bớt nội dung" : "Xem thêm nội dung";
        private string ContentClass => IsExpanded ? "" : "content-hidden";
        private string AdditionalContentClass => IsExpanded ? "" : "content-hidden";

        private void ToggleContent()
        {
            IsExpanded = !IsExpanded;
        }

    }
}