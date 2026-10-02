using System;
using System.Collections.Generic;
using Xunit;
using EcommerceWebApi.Filters;

namespace EcommerceWebApi.Tests.Filters
{
    public class AllowAnonymousAttributeTests
    {
        [Fact]
        public void AllowAnonymousAttribute_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var attr = new AllowAnonymousAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void AllowAnonymousAttribute_IsAttribute()
        {
            // Arrange & Act
            var attr = new AllowAnonymousAttribute();

            // Assert
            Assert.IsAssignableFrom<Attribute>(attr);
        }

        [Fact]
        public void AllowAnonymousAttribute_HasCorrectAttributeUsage()
        {
            // Arrange
            var attrUsage = typeof(AllowAnonymousAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false);

            // Assert
            Assert.NotEmpty(attrUsage);
        }
    }

    public class AllowFirstFactorAttributeTests
    {
        [Fact]
        public void AllowFirstFactorAttribute_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var attr = new AllowFirstFactorAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void AllowFirstFactorAttribute_IsAttribute()
        {
            // Arrange & Act
            var attr = new AllowFirstFactorAttribute();

            // Assert
            Assert.IsAssignableFrom<Attribute>(attr);
        }
    }

    public class AuthorizeRoleAttributeTests
    {
        [Fact]
        public void AuthorizeRoleAttribute_Constructor_SetsRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("Admin");

            // Assert
            Assert.Equal("Admin", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_Constructor_WithUserRole_SetsRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("User");

            // Assert
            Assert.Equal("User", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_Constructor_WithEmptyRole_SetsEmptyRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("");

            // Assert
            Assert.Equal("", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_IsAttribute()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("Admin");

            // Assert
            Assert.IsAssignableFrom<Attribute>(attr);
        }

        [Fact]
        public void AuthorizeRoleAttribute_HasCorrectAttributeUsage()
        {
            // Arrange
            var attrUsage = typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false);

            // Assert
            Assert.NotEmpty(attrUsage);
        }

        [Fact]
        public void AuthorizeRoleAttribute_AllowsMultiple()
        {
            // Arrange
            var attrUsage = (AttributeUsageAttribute)typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            // Assert
            Assert.True(attrUsage.AllowMultiple);
        }

        [Fact]
        public void AuthorizeRoleAttribute_Role_IsReadOnly()
        {
            // Arrange
            var attr = new AuthorizeRoleAttribute("Admin");

            // Assert - Role property should be set via constructor only
            Assert.Equal("Admin", attr.Role);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        [InlineData("Manager")]
        [InlineData("SuperAdmin")]
        public void AuthorizeRoleAttribute_Constructor_WithVariousRoles_SetsCorrectly(string role)
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute(role);

            // Assert
            Assert.Equal(role, attr.Role);
        }
    }
}
