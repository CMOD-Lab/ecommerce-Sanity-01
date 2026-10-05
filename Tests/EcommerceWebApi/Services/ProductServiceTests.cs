using EcommerceWebApi.Entities;
using EcommerceWebApi.Repositories;
using EcommerceWebApi.Services;
using EcommerceWebApi.Tests.Helpers;
using EcommerceWebApi.Utilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EcommerceWebApi.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        private Product CreateTestProduct(int id = 1, string title = "Test Product")
        {
            return new Product
            {
                Id = id,
                Title = title,
                Price = 99.99f,
                Rating = 4.0f,
                Brand = "TestBrand",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/img.jpg"),
                Quantity = 10
            };
        }

        [Fact]
        public void GetProductById_ExistingId_ReturnsProduct()
        {
            // Arrange
            var product = CreateTestProduct(1);
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            var service = CreateProductService();

            // Act
            var result = service.GetProductById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
        }

        [Fact]
        public void GetProductById_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);
            var service = CreateProductService();

            // Act
            var result = service.GetProductById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InsertProductAsync_Success_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);
            var service = CreateProductService();

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertProductAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(false);
            var service = CreateProductService();

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductAsync_Success_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);
            var service = CreateProductService();

            // Act
            var result = await service.UpdateProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductAsync_Failure_ReturnsFalse()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(false);
            var service = CreateProductService();

            // Act
            var result = await service.UpdateProductAsync(product);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProductAsync_Success_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = CreateProductService();

            // Act
            var result = await service.DeleteProductAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_Failure_ReturnsFalse()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = CreateProductService();

            // Act
            var result = await service.DeleteProductAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_ValidProperty_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            var service = CreateProductService();

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "Title", "New Title");

            // Assert
            Assert.True(result);
            Assert.Equal("New Title", product.Title);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_InvalidProperty_ReturnsFalse()
        {
            // Arrange
            var product = CreateTestProduct();
            var service = CreateProductService();

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "NonExistentProp", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_PriceProperty_UpdatesPrice()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            var service = CreateProductService();

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "Price", 199.99f);

            // Assert
            Assert.True(result);
            Assert.Equal(199.99f, product.Price);
        }

        [Fact]
        public void GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateTestProduct(1, "Product 1"),
                CreateTestProduct(2, "Product 2"),
                CreateTestProduct(3, "Product 3")
            };
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateProductService();

            // Act
            var result = service.GetAllProducts();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetNextProductId_ReturnsNextId()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateTestProduct(1),
                CreateTestProduct(2)
            };
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateProductService();

            // Act
            var result = service.GetNextProductId();

            // Assert
            Assert.True(result >= 0);
        }

        [Fact]
        public void GetPaginationProducts_WithSearch_FiltersCorrectly()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateTestProduct(1, "Laptop"),
                CreateTestProduct(2, "Phone"),
                CreateTestProduct(3, "Laptop Pro")
            };
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateProductService();

            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SearchBy = "Title", Search = "Laptop" };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(2, count);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationProducts_WithSort_SortsCorrectly()
        {
            // Arrange
            var products = new List<Product>();
            var p1 = CreateTestProduct(1, "C Product");
            p1.Price = 30f;
            products.Add(p1);
            var p2 = CreateTestProduct(2, "A Product");
            p2.Price = 10f;
            products.Add(p2);
            var p3 = CreateTestProduct(3, "B Product");
            p3.Price = 20f;
            products.Add(p3);

            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateProductService();

            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SortBy = "Price", IsSortAscending = true };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(3, count);
            Assert.Equal(10f, result[0].Price);
        }

        [Fact]
        public void GetPaginationProducts_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var products = Enumerable.Range(1, 15)
                .Select(i => CreateTestProduct(i, $"Product {i}"))
                .ToList();
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = CreateProductService();

            var paginationFilter = new PaginationFilter(2, 5);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(15, count);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var service = CreateProductService();

            // Act & Assert (no exception)
            service.Dispose();
        }

        private ProductService CreateProductService()
        {
            return new TestableProductService(_mockUnitOfWork.Object);
        }
    }

    /// <summary>
    /// Testable ProductService that accepts IUnitOfWork mock
    /// </summary>
    internal class TestableProductService : ProductService
    {
        public TestableProductService(IUnitOfWork unitOfWork) : base(null!)
        {
            typeof(ProductService)
                .GetField("_unitOfWork", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, unitOfWork);
        }
    }
}
