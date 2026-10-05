using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.DTOs;

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
            var filter = new PaginationFilter(1, 50);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSize10_SetsTo10()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 10);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageSize5_SetsTo5()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 5);

            // Assert
            Assert.Equal(5, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_Constructor_WithPageNumber1_PageSize1()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 1);

            // Assert
            Assert.Equal(1, filter.PageNumber);
            Assert.Equal(1, filter.PageSize);
        }
    }

    public class PaginationHelperTests
    {
        [Fact]
        public void CreatePagedReponse_WithValidData_ReturnsCorrectPagination()
        {
            // Arrange
            var data = new List<string> { "a", "b", "c" };
            var filter = new PaginationFilter(1, 3);
            int totalRecords = 9;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data, result.Data);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(3, result.PageSize);
            Assert.Equal(9, result.TotalRecords);
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_WithTotalRecordsNotDivisible_CeilsTotalPages()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3 };
            var filter = new PaginationFilter(1, 3);
            int totalRecords = 10;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(4, result.TotalPages); // ceil(10/3) = 4
        }

        [Fact]
        public void CreatePagedReponse_WithEmptyData_ReturnsZeroTotalPages()
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
        public void CreatePagedReponse_WithExactDivision_ReturnsCorrectTotalPages()
        {
            // Arrange
            var data = new List<int> { 1, 2 };
            var filter = new PaginationFilter(1, 5);
            int totalRecords = 10;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_WithSingleRecord_ReturnsOnePage()
        {
            // Arrange
            var data = new List<string> { "only" };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 1;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.TotalRecords);
        }
    }

    public class QueryFilterTests
    {
        [Fact]
        public void QueryFilter_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.NotNull(filter);
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
        private class TestItem
        {
            [Searchable]
            public string Name { get; set; } = string.Empty;

            [Sortable]
            public int Value { get; set; }

            public string NonSearchable { get; set; } = string.Empty;
        }

        [Fact]
        public void SearchObjects_WithNullSearchBy_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "Banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, null, "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_WithNullSearch_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "Banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", null, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_WithEmptySearchBy_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "Banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_WithValidSearchableProperty_FiltersCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "Banana" },
                new TestItem { Name = "Apricot" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "Ap", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Name == "Apple");
            Assert.Contains(result, x => x.Name == "Apricot");
        }

        [Fact]
        public void SearchObjects_WithNonSearchableProperty_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { NonSearchable = "Apple" },
                new TestItem { NonSearchable = "Banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "NonSearchable", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_WithNonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "Banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "NonExistentProp", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SortObjects_WithNullSortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 3 },
                new TestItem { Value = 1 },
                new TestItem { Value = 2 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, null, true).ToList();

            // Assert
            Assert.Equal(3, result[0].Value);
            Assert.Equal(1, result[1].Value);
            Assert.Equal(2, result[2].Value);
        }

        [Fact]
        public void SortObjects_WithEmptySortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 3 },
                new TestItem { Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "", true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SortObjects_Ascending_SortsCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 3 },
                new TestItem { Value = 1 },
                new TestItem { Value = 2 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "Value", true).ToList();

            // Assert
            Assert.Equal(1, result[0].Value);
            Assert.Equal(2, result[1].Value);
            Assert.Equal(3, result[2].Value);
        }

        [Fact]
        public void SortObjects_Descending_SortsCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 1 },
                new TestItem { Value = 3 },
                new TestItem { Value = 2 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "Value", false).ToList();

            // Assert
            Assert.Equal(3, result[0].Value);
            Assert.Equal(2, result[1].Value);
            Assert.Equal(1, result[2].Value);
        }

        [Fact]
        public void SortObjects_WithNonSortableProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { NonSearchable = "B" },
                new TestItem { NonSearchable = "A" }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "NonSearchable", true).ToList();

            // Assert
            Assert.Equal("B", result[0].NonSearchable);
            Assert.Equal("A", result[1].NonSearchable);
        }

        [Fact]
        public void SortObjects_WithNonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 3 },
                new TestItem { Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "NonExistentProp", true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SearchObjects_CaseInsensitive_FindsMatch()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "APPLE" },
                new TestItem { Name = "banana" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("APPLE", result[0].Name);
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
    }
}
