using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Utilities.Tests
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
        public void PaginationFilter_Constructor_WithPageNumberLessThan1_SetsTo1()
        {
            // Arrange & Act
            var filter = new PaginationFilter(0, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithNegativePageNumber_SetsTo1()
        {
            // Arrange & Act
            var filter = new PaginationFilter(-5, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSizeGreaterThan10_SetsTo10()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 20);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSizeExactly10_Keeps10()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 10);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 5)]
        [InlineData(3, 10)]
        public void PaginationFilter_Constructor_WithValidPageSizes_SetsCorrectly(int pageNumber, int pageSize)
        {
            // Arrange & Act
            var filter = new PaginationFilter(pageNumber, pageSize);

            // Assert
            Assert.Equal(pageNumber, filter.PageNumber);
            Assert.Equal(pageSize, filter.PageSize);
        }
    }

    public class PaginationHelperTests
    {
        [Fact]
        public void CreatePagedResponse_WithValidData_ReturnsCorrectPagination()
        {
            // Arrange
            var data = new List<string> { "a", "b", "c" };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 30;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data, result.Data);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(30, result.TotalRecords);
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void CreatePagedResponse_WithExactDivision_ReturnsCorrectTotalPages()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var filter = new PaginationFilter(1, 5);
            int totalRecords = 25;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(5, result.TotalPages);
        }

        [Fact]
        public void CreatePagedResponse_WithRemainder_RoundsUpTotalPages()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3 };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 21;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void CreatePagedResponse_WithZeroRecords_ReturnsZeroTotalPages()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 0;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(0, result.TotalPages);
            Assert.Equal(0, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedResponse_WithSingleRecord_ReturnsOneTotalPage()
        {
            // Arrange
            var data = new List<string> { "single" };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 1;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.TotalRecords);
        }
    }

    public class QueryHelperTests
    {
        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, Title = "Apple iPhone", Brand = "Apple", Category = "Electronics", Price = 999f, Rating = 4.5f, Quantity = 10 },
                new Product { Id = 2, Title = "Samsung Galaxy", Brand = "Samsung", Category = "Electronics", Price = 799f, Rating = 4.2f, Quantity = 20 },
                new Product { Id = 3, Title = "Nike Shoes", Brand = "Nike", Category = "Footwear", Price = 120f, Rating = 4.0f, Quantity = 50 },
                new Product { Id = 4, Title = "Adidas Sneakers", Brand = "Adidas", Category = "Footwear", Price = 100f, Rating = 3.8f, Quantity = 30 },
            };
        }

        [Fact]
        public void SearchObjects_WithNullSearchBy_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, null, "Apple", StringComparison.OrdinalIgnoreCase);

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
            var result = QueryHelper.SearchObjects(products, "", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithValidSearchableProperty_ReturnsFilteredResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "Apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Apple iPhone", result[0].Title);
        }

        [Fact]
        public void SearchObjects_WithBrandSearchableProperty_ReturnsFilteredResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Brand", "Nike", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Nike", result[0].Brand);
        }

        [Fact]
        public void SearchObjects_WithNonSearchableProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Price is not [Searchable], so it should return all
            var result = QueryHelper.SearchObjects(products, "Price", "999", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_WithNonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "NonExistentProperty", "value", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public void SearchObjects_CaseInsensitive_FindsMatch()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SearchObjects(products, "Title", "apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Single(result);
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
        public void SortObjects_ByPriceAscending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(100f, result[0].Price);
            Assert.Equal(999f, result[3].Price);
        }

        [Fact]
        public void SortObjects_ByPriceDescending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Price", false).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(999f, result[0].Price);
            Assert.Equal(100f, result[3].Price);
        }

        [Fact]
        public void SortObjects_ByRatingAscending_ReturnsSortedResults()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "Rating", true).ToList();

            // Assert
            Assert.Equal(3.8f, result[0].Rating);
            Assert.Equal(4.5f, result[3].Rating);
        }

        [Fact]
        public void SortObjects_WithNonSortableProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            // Title is not [Sortable], so it should return original order
            var result = QueryHelper.SortObjects(products, "Title", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void SortObjects_WithNonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var products = GetSampleProducts();

            // Act
            var result = QueryHelper.SortObjects(products, "NonExistentProperty", true).ToList();

            // Assert
            Assert.Equal(4, result.Count);
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
}
