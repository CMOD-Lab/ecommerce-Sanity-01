using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.DTOs;

namespace EcommerceWebApi.DTOs.Tests
{
    public class PaginationDTOTests
    {
        [Fact]
        public void PaginationDTO_Constructor_SetsPageNumberAndPageSize()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<string>>(new List<string>(), 2, 5);

            // Assert
            Assert.Equal(2, dto.PageNumber);
            Assert.Equal(5, dto.PageSize);
        }

        [Fact]
        public void PaginationDTO_Constructor_SetsData()
        {
            // Arrange
            var data = new List<string> { "item1", "item2" };

            // Act
            var dto = new PaginationDTO<List<string>>(data, 1, 10);

            // Assert
            Assert.Equal(data, dto.Data);
            Assert.Equal(2, dto.Data.Count);
        }

        [Fact]
        public void PaginationDTO_Constructor_EmptyData_IsValid()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<int>>(new List<int>(), 1, 10);

            // Assert
            Assert.NotNull(dto.Data);
            Assert.Empty(dto.Data);
        }

        [Fact]
        public void PaginationDTO_SetTotalPages_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Act
            dto.TotalPages = 5;

            // Assert
            Assert.Equal(5, dto.TotalPages);
        }

        [Fact]
        public void PaginationDTO_SetTotalRecords_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Act
            dto.TotalRecords = 100;

            // Assert
            Assert.Equal(100, dto.TotalRecords);
        }

        [Fact]
        public void PaginationDTO_PageNumberOne_IsValid()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("data", 1, 10);

            // Assert
            Assert.Equal(1, dto.PageNumber);
        }

        [Fact]
        public void PaginationDTO_WithIntData_WorksCorrectly()
        {
            // Arrange & Act
            var dto = new PaginationDTO<int>(42, 1, 10);

            // Assert
            Assert.Equal(42, dto.Data);
        }

        [Fact]
        public void PaginationDTO_DefaultTotalPages_IsZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("data", 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalPages);
        }

        [Fact]
        public void PaginationDTO_DefaultTotalRecords_IsZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("data", 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalRecords);
        }
    }

    public class ProductDTOTests
    {
        [Fact]
        public void ProductDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new ProductDTO();

            // Assert
            Assert.NotNull(dto);
        }

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
            dto.Price = 49.99f;

            // Assert
            Assert.Equal(49.99f, dto.Price);
        }

        [Fact]
        public void ProductDTO_SetBrand_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Brand = "BrandX";

            // Assert
            Assert.Equal("BrandX", dto.Brand);
        }

        [Fact]
        public void ProductDTO_SetCategory_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Category = "Clothing";

            // Assert
            Assert.Equal("Clothing", dto.Category);
        }

        [Fact]
        public void ProductDTO_SetThumbnail_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new ProductDTO();
            var uri = new Uri("https://example.com/thumb.jpg");

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
            dto.Quantity = 25;

            // Assert
            Assert.Equal(25, dto.Quantity);
        }
    }

    public class UserDTOTests
    {
        [Fact]
        public void UserDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new UserDTO();

            // Assert
            Assert.NotNull(dto);
        }

        [Fact]
        public void UserDTO_SetId_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Id = "user-id-123";

            // Assert
            Assert.Equal("user-id-123", dto.Id);
        }

        [Fact]
        public void UserDTO_SetUsername_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Username = "johndoe";

            // Assert
            Assert.Equal("johndoe", dto.Username);
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
        public void UserDTO_SetIsTwoFactorAuthActivated_True_ReturnsTrue()
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

    public class OrderDTOTests
    {
        [Fact]
        public void OrderDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new OrderDTO();

            // Assert
            Assert.NotNull(dto);
        }

        [Fact]
        public void OrderDTO_SetProductList_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new OrderDTO();
            var productList = new Dictionary<int, int> { { 1, 2 }, { 3, 1 } };

            // Act
            dto.ProductList = productList;

            // Assert
            Assert.Equal(productList, dto.ProductList);
            Assert.Equal(2, dto.ProductList.Count);
        }

        [Fact]
        public void OrderDTO_ProductList_EmptyDictionary_IsValid()
        {
            // Arrange
            var dto = new OrderDTO();

            // Act
            dto.ProductList = new Dictionary<int, int>();

            // Assert
            Assert.NotNull(dto.ProductList);
            Assert.Empty(dto.ProductList);
        }
    }

    public class LoginDTOTests
    {
        [Fact]
        public void LoginDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new LoginDTO();

            // Assert
            Assert.NotNull(dto);
        }

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
        public void RegisterDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new RegisterDTO();

            // Assert
            Assert.NotNull(dto);
        }

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
            dto.Password = "securepass";

            // Assert
            Assert.Equal("securepass", dto.Password);
        }

        [Fact]
        public void RegisterDTO_SetConfirmPassword_ReturnsCorrectValue()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.ConfirmPassword = "securepass";

            // Assert
            Assert.Equal("securepass", dto.ConfirmPassword);
        }

        [Fact]
        public void RegisterDTO_PasswordAndConfirmMatch_AreEqual()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Username = "user",
                Password = "pass123",
                ConfirmPassword = "pass123"
            };

            // Assert
            Assert.Equal(dto.Password, dto.ConfirmPassword);
        }
    }

    public class TotpDTOTests
    {
        [Fact]
        public void TotpDTO_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var dto = new TotpDTO();

            // Assert
            Assert.NotNull(dto);
        }

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
