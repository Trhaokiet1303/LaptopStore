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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Components.Routing;

namespace LaptopStore.Client.Pages.Shop
{
    public partial class ListProducts
    {
        [Inject] private IProductManager ProductManager { get; set; }

        [CascadingParameter] private HubConnection HubConnection { get; set; }

        private IEnumerable<GetAllPagedProductsResponse> _pagedData;
        private MudTable<GetAllPagedProductsResponse> _table;

        private bool isFilterPanelVisible = false;

        private int _totalItems;
        private int _currentPage;
        private string _searchString = "";
        private bool _loaded;

        protected override async Task OnParametersSetAsync()
        {
            await UpdateSearchStringAndReload();
        }

        protected override async Task OnInitializedAsync()
        {
            NavigationManager.LocationChanged += OnLocationChanged;
            await UpdateSearchStringAndReload();
        }

        private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            await UpdateSearchStringAndReload();
        }

        private async Task UpdateSearchStringAndReload()
        {
            // Extract the updated search query from the URL
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("search", out var searchQuery))
            {
                var newSearchString = searchQuery.ToString();
                if (_searchString != newSearchString)
                {
                    _searchString = newSearchString;
                    await ReloadDataBasedOnSearch();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_searchString))
                {
                    _searchString = string.Empty;
                }
                await ReloadDataBasedOnSearch();
            }
        }

        private async Task ReloadDataBasedOnSearch()
        {
            _loaded = false; // Start loading
            if (string.IsNullOrEmpty(_searchString))
            {
                await LoadAllProducts(); 
            }
            else
            {
                await LoadFilteredProducts(_searchString);
            }
            _loaded = true; 
            StateHasChanged(); 
        }
        private async Task LoadAllProducts()
        {
            await LoadData(0, 10, new TableState());
        }

        private async Task LoadFilteredProducts(string searchString)
        {
            await LoadData(0, 10, new TableState());
        }
        private async Task ReloadPageData()
        {
            await LoadData(0, 10, new TableState());
            StateHasChanged();
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
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetAllPagedProductsRequest
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchString = _searchString,
                Orderby = orderings
            };

            var response = await ProductManager.GetProductsAsync(request);
            if (response.Succeeded && response.Data != null && response.Data.Any())
            {
                _totalItems = response.TotalCount;
                _pagedData = response.Data;
            }
            else
            {
                _pagedData = new List<GetAllPagedProductsResponse>();
                _totalItems = 0;
                _snackBar.Add("Không tìm thấy sản phẩm nào.", Severity.Info);
            }
        }
        private void ToggleFilterPanel()
        {
            isFilterPanelVisible = !isFilterPanelVisible;
        }
        private void ToggleBrandSelection(BrandFilter brand)
        {
            brand.IsSelected = !brand.IsSelected; // Thay đổi trạng thái chọn
            ApplyFilters(); // Áp dụng bộ lọc
        }


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
            ApplyBrandFilter();
            if (_pagedData == null) return;

            var selectedBrands = _brands.Where(b => b.IsSelected).Select(b => b.Name).ToList();
            var selectedDescriptions = _descriptions.Where(d => d.IsSelected).Select(d => d.Name).ToList();
        }
        private void ApplyBrandFilter()
        {
            if (_pagedData == null) return;

            // Lọc sản phẩm theo hãng
            var selectedBrands = _brands.Where(b => b.IsSelected).Select(b => b.Name).ToList();
            if (selectedBrands.Any())
            {
                _pagedData = _pagedData.Where(p => selectedBrands.Contains(p.Brand)).ToList();
            }
            else
            {
                // Nếu không chọn hãng nào, hiển thị tất cả sản phẩm
                _table.ReloadServerData();
            }
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
        private string GetFilterPanelClass() =>
            isFilterPanelVisible ? "filter-panel-visible" : "filter-panel-hidden";

        private void NavigateToProductDetail(int productId)
        {
            NavigationManager.NavigateTo($"/product/{productId}");
        }
    }
}