using LaptopStore.Application.Specifications.Base;
using LaptopStore.Domain.Entities.Catalog;
using System.Collections.Generic;
using System.Diagnostics;

namespace LaptopStore.Application.Specifications.Catalog
{
    public class ProductFilterSpecification : HeroSpecification<Product>
    {
        public ProductFilterSpecification(string searchString)
        {
            Includes.Add(a => a.Brand);
            if (!string.IsNullOrEmpty(searchString))
            {
                Criteria = p => p.Barcode != null && (p.Name.Contains(searchString) ||
                p.Description.Contains(searchString) || p.Barcode.Contains(searchString) ||
                p.Brand.Name.Contains(searchString) || p.CPU.Contains(searchString) ||
                p.Screen.Contains(searchString) || p.Card.Contains(searchString) ||
                p.Ram.Contains(searchString) || p.Rom.Contains(searchString) ||
                p.Battery.Contains(searchString) || p.Weight.Contains(searchString) ||
                p.ProductLine.Contains(searchString)
                );
            }
            else
            {
                Criteria = p => p.Barcode != null;
            }
        }
    }
    public class BrandFilter
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string LogoPath { get; set; }
    }

    public class DescriptionFilter
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string DescriptionPath { get; set; }
    }

    public class PriceFilter
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
    }

    public class RateFilter
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public decimal MinRate { get; set; }
    }
}