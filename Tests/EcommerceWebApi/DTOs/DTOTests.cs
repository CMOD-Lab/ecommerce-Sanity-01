using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.DTOs;

namespace EcommerceWebApi.Tests.DTOs
{
    public class PaginationDTOTests
    {
        [Fact]
        public void PaginationDTO_Constructor_SetsDataPageNumberPageSize()
        {
            // Arrange
            var data = new List<string> { "item1", "item2" };

            // Act
            var dto = new PaginationDTO<List<string>>(data, 1, 10);

            // Assert
            Assert.Equal(data, dto.Data);
            Assert.Equal(1, dto.PageNumber);
            Assert.Equal(10, dto.PageSize);
        }

        [Fact]
        public void PaginationDTO_SetTotalPages_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new PaginationDTO<string>("test", 1, 10);

            // Act
            dto.TotalPages = 5;

            // Assert
            Assert.Equal(5, dto.TotalPages);
        }

        [Fact]
        public void PaginationDTO_SetTotalRecords_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new PaginationDTO<string>("test", 1, 10);

            // Act
            dto.TotalRecords = 100;

            // Assert
            Assert.Equal(100, dto.TotalRecords);
        }

        [Fact]
        public void PaginationDTO_DefaultTotalPages_IsZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("test", 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalPages);
        }

        [Fact]
        public void PaginationDTO_DefaultTotalRecords_IsZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("test", 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalRecords);
        }

        [Fact]
        public void PaginationDTO_WithIntData_WorksCorrectly()
        {
            // Arrange & Act
            var dto = new PaginationDTO<int>(42, 2, 5);

            // Assert
            Assert.Equal(42, dto.Data);
            Assert.Equal(2, dto.PageNumber);
            Assert.Equal(5, dto.PageSize);
        }
    }

    public class OrderDTOTests
    {
        [Fact]
        public void OrderDTO_SetProductList_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new OrderDTO();
            var productList = new Dictionary<int, int> { { 1, 2 }, { 3, 4 } };

            // Act
            dto.ProductList = productList;

            // Assert
            Assert.Equal(productList, dto.ProductList);
        }

        [Fact]
        public void OrderDTO_ProductList_DefaultIsNull()
        {
            // Arrange & Act
            var dto = new OrderDTO();

            // Assert - ProductList is null! by default
            Assert.Null(dto.ProductList);
        }
    }

    public class ProductDTOTests
    {
        [Fact]
        public void ProductDTO_SetTitle_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Title = "Test Product";

            // Assert
            Assert.Equal("Test Product", dto.Title);
        }

        [Fact]
        public void ProductDTO_SetPrice_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Price = 99.99f;

            // Assert
            Assert.Equal(99.99f, dto.Price);
        }

        [Fact]
        public void ProductDTO_SetBrand_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Brand = "TestBrand";

            // Assert
            Assert.Equal("TestBrand", dto.Brand);
        }

        [Fact]
        public void ProductDTO_SetCategory_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Category = "Electronics";

            // Assert
            Assert.Equal("Electronics", dto.Category);
        }

        [Fact]
        public void ProductDTO_SetThumbnail_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();
            var uri = new Uri("https://example.com/image.jpg");

            // Act
            dto.Thumbnail = uri;

            // Assert
            Assert.Equal(uri, dto.Thumbnail);
        }

        [Fact]
        public void ProductDTO_SetQuantity_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Quantity = 50;

            // Assert
            Assert.Equal(50, dto.Quantity);
        }
    }

    public class UserDTOTests
    {
        [Fact]
        public void UserDTO_SetId_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Id = "user-123";

            // Assert
            Assert.Equal("user-123", dto.Id);
        }

        [Fact]
        public void UserDTO_SetUsername_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Username = "testuser";

            // Assert
            Assert.Equal("testuser", dto.Username);
        }

        [Fact]
        public void UserDTO_SetRole_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Role = "Admin";

            // Assert
            Assert.Equal("Admin", dto.Role);
        }

        [Fact]
        public void UserDTO_SetIsTwoFactorAuthActivated_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.IsTwoFactorAuthActivated = true;

            // Assert
            Assert.True(dto.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void UserDTO_DefaultIsTwoFactorAuthActivated_IsFalse()
        {
            // Arrange & Act
            var dto = new UserDTO();

            // Assert
            Assert.False(dto.IsTwoFactorAuthActivated);
        }
    }

    public class LoginDTOTests
    {
        [Fact]
        public void LoginDTO_SetUsername_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new LoginDTO();

            // Act
            dto.Username = "testuser";

            // Assert
            Assert.Equal("testuser", dto.Username);
        }

        [Fact]
        public void LoginDTO_SetPassword_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new LoginDTO();

            // Act
            dto.Password = "password123";

            // Assert
            Assert.Equal("password123", dto.Password);
        }
    }

    public class RegisterDTOTests
    {
        [Fact]
        public void RegisterDTO_SetUsername_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.Username = "newuser";

            // Assert
            Assert.Equal("newuser", dto.Username);
        }

        [Fact]
        public void RegisterDTO_SetPassword_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.Password = "securepassword";

            // Assert
            Assert.Equal("securepassword", dto.Password);
        }

        [Fact]
        public void RegisterDTO_SetConfirmPassword_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.ConfirmPassword = "securepassword";

            // Assert
            Assert.Equal("securepassword", dto.ConfirmPassword);
        }
    }

    public class TotpDTOTests
    {
        [Fact]
        public void TotpDTO_SetQrCode_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new TotpDTO();

            // Act
            dto.QrCode = "otpauth://totp/test";

            // Assert
            Assert.Equal("otpauth://totp/test", dto.QrCode);
        }

        [Fact]
        public void TotpDTO_SetTotp_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new TotpDTO();

            // Act
            dto.Totp = "123456";

            // Assert
            Assert.Equal("123456", dto.Totp);
        }
    }
}
