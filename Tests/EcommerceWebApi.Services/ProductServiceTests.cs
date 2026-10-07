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

namespace EcommerceWebApi.Services.Tests
{
    // Testable subclass to bypass concrete UnitOfWork constructor dependency
    internal class ProductServiceTestable : ProductService
    {
        public ProductServiceTestable(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }

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

        private Product CreateSampleProduct(int id = 1)
        {
            return new Product
            {
                Id = id,
                Title = $"Product {id}",
                Price = 99.99f,
                Rating = 4.0f,
                Brand = "TestBrand",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/product.jpg"),
                Quantity = 10
            };
        }

        [Fact]
        public void GetAllProducts_WhenProductsExist_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(1),
                CreateSampleProduct(2),
                CreateSampleProduct(3)
            }.AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAllProducts_WhenNoProducts_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<Product>().AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetProductById_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var product = CreateSampleProduct(5);
            _mockProductRepository.Setup(r => r.GetById(5)).Returns(product);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetProductById(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public void GetProductById_WhenProductNotExists_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(It.IsAny<int>())).Returns((Product?)null);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetProductById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetNextProductId_WhenProductsExist_ReturnsMaxIdPlusOne()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateSampleProduct(1),
                CreateSampleProduct(5),
                CreateSampleProduct(3)
            }.AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetNextProductId();

            // Assert
            Assert.Equal(6, result);
        }

        [Fact]
        public void GetNextProductId_WhenNoProducts_ReturnsOne()
        {
            // Arrange
            var products = new List<Product>().AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetNextProductId();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetPaginationProducts_WithValidFilter_ReturnsPaginatedProducts()
        {
            // Arrange
            var products = new List<Product>();
            for (int i = 1; i <= 15; i++)
            {
                products.Add(CreateSampleProduct(i));
            }
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products.AsQueryable());

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);
            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
            Assert.Equal(15, count);
        }

        [Fact]
        public void GetPaginationProducts_WithSearchFilter_ReturnsFilteredProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Apple iPhone", Brand = "Apple", Category = "Electronics", Price = 999f, Rating = 4.5f, Quantity = 10, Thumbnail = new Uri("https://example.com/1.jpg") },
                new Product { Id = 2, Title = "Samsung Galaxy", Brand = "Samsung", Category = "Electronics", Price = 799f, Rating = 4.2f, Quantity = 20, Thumbnail = new Uri("https://example.com/2.jpg") },
            }.AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);
            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SearchBy = "Title", Search = "Apple" };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, count);
        }

        [Fact]
        public void GetPaginationProducts_WithSortFilter_ReturnsSortedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product A", Brand = "BrandA", Category = "Cat", Price = 200f, Rating = 4.0f, Quantity = 10, Thumbnail = new Uri("https://example.com/1.jpg") },
                new Product { Id = 2, Title = "Product B", Brand = "BrandB", Category = "Cat", Price = 100f, Rating = 3.5f, Quantity = 20, Thumbnail = new Uri("https://example.com/2.jpg") },
            }.AsQueryable();
            _mockProductRepository.Setup(r => r.GetAll()).Returns(products);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);
            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SortBy = "Price", IsSortAscending = true };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(100f, result[0].Price);
        }

        [Fact]
        public async Task InsertProductAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertProductAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(false);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_WithValidProperty_ReturnsTrue()
        {
            // Arrange
            var product = CreateSampleProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "Title", "Updated Title");

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Title", product.Title);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_WithInvalidProperty_ReturnsFalse()
        {
            // Arrange
            var product = CreateSampleProduct();
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "NonExistentProperty", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteProductAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductNotFound_ReturnsFalse()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(It.IsAny<int>())).ReturnsAsync(false);

            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteProductAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Dispose());
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act & Assert
            var exception = Record.Exception(() => service.Dispose());
            Assert.Null(exception);
        }
    }
}
