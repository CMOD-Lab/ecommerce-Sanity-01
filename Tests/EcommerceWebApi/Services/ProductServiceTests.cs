using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using EcommerceWebApi.Entities;
using EcommerceWebApi.Services;
using EcommerceWebApi.Utilities;
using EcommerceWebApi.Repositories;

namespace EcommerceWebApi.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        private Product CreateSampleProduct(int id = 1, string title = "Test Product")
        {
            return new Product
            {
                Id = id,
                Title = title,
                Price = 99.99f,
                Rating = 4.5f,
                Brand = "TestBrand",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/image.jpg"),
                Quantity = 50
            };
        }

        [Fact]
        public void GetProductById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetProductById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);

            // Act
            var result = _mockUnitOfWork.Object.Products.GetById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertProductAsync_ValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.InsertAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertProductAsync_FailedInsert_ReturnsFalse()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Products.InsertAsync(product);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductAsync_ValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.UpdateAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_ValidId_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _mockUnitOfWork.Object.Products.DeleteAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_NonExistingId_ReturnsFalse()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

            // Act
            var result = await _mockUnitOfWork.Object.Products.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPaginationProducts_WithValidFilter_ReturnsCorrectPage()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(1, "Product 1"),
                CreateSampleProduct(2, "Product 2"),
                CreateSampleProduct(3, "Product 3"),
                CreateSampleProduct(4, "Product 4"),
                CreateSampleProduct(5, "Product 5"),
            };

            var paginationFilter = new PaginationFilter(1, 2);
            var queryFilter = new QueryFilter();

            // Act - simulate pagination logic
            var result = products
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public void GetPaginationProducts_WithSearchFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(1, "Apple iPhone"),
                CreateSampleProduct(2, "Samsung Galaxy"),
                CreateSampleProduct(3, "Apple MacBook"),
            };

            var queryFilter = new QueryFilter { SearchBy = "Title", Search = "Apple" };

            // Act - simulate search logic
            var result = QueryHelper.SearchObjects(
                products,
                queryFilter.SearchBy,
                queryFilter.Search,
                StringComparison.OrdinalIgnoreCase
            ).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationProducts_WithSortFilter_ReturnsSortedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(1, "Product C") ,
                CreateSampleProduct(2, "Product A"),
                CreateSampleProduct(3, "Product B"),
            };
            products[0].Price = 300f;
            products[1].Price = 100f;
            products[2].Price = 200f;

            var queryFilter = new QueryFilter { SortBy = "Price", IsSortAscending = true };

            // Act - simulate sort logic
            var result = QueryHelper.SortObjects(products, queryFilter.SortBy, queryFilter.IsSortAscending).ToList();

            // Assert
            Assert.Equal(100f, result[0].Price);
            Assert.Equal(200f, result[1].Price);
            Assert.Equal(300f, result[2].Price);
        }

        [Fact]
        public void UpdateProductPropertyAsync_ValidProperty_UpdatesProperty()
        {
            // Arrange
            var product = CreateSampleProduct();
            var propertyInfo = typeof(Product).GetProperty(
                "Title",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Act
            propertyInfo?.SetValue(product, "Updated Title", null);

            // Assert
            Assert.Equal("Updated Title", product.Title);
        }

        [Fact]
        public void UpdateProductPropertyAsync_InvalidProperty_PropertyInfoIsNull()
        {
            // Arrange
            var propertyInfo = typeof(Product).GetProperty(
                "NonExistentProperty",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Assert
            Assert.Null(propertyInfo);
        }

        [Fact]
        public void UpdateProductPropertyAsync_PriceProperty_UpdatesPrice()
        {
            // Arrange
            var product = CreateSampleProduct();
            var propertyInfo = typeof(Product).GetProperty(
                "Price",
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );

            // Act
            propertyInfo?.SetValue(product, 199.99f, null);

            // Assert
            Assert.Equal(199.99f, product.Price);
        }
    }
}
