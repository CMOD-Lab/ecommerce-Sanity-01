using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Utilities;

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
        public void PaginationFilter_ParameterizedConstructor_ValidValues_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(2, 5);

            // Assert
            Assert.Equal(2, filter.PageNumber);
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
        public void PaginationFilter_ParameterizedConstructor_PageSizeExactlyTen_SetsToTen()
        {
            // Arrange & Act
            var filter = new PaginationFilter(1, 10);

            // Assert
            Assert.Equal(10, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageSizeLessThanTen_SetsCorrectly()
        {
            // Arrange & Act
            var filter = new PaginationFilter(3, 7);

            // Assert
            Assert.Equal(3, filter.PageNumber);
            Assert.Equal(7, filter.PageSize);
        }

        [Fact]
        public void PaginationFilter_ParameterizedConstructor_PageNumberOne_SetsCorrectly()
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
        public void CreatePagedResponse_ValidData_ReturnsCorrectPagination()
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
            Assert.Equal(4, result.TotalPages); // ceil(10/3) = 4
        }

        [Fact]
        public void CreatePagedResponse_ExactDivision_ReturnsCorrectTotalPages()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3, 4, 5 };
            var filter = new PaginationFilter(1, 5);
            int totalRecords = 10;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(2, result.TotalPages); // 10/5 = 2 exactly
        }

        [Fact]
        public void CreatePagedResponse_EmptyData_ReturnsZeroTotalPages()
        {
            // Arrange
            var data = new List<string>();
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 0;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRecords);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public void CreatePagedResponse_SingleRecord_ReturnsOneTotalPage()
        {
            // Arrange
            var data = new List<string> { "only-item" };
            var filter = new PaginationFilter(1, 10);
            int totalRecords = 1;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public void CreatePagedResponse_PageNumberTwo_ReturnsCorrectPageNumber()
        {
            // Arrange
            var data = new List<string> { "item4", "item5" };
            var filter = new PaginationFilter(2, 3);
            int totalRecords = 5;

            // Act
            var result = PaginationHelper.CreatePagedReponse(data, filter, totalRecords);

            // Assert
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(3, result.PageSize);
        }
    }
}
