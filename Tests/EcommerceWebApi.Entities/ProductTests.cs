using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Entities.Tests
{
    public class ProductTests
    {
        [Fact]
        public void Product_DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.Equal(0, product.Id);
            Assert.Equal(0f, product.Price);
            Assert.Equal(0f, product.Rating);
            Assert.Equal(0, product.Quantity);
        }

        [Fact]
        public void Product_SetTitle_ReturnsCorrectTitle()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Title = "Test Product";

            // Assert
            Assert.Equal("Test Product", product.Title);
        }

        [Fact]
        public void Product_SetPrice_ReturnsCorrectPrice()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Price = 99.99f;

            // Assert
            Assert.Equal(99.99f, product.Price);
        }

        [Fact]
        public void Product_SetRating_ReturnsCorrectRating()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Rating = 4.5f;

            // Assert
            Assert.Equal(4.5f, product.Rating);
        }

        [Fact]
        public void Product_SetBrand_ReturnsCorrectBrand()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Brand = "TestBrand";

            // Assert
            Assert.Equal("TestBrand", product.Brand);
        }

        [Fact]
        public void Product_SetCategory_ReturnsCorrectCategory()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Category = "Electronics";

            // Assert
            Assert.Equal("Electronics", product.Category);
        }

        [Fact]
        public void Product_SetQuantity_ReturnsCorrectQuantity()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Quantity = 100;

            // Assert
            Assert.Equal(100, product.Quantity);
        }

        [Fact]
        public void Product_SetThumbnail_ReturnsCorrectUri()
        {
            // Arrange
            var product = new Product();
            var uri = new Uri("https://example.com/image.jpg");

            // Act
            product.Thumbnail = uri;

            // Assert
            Assert.Equal(uri, product.Thumbnail);
        }

        [Fact]
        public void Product_SetId_ReturnsCorrectId()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 42;

            // Assert
            Assert.Equal(42, product.Id);
        }

        [Fact]
        public void Product_FullyPopulated_AllPropertiesCorrect()
        {
            // Arrange & Act
            var product = new Product
            {
                Id = 1,
                Title = "Laptop",
                Price = 999.99f,
                Rating = 4.8f,
                Brand = "Dell",
                Category = "Electronics",
                Thumbnail = new Uri("https://example.com/laptop.jpg"),
                Quantity = 50
            };

            // Assert
            Assert.Equal(1, product.Id);
            Assert.Equal("Laptop", product.Title);
            Assert.Equal(999.99f, product.Price);
            Assert.Equal(4.8f, product.Rating);
            Assert.Equal("Dell", product.Brand);
            Assert.Equal("Electronics", product.Category);
            Assert.NotNull(product.Thumbnail);
            Assert.Equal(50, product.Quantity);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(2.5f)]
        [InlineData(5f)]
        public void Product_RatingBoundaryValues_AcceptsValidRatings(float rating)
        {
            // Arrange
            var product = new Product();

            // Act
            product.Rating = rating;

            // Assert
            Assert.Equal(rating, product.Rating);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1000)]
        public void Product_QuantityBoundaryValues_AcceptsValidQuantities(int quantity)
        {
            // Arrange
            var product = new Product();

            // Act
            product.Quantity = quantity;

            // Assert
            Assert.Equal(quantity, product.Quantity);
        }
    }
}
