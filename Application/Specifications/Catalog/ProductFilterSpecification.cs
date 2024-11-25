using LaptopStore.Application.Specifications.Base;
using LaptopStore.Domain.Entities.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LaptopStore.Application.Specifications.Catalog
{
    public class ProductFilterSpecification : HeroSpecification<Product>
    {
        public ProductFilterSpecification(string searchString, List<string> brands = null, List<string> descriptions = null, string priceRange = "all", string rateRange = "all")
        {
            Includes.Add(a => a.Brand);

            Criteria = p => p.Barcode != null;

            if (!string.IsNullOrEmpty(searchString))
            {
                Criteria = Criteria.And(p =>
                    p.Name.Contains(searchString) ||
                    p.Description.Contains(searchString) ||
                    p.Barcode.Contains(searchString) ||
                    p.Brand.Name.Contains(searchString) ||
                    p.CPU.Contains(searchString) ||
                    p.Screen.Contains(searchString) ||
                    p.Card.Contains(searchString));
            }

            if (brands != null && brands.Any())
            {
                Criteria = Criteria.And(p => brands.Contains(p.Brand.Name));
            }

            if (descriptions != null && descriptions.Any())
            {
                Criteria = Criteria.And(p => descriptions.Contains(p.Description));
            }

            if (priceRange != "all")
            {
                Criteria = priceRange switch
                {
                    "under10" => Criteria.And(p => p.Price <= 10000000),
                    "10to15" => Criteria.And(p => p.Price > 10000000 && p.Price <= 15000000),
                    _ => Criteria // Thêm các trường hợp khác
                };
            }

            if (rateRange != "all")
            {
                Criteria = rateRange switch
                {
                    "4andAbove" => Criteria.And(p => p.Rate >= 4),
                    "3andAbove" => Criteria.And(p => p.Rate >= 3),
                    _ => Criteria // Thêm các trường hợp khác
                };
            }
        }

    }


    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

}