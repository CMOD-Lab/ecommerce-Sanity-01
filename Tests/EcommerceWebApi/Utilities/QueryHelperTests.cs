using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Tests.Utilities
{
    public class QueryHelperTests
    {
        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Title = "Apple iPhone", Brand = "Apple", Category = "Electronics", Price = 999f, Rating = 4.5f, Quantity = 10 },
                new Product { Id = 2, Title = "Samsung Galaxy", Brand = "Samsung", Category = "Electronics", Price = 799f, Rating = 4.2f, Quantity = 20 },
                new Product { Id = 3, Title = "Dell Laptop", Brand = "Dell", Category = "Computers", Price = 1299f, Rating = 4.0f, Quantity = 5 },
                new Product { Id = 4, Title = "Apple MacBook", Brand = "Apple", Category = "Computers", Price = 1999f, Rating = 4.8f, Quantity = 8 },
            };
        }

        // SearchObjects Tests
        [Fact]
        public void SearchObjects_NullSearchBy_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, null, "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_EmptySearchBy_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_NullSearch_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", null, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_EmptySearch_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_ValidSearchByTitle_ReturnsMatchingProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "Apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Contains("Apple", p.Title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SearchObjects_ValidSearchByBrand_ReturnsMatchingProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Brand", "Apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("Apple", p.Brand));
        }

        [Fact]
        public void SearchObjects_NonSearchableProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Price is not marked as [Searchable]
            var result = QueryHelper.SearchObjects(products, "Price", "999", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SearchObjects_NonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "NonExistentProperty", "value", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SearchObjects_CaseInsensitiveSearch_ReturnsMatchingProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Brand", "apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SearchObjects_NoMatchingResults_ReturnsEmptyList()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "NonExistentProduct", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Empty(result);
        }

        // SortObjects Tests
        [Fact]
        public void SortObjects_NullSortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, null, true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void SortObjects_EmptySortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SortObjects_SortByPriceAscending_ReturnsSortedProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.True(result[0].Price <= result[1].Price);
            Assert.True(result[1].Price <= result[2].Price);
            Assert.True(result[2].Price <= result[3].Price);
        }

        [Fact]
        public void SortObjects_SortByPriceDescending_ReturnsSortedProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", false).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.True(result[0].Price >= result[1].Price);
            Assert.True(result[1].Price >= result[2].Price);
        }

        [Fact]
        public void SortObjects_SortByRatingAscending_ReturnsSortedProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Rating", true).ToList();

            // Assert
            Assert.True(result[0].Rating <= result[1].Rating);
        }

        [Fact]
        public void SortObjects_NonSortableProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Title is not marked as [Sortable]
            var result = QueryHelper.SortObjects(products, "Title", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(1, result[0].Id); // Original order preserved
        }

        [Fact]
        public void SortObjects_NonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "NonExistentProperty", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SortObjects_SortByQuantityDescending_ReturnsSortedProducts()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Quantity", false).ToList();

            // Assert
            Assert.True(result[0].Quantity >= result[1].Quantity);
        }
    }

    public class QueryFilterTests
    {
        [Fact]
        public void QueryFilter_DefaultConstructor_SetsNullDefaults()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.SearchBy);
            Assert.Null(filter.Search);
            Assert.Null(filter.SortBy);
            Assert.False(filter.IsSortAscending);
        }

        [Fact]
        public void QueryFilter_SetSearchBy_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.SearchBy = "Title";

            // Assert
            Assert.Equal("Title", filter.SearchBy);
        }

        [Fact]
        public void QueryFilter_SetSearch_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.Search = "Apple";

            // Assert
            Assert.Equal("Apple", filter.Search);
        }

        [Fact]
        public void QueryFilter_SetSortBy_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.SortBy = "Price";

            // Assert
            Assert.Equal("Price", filter.SortBy);
        }

        [Fact]
        public void QueryFilter_SetIsSortAscending_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.IsSortAscending = true;

            // Assert
            Assert.True(filter.IsSortAscending);
        }
    }

    public class AttributeTests
    {
        [Fact]
        public void SearchableAttribute_CanBeAppliedToProperty()
        {
            // Arrange & Act
            var attr = new SearchableAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void SortableAttribute_CanBeAppliedToProperty()
        {
            // Arrange & Act
            var attr = new SortableAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void UpdatableAttribute_CanBeAppliedToProperty()
        {
            // Arrange & Act
            var attr = new UpdatableAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void Product_TitleProperty_HasSearchableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Title");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SearchableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }

        [Fact]
        public void Product_PriceProperty_HasSortableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Price");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SortableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }

        [Fact]
        public void Product_BrandProperty_HasSearchableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Brand");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SearchableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }
    }
}
