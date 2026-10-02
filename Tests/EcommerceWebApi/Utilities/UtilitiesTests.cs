using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.DTOs;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Tests.Utilities
{
    public class PaginationFilterTests
    {
        [Fact]
        public void PaginationFilter_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var filter = new PaginationFilter();

            // Assert
            Assert.Equal(1, filter.PageNumber);
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithValidValues_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(2, 5);

            // Assert
            Assert.Equal(2, filter.PageNumber);
            Assert.Equal(5, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageNumberLessThanOne_SetsToOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(0, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithNegativePageNumber_SetsToOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(-5, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSizeGreaterThanTen_SetsToTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 20);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSizeEqualToTen_SetsToTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 10);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSizeLessThanTen_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 5);

            // Assert
            Assert.Equal(5, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageNumberOne_SetsToOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithLargePageNumber_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(100, 5);

            // Assert
            Assert.Equal(100, filter.PageNumber);
        }
    }

    public class PaginationHelperTests
    {
        [Fact]
        public void CreatePagedReponse_WithValidData_ReturnsCorrectPagination()
        {
            // Arrange
            var data = new List<string> { "item1", "item2", "item3" };
            var filter = new PaginationFilter(1, 3);
            int totalRecords = 10;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data, result.Data);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(3, result.PageSize);
            Assert.Equal(10, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedReponse_TotalPages_CalculatedCorrectly()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 25;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(3, result.TotalPages); // ceil(25/10) = 3
        }

        [Fact]
        public void CreatePagedReponse_TotalPages_ExactDivision()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 20;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(2, result.TotalPages); // 20/10 = 2
        }

        [Fact]
        public void CreatePagedReponse_TotalPages_WithZeroRecords()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 0;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_TotalPages_WithOneRecord()
        {
            // Arrange
            var data = new List<string> { "item" };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 1;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecords_SetCorrectly()
        {
            // Arrange
            var data = new List<int> { 1, 2 };
            var filter = new PaginationFilter(1, 5);
            int totalRecords = 50;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(50, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedReponse_WithPageSize5_TotalPages_CalculatedCorrectly()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 5);
            int totalRecords = 13;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(3, result.TotalPages); // ceil(13/5) = 3
        }
    }

    public class QueryFilterTests
    {
        [Fact]
        public void QueryFilter_DefaultConstructor_SetsNullValues()
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
            filter.Search = "laptop";

            // Assert
            Assert.Equal("laptop", filter.Search);
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

    public class QueryHelperTests
    {
        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Title = "Laptop", Brand = "Dell", Category = "Electronics", Price = 999f, Rating = 4.5f, Quantity = 10 },
                new Product { Id = 2, Title = "Phone", Brand = "Apple", Category = "Electronics", Price = 799f, Rating = 4.8f, Quantity = 20 },
                new Product { Id = 3, Title = "Shirt", Brand = "Nike", Category = "Clothing", Price = 29f, Rating = 3.5f, Quantity = 100 },
                new Product { Id = 4, Title = "Laptop Pro", Brand = "Apple", Category = "Electronics", Price = 1299f, Rating = 4.9f, Quantity = 5 }
            };
        }

        [Fact]
        public void SearchObjects_WithNullSearchBy_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, null, "laptop", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithNullSearch_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", null, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithEmptySearchBy_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "", "laptop", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithEmptySearch_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithValidSearchableProperty_ReturnsFilteredResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "Laptop", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Contains("Laptop", p.Title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SearchObjects_WithBrandSearch_ReturnsFilteredResults()
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
        public void SearchObjects_WithNonSearchableProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Price is not [Searchable], so it should return all
            var result = QueryHelper.SearchObjects(products, "Price", "999", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SearchObjects_WithNonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "NonExistentProperty", "value", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SearchObjects_CaseInsensitive_ReturnsCorrectResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "LAPTOP", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SortObjects_WithNullSortBy_ReturnsOriginalOrder()
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
        public void SortObjects_WithEmptySortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SortObjects_ByPrice_Ascending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(29f, result[0].Price);
            Assert.Equal(1299f, result[3].Price);
        }

        [Fact]
        public void SortObjects_ByPrice_Descending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", false).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(1299f, result[0].Price);
            Assert.Equal(29f, result[3].Price);
        }

        [Fact]
        public void SortObjects_ByRating_Ascending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Rating", true).ToList();

            // Assert
            Assert.Equal(3.5f, result[0].Rating);
        }

        [Fact]
        public void SortObjects_ByRating_Descending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Rating", false).ToList();

            // Assert
            Assert.Equal(4.9f, result[0].Rating);
        }

        [Fact]
        public void SortObjects_WithNonSortableProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Title is [Searchable] but not [Sortable]
            var result = QueryHelper.SortObjects(products, "Title", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(1, result[0].Id); // original order preserved
        }

        [Fact]
        public void SortObjects_WithNonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "NonExistent", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void SortObjects_ByQuantity_Ascending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Quantity", true).ToList();

            // Assert
            Assert.Equal(5, result[0].Quantity);
            Assert.Equal(100, result[3].Quantity);
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
        public void Product_Title_HasSearchableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Title");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SearchableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }

        [Fact]
        public void Product_Price_HasSortableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Price");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SortableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }

        [Fact]
        public void Product_Brand_HasSearchableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Brand");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SearchableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }

        [Fact]
        public void Product_Rating_HasSortableAttribute()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty("Rating");

            // Act
            var hasAttr = propertyInfo?.GetCustomAttributes(typeof(SortableAttribute), false).Any();

            // Assert
            Assert.True(hasAttr);
        }
    }
}
