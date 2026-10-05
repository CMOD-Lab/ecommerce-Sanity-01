using EcommerceWebApi.DTOs;
using EcommerceWebApi.Utilities;
using System;
using System.Collections.Generic;
using Xunit;

namespace EcommerceWebApi.Tests.Utilities
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
        public void PaginationFilter_ParameterizedConstructor_ValidPageNumber()
        {
            // Arrange & Act
            var filter = new PaginationFilter(3, 5);

            // Assert
            Assert.Equal(3, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_ValidPageSize()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 5);

            // Assert
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
            var filter = new PaginationFilter(1, 7);

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

        [Fact]
        public void PaginationFilter_SetPageNumber_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new PaginationFilter();

            // Act
            filter.PageNumber = 5;

            // Assert
            Assert.Equal(5, filter.PageNumber);
        }

        [Fact]
        public void PaginationFilter_SetPageSize_ReturnsCorrectValue()
        {
            // Arrange
            var filter = new PaginationFilter();

            // Act
            filter.PageSize = 8;

            // Assert
            Assert.Equal(8, filter.PageSize);
        }
    }

    public class PaginationHelperTests
    {
        [Fact]
        public void CreatePagedReponse_ReturnsCorrectPageNumber()
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
        public void CreatePagedReponse_ReturnsCorrectPageSize()
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
        public void CreatePagedReponse_ReturnsCorrectTotalRecords()
        {
            // Arrange
            var data = new List<string> { "a", "b" };
            var filter = new PaginationFilter(1, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 20);

            // Assert
            Assert.Equal(20, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedReponse_ReturnsCorrectTotalPages_ExactDivision()
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
        public void CreatePagedReponse_ReturnsCorrectTotalPages_WithRemainder()
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
        public void CreatePagedReponse_ReturnsCorrectData()
        {
            // Arrange
            var data = new List<string> { "item1", "item2" };
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 2);

            // Assert
            Assert.Equal(data, result.Data);
        }

        [Fact]
        public void CreatePagedReponse_ZeroTotalRecords_TotalPagesIsZero()
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
        public void CreatePagedReponse_OneTotalRecord_TotalPagesIsOne()
        {
            // Arrange
            var data = new List<string> { "item" };
            var filter = new PaginationFilter(1, 10);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 1);

            // Assert
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public void CreatePagedReponse_TotalRecordsEqualPageSize_TotalPagesIsOne()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var filter = new PaginationFilter(1, 5);

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, 5);

            // Assert
            Assert.Equal(1, result.TotalPages);
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
        public void QueryFilter_SetIsSortAscending_True()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.IsSortAscending = true;

            // Assert
            Assert.True(filter.IsSortAscending);
        }

        [Fact]
        public void QueryFilter_SetIsSortAscending_False()
        {
            // Arrange
            var filter = new QueryFilter();

            // Act
            filter.IsSortAscending = false;

            // Assert
            Assert.False(filter.IsSortAscending);
        }

        [Fact]
        public void QueryFilter_DefaultSearchBy_IsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.SearchBy);
        }

        [Fact]
        public void QueryFilter_DefaultSearch_IsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.Search);
        }

        [Fact]
        public void QueryFilter_DefaultSortBy_IsNull()
        {
            // Arrange & Act
            var filter = new QueryFilter();

            // Assert
            Assert.Null(filter.SortBy);
        }
    }

    public class QueryHelperTests
    {
        private class TestItem
        {
            [Searchable]
            public string Name { get; set; } = null!;

            [Sortable]
            public int Value { get; set; }

            public string NonSearchable { get; set; } = null!;
        }

        [Fact]
        public void SearchObjects_NullSearchBy_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 },
                new TestItem { Name = "Banana", Value = 2 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, null, "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_EmptySearchBy_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 },
                new TestItem { Name = "Banana", Value = 2 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "", "Apple", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_NullSearch_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 },
                new TestItem { Name = "Banana", Value = 2 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", null, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_ValidSearchableProperty_FiltersCorrectly()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 },
                new TestItem { Name = "Banana", Value = 2 },
                new TestItem { Name = "Apricot", Value = 3 }
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
                new TestItem { Name = "Apple", NonSearchable = "X", Value = 1 },
                new TestItem { Name = "Banana", NonSearchable = "X", Value = 2 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "NonSearchable", "X", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void SearchObjects_NonExistentProperty_ReturnsAllObjects()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "NonExistent", "test", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void SearchObjects_CaseInsensitive_FindsMatch()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "Apple", Value = 1 },
                new TestItem { Name = "Banana", Value = 2 }
            };

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "apple", StringComparison.OrdinalIgnoreCase).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Apple", result[0].Name);
        }

        [Fact]
        public void SortObjects_NullSortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "B", Value = 2 },
                new TestItem { Name = "A", Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, null, true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("B", result[0].Name);
        }

        [Fact]
        public void SortObjects_EmptySortBy_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "B", Value = 2 },
                new TestItem { Name = "A", Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "", true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SortObjects_ValidSortableProperty_Ascending()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "C", Value = 3 },
                new TestItem { Name = "A", Value = 1 },
                new TestItem { Name = "B", Value = 2 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "Value", true).ToList();

            // Assert
            Assert.Equal(1, result[0].Value);
            Assert.Equal(2, result[1].Value);
            Assert.Equal(3, result[2].Value);
        }

        [Fact]
        public void SortObjects_ValidSortableProperty_Descending()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "C", Value = 3 },
                new TestItem { Name = "A", Value = 1 },
                new TestItem { Name = "B", Value = 2 }
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
                new TestItem { Name = "B", Value = 2 },
                new TestItem { Name = "A", Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "NonSearchable", true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("B", result[0].Name);
        }

        [Fact]
        public void SortObjects_NonExistentProperty_ReturnsOriginalOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem { Name = "B", Value = 2 },
                new TestItem { Name = "A", Value = 1 }
            };

            // Act
            var result = QueryHelper.SortObjects(items, "NonExistent", true).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void SearchObjects_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var items = new List<TestItem>();

            // Act
            var result = QueryHelper.SearchObjects(items, "Name", "test", StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void SortObjects_EmptyList_ReturnsEmpty()
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
