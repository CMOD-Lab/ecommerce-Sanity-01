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
        public void PaginationFilter_DefaultConstructor_PageNumberIsOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter();

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_DefaultConstructor_PageSizeIsTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter();

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_ValidValues_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(3, 5);

            // Assert
            Assert.Equal(3, filter.PageNumber);
            Assert.Equal(5, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageNumberLessThanOne_SetsToOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(0, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_NegativePageNumber_SetsToOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(-5, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageSizeGreaterThanTen_SetsToTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 20);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageSizeExactlyTen_KeepsTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 10);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageSizeLessThanTen_KeepsValue()
        {
            // Arrange & Act
            var filter = new PaginationFilter(2, 7);

            // Assert
            Assert.Equal(7, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageNumberExactlyOne_KeepsOne()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 5);

            // Assert
            Assert.Equal(1, filter.PageNumber);
        }
    }

    public class PaginationHelperTests
    {
        [Fact]
        public void CreatePagedReponse_ValidInputs_ReturnsCorrectPageNumber()
        {
            // Arrange
            var data = new List<string> { "a", "b" };
            var filter = new PaginationFilter(2, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 20);

            // Assert
            Assert.Equal(2, result.PageNumber);
        }

        [Fact]
        public void CreatePagedReponse_ValidInputs_ReturnsCorrectPageSize()
        {
            // Arrange
            var data = new List<string> { "a", "b" };
            var filter = new PaginationFilter(1, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 20);

            // Assert
            Assert.Equal(5, result.PageSize);
        }

        [Fact]
        public void CreatePagedReponse_ValidInputs_ReturnsCorrectTotalRecords()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3 };
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 50);

            // Assert
            Assert.Equal(50, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecords20_PageSize5_TotalPagesFour()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 20);

            // Assert
            Assert.Equal(4, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecords21_PageSize5_TotalPagesFive()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 21);

            // Assert
            Assert.Equal(5, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecordsZero_TotalPagesZero()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 0);

            // Assert
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_DataIsReturned()
        {
            // Arrange
            var data = new List<string> { "item1", "item2", "item3" };
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 3);

            // Assert
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecords10_PageSize10_TotalPagesOne()
        {
            // Arrange
            var data = new List<int>();
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 10);

            // Assert
            Assert.Equal(1, result.TotalPages);
        }
    }

    public class QueryFilterTests
    {
        [Fact]
        public void QueryFilter_DefaultConstructor_SearchByIsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.SearchBy);
        }

        [Fact]
        public void QueryFilter_DefaultConstructor_SearchIsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.Search);
        }

        [Fact]
        public void QueryFilter_DefaultConstructor_SortByIsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.SortBy);
        }

        [Fact]
        public void QueryFilter_DefaultConstructor_IsSortAscendingIsFalse()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
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
        public void QueryFilter_SetIsSortAscending_True_ReturnsTrue()
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
        public void SearchObjects_NullSearchBy_ReturnsAllObjects()
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
        public void SearchObjects_NullSearch_ReturnsAllObjects()
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
        public void SearchObjects_EmptySearchBy_ReturnsAllObjects()
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
        public void SearchObjects_ValidSearchableProperty_FiltersCorrectly()
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
        }

        [Fact]
        public void SearchObjects_NonSearchableProperty_ReturnsAllObjects()
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
        public void SearchObjects_NonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "NonExistentProp", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void SearchObjects_CaseSensitive_FiltersCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple" },
                new TestItem { Name = "apple" }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "Apple", StringComparison.Ordinal).ToList();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void SortObjects_NullSortBy_ReturnsOriginalOrder()
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
        }

        [Fact]
        public void SortObjects_EmptySortBy_ReturnsOriginalOrder()
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
            Assert.Equal(3, result[0].Value);
        }

        [Fact]
        public void SortObjects_ValidSortableProperty_Ascending_SortsCorrectly()
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
        public void SortObjects_ValidSortableProperty_Descending_SortsCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 3 },
                new TestItem { Value = 1 },
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
        public void SortObjects_NonSortableProperty_ReturnsOriginalOrder()
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
        }

        [Fact]
        public void SortObjects_NonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Value = 2 },
                new TestItem { Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "NonExistentProp", true).ToList();

            // Assert
            Assert.Equal(2, result[0].Value);
        }

        [Fact]
        public void SearchObjects_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var items = new List<TestItem>();

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void SortObjects_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var items = new List<TestItem>();

            // Act
            var result = QueryHelper.SortObjects(items, "Value", true);

            // Assert
            Assert.Empty(result);
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
