using EcommerceWebApi.DTOs;
using System;
using System.Collections.Generic;
using Xunit;

namespace EcommerceWebApi.Tests.DTOs
{
    public class PaginationDTOTests
    {
        [Fact]
        public void PaginationDTO_Constructor_SetsPageNumber()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<string>>(new List<string>(), 2, 5);

            // Assert
            Assert.Equal(2, dto.PageNumber);
        }

        [Fact]
        public void PaginationDTO_Constructor_SetsPageSize()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Assert
            Assert.Equal(10, dto.PageSize);
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
        }

        [Fact]
        public void PaginationDTO_Constructor_TotalPagesDefaultsToZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalPages);
        }

        [Fact]
        public void PaginationDTO_Constructor_TotalRecordsDefaultsToZero()
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Assert
            Assert.Equal(0, dto.TotalRecords);
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
            dto.TotalRecords = 50;

            // Assert
            Assert.Equal(50, dto.TotalRecords);
        }

        [Fact]
        public void PaginationDTO_WithIntData_WorksCorrectly()
        {
            // Arrange
            var data = new List<int> { 1, 2, 3 };

            // Act
            var dto = new PaginationDTO<List<int>>(data, 1, 10);

            // Assert
            Assert.Equal(3, dto.Data.Count);
        }

        [Fact]
        public void PaginationDTO_PageNumberOne_IsFirstPage()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string>("data", 1, 10);

            // Assert
            Assert.Equal(1, dto.PageNumber);
        }

        [Fact]
        public void PaginationDTO_WithNullData_DoesNotThrow()
        {
            // Arrange & Act
            var dto = new PaginationDTO<string?>(null, 1, 10);

            // Assert
            Assert.Null(dto.Data);
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
            dto.Price = 29.99f;

            // Assert
            Assert.Equal(29.99f, dto.Price);
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
        public void UserDTO_SetIsTwoFactorAuthActivated_True()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.IsTwoFactorAuthActivated = true;

            // Assert
            Assert.True(dto.IsTwoFactorAuthActivated);
        }

        [Fact]
        public void UserDTO_SetIsTwoFactorAuthActivated_False()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.IsTwoFactorAuthActivated = false;

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
            var productList = new Dictionary<int, int> { { 1, 3 }, { 2, 1 } };

            // Act
            dto.ProductList = productList;

            // Assert
            Assert.Equal(productList, dto.ProductList);
        }

        [Fact]
        public void OrderDTO_ProductList_ContainsCorrectItems()
        {
            // Arrange
            var dto = new OrderDTO
            {
                ProductList = new Dictionary<int, int> { { 5, 2 } }
            };

            // Assert
            Assert.True(dto.ProductList.ContainsKey(5));
            Assert.Equal(2, dto.ProductList[5]);
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
        public void RegisterDTO_PasswordsMatch_AreEqual()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Username = "user",
                Password = "mypassword",
                ConfirmPassword = "mypassword"
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
