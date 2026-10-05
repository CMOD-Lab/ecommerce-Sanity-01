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
        public void GetProductById_WithValidId_ReturnsProduct()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.GetById(1)).Returns(product);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetProductById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetProductById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.GetById(999)).Returns((Product?)null);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetProductById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateTestProduct(1, "Product A"),
                CreateTestProduct(2, "Product B"),
                CreateTestProduct(3, "Product C")
            };
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

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
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = service.GetNextProductId();

            // Assert
            Assert.Equal(3, result); // 2 items + 1
        }

        [Fact]
        public async Task InsertProductAsync_WithValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(true);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InsertProductAsync_WhenRepositoryFails_ReturnsFalse()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.InsertAsync(product)).ReturnsAsync(false);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.InsertProductAsync(product);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidProduct_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(product)).ReturnsAsync(true);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductAsync(product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(u => u.CommitTransaction());
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.DeleteProductAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_WithValidProperty_ReturnsTrue()
        {
            // Arrange
            var product = CreateTestProduct();
            _mockProductRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(true);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "Title", "New Title");

            // Assert
            Assert.True(result);
            Assert.Equal("New Title", product.Title);
        }

        [Fact]
        public async Task UpdateProductPropertyAsync_WithInvalidProperty_ReturnsFalse()
        {
            // Arrange
            var product = CreateTestProduct();
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            // Act
            var result = await service.UpdateProductPropertyAsync(product, "NonExistentProp", "value");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetPaginationProducts_WithSearchFilter_FiltersCorrectly()
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
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SearchBy = "Title", Search = "Laptop" };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(2, count);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetPaginationProducts_WithSortFilter_SortsCorrectly()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateTestProduct(1, "C Product") ,
                CreateTestProduct(2, "A Product"),
                CreateTestProduct(3, "B Product")
            };
            products[0].Price = 30f;
            products[1].Price = 10f;
            products[2].Price = 20f;

            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            var paginationFilter = new PaginationFilter(1, 10);
            var queryFilter = new QueryFilter { SortBy = "Price", IsSortAscending = true };

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(3, count);
            Assert.Equal(10f, result[0].Price);
        }

        [Fact]
        public void GetPaginationProducts_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var products = Enumerable.Range(1, 15)
                .Select(i => CreateTestProduct(i, $"Product {i}"))
                .ToList();
            var mockCollection = new MockDocumentCollection<Product>(products);
            _mockProductRepository.Setup(r => r.GetAll()).Returns(mockCollection);
            var service = new ProductServiceTestable(_mockUnitOfWork.Object);

            var paginationFilter = new PaginationFilter(2, 5);
            var queryFilter = new QueryFilter();

            // Act
            var result = service.GetPaginationProducts(paginationFilter, queryFilter, out int count);

            // Assert
            Assert.Equal(15, count);
            Assert.Equal(5, result.Count);
        }
    }

    // Testable wrapper for ProductService
    public class ProductServiceTestable : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductServiceTestable(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<Product> GetAllProducts()
        {
            return _unitOfWork.Products.GetAll().AsQueryable().ToList();
        }

        public int GetNextProductId()
        {
            return _unitOfWork.Products.GetAll().GetNextIdValue();
        }

        public List<Product> GetPaginationProducts(PaginationFilter paginationFilter, QueryFilter queryFilter, out int queryProductCount)
        {
            var products = GetAllProducts();
            products = QueryHelper.SearchObjects(products, queryFilter.SearchBy, queryFilter.Search, StringComparison.OrdinalIgnoreCase).ToList();
            products = QueryHelper.SortObjects(products, queryFilter.SortBy, queryFilter.IsSortAscending).ToList();
            queryProductCount = products.Count;
            return products
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public Product? GetProductById(int id) => _unitOfWork.Products.GetById(id);

        public async Task<bool> InsertProductAsync(Product product) => await _unitOfWork.Products.InsertAsync(product);

        public async Task<bool> UpdateProductAsync(Product product) => await _unitOfWork.Products.UpdateAsync(product);

        public async Task<bool> UpdateProductPropertyAsync<T>(Product product, string property, T value)
        {
            var propertyInfo = typeof(Product).GetProperty(
                property,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
            );
            if (propertyInfo == null) return false;
            propertyInfo.SetValue(product, value, null);
            return await UpdateProductAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var result = await _unitOfWork.Products.DeleteAsync(id);
            _unitOfWork.CommitTransaction();
            return result;
        }

        public void Dispose() => _unitOfWork.Dispose();
    }
}
