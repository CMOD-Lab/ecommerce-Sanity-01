using System;
using Xunit;
using EcommerceWebApi.Filters;

namespace EcommerceWebApi.Filters.Tests
{
    public class AllowAnonymousAttributeTests
    {
        [Fact]
        public void AllowAnonymousAttribute_CanBeInstantiated()
        {
            // Arrange & Act
            var attribute = new AllowAnonymousAttribute();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void AllowAnonymousAttribute_IsAttribute()
        {
            // Arrange & Act
            var attribute = new AllowAnonymousAttribute();

            // Assert
            Assert.IsAssignableFrom<Attribute>(attribute);
        }
    }

    public class AllowFirstFactorAttributeTests
    {
        [Fact]
        public void AllowFirstFactorAttribute_CanBeInstantiated()
        {
            // Arrange & Act
            var attribute = new AllowFirstFactorAttribute();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void AllowFirstFactorAttribute_IsAttribute()
        {
            // Arrange & Act
            var attribute = new AllowFirstFactorAttribute();

            // Assert
            Assert.IsAssignableFrom<Attribute>(attribute);
        }
    }

    public class AuthorizeRoleAttributeTests
    {
        [Fact]
        public void AuthorizeRoleAttribute_Constructor_SetsRole()
        {
            // Arrange & Act
            var attribute = new AuthorizeRoleAttribute("Admin");

            // Assert
            Assert.Equal("Admin", attribute.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_Constructor_WithUserRole_SetsRole()
        {
            // Arrange & Act
            var attribute = new AuthorizeRoleAttribute("User");

            // Assert
            Assert.Equal("User", attribute.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_Constructor_WithEmptyRole_SetsEmptyRole()
        {
            // Arrange & Act
            var attribute = new AuthorizeRoleAttribute(string.Empty);

            // Assert
            Assert.Equal(string.Empty, attribute.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_IsAttribute()
        {
            // Arrange & Act
            var attribute = new AuthorizeRoleAttribute("Admin");

            // Assert
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        [InlineData("Manager")]
        [InlineData("SuperAdmin")]
        public void AuthorizeRoleAttribute_WithVariousRoles_SetsCorrectRole(string role)
        {
            // Arrange & Act
            var attribute = new AuthorizeRoleAttribute(role);

            // Assert
            Assert.Equal(role, attribute.Role);
        }
    }
}
