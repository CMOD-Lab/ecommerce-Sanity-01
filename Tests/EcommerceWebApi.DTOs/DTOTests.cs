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
            var dto = new PaginationDTO<List<string>>(new List<string>(), 1, 10);

            // Assert
            Assert.Equal(1, dto.PageNumber);
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
            Assert.Equal(2, dto.Data.Count);
        }

        [Fact]
        public void PaginationDTO_Constructor_WithEmptyData_SetsEmptyData()
        {
            // Arrange
            var data = new List<string>();

            // Act
            var dto = new PaginationDTO<List<string>>(data, 1, 10);

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

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 7)]
        public void PaginationDTO_Constructor_WithVariousPageParams_SetsCorrectly(int pageNumber, int pageSize)
        {
            // Arrange & Act
            var dto = new PaginationDTO<List<int>>(new List<int>(), pageNumber, pageSize);

            // Assert
            Assert.Equal(pageNumber, dto.PageNumber);
            Assert.Equal(pageSize, dto.PageSize);
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
            Assert.Contains(1, dto.Data);
            Assert.Contains(2, dto.Data);
            Assert.Contains(3, dto.Data);
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
        public void ProductDTO_SetTitle_ReturnsCorrectTitle()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Title = "Test Product";

            // Assert
            Assert.Equal("Test Product", dto.Title);
        }

        [Fact]
        public void ProductDTO_SetPrice_ReturnsCorrectPrice()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Price = 49.99f;

            // Assert
            Assert.Equal(49.99f, dto.Price);
        }

        [Fact]
        public void ProductDTO_SetBrand_ReturnsCorrectBrand()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Brand = "Nike";

            // Assert
            Assert.Equal("Nike", dto.Brand);
        }

        [Fact]
        public void ProductDTO_SetCategory_ReturnsCorrectCategory()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Category = "Shoes";

            // Assert
            Assert.Equal("Shoes", dto.Category);
        }

        [Fact]
        public void ProductDTO_SetQuantity_ReturnsCorrectQuantity()
        {
            // Arrange
            var dto = new ProductDTO();

            // Act
            dto.Quantity = 25;

            // Assert
            Assert.Equal(25, dto.Quantity);
        }

        [Fact]
        public void ProductDTO_SetThumbnail_ReturnsCorrectUri()
        {
            // Arrange
            var dto = new ProductDTO();
            var uri = new Uri("https://example.com/product.jpg");

            // Act
            dto.Thumbnail = uri;

            // Assert
            Assert.Equal(uri, dto.Thumbnail);
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
        public void UserDTO_SetId_ReturnsCorrectId()
        {
            // Arrange
            var dto = new UserDTO();
            var id = Guid.NewGuid().ToString();

            // Act
            dto.Id = id;

            // Assert
            Assert.Equal(id, dto.Id);
        }

        [Fact]
        public void UserDTO_SetUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var dto = new UserDTO();

            // Act
            dto.Username = "john_doe";

            // Assert
            Assert.Equal("john_doe", dto.Username);
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
        public void UserDTO_IsTwoFactorAuthActivated_DefaultFalse()
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
        public void OrderDTO_SetProductList_ReturnsCorrectProductList()
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
        public void OrderDTO_ProductList_WithMultipleItems_CountIsCorrect()
        {
            // Arrange
            var dto = new OrderDTO
            {
                ProductList = new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 3 },
                    { 5, 2 }
                }
            };

            // Assert
            Assert.Equal(3, dto.ProductList.Count);
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
        public void LoginDTO_SetUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var dto = new LoginDTO();

            // Act
            dto.Username = "admin";

            // Assert
            Assert.Equal("admin", dto.Username);
        }

        [Fact]
        public void LoginDTO_SetPassword_ReturnsCorrectPassword()
        {
            // Arrange
            var dto = new LoginDTO();

            // Act
            dto.Password = "secret123";

            // Assert
            Assert.Equal("secret123", dto.Password);
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
        public void RegisterDTO_SetUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.Username = "newuser";

            // Assert
            Assert.Equal("newuser", dto.Username);
        }

        [Fact]
        public void RegisterDTO_SetPassword_ReturnsCorrectPassword()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.Password = "password123";

            // Assert
            Assert.Equal("password123", dto.Password);
        }

        [Fact]
        public void RegisterDTO_SetConfirmPassword_ReturnsCorrectConfirmPassword()
        {
            // Arrange
            var dto = new RegisterDTO();

            // Act
            dto.ConfirmPassword = "password123";

            // Assert
            Assert.Equal("password123", dto.ConfirmPassword);
        }

        [Fact]
        public void RegisterDTO_PasswordAndConfirmPassword_CanMatch()
        {
            // Arrange & Act
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
        public void TotpDTO_SetQrCode_ReturnsCorrectQrCode()
        {
            // Arrange
            var dto = new TotpDTO();

            // Act
            dto.QrCode = "otpauth://totp/test";

            // Assert
            Assert.Equal("otpauth://totp/test", dto.QrCode);
        }

        [Fact]
        public void TotpDTO_SetTotp_ReturnsCorrectTotp()
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
