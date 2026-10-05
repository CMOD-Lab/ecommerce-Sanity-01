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
            var usageAttr = typeof(AllowAnonymousAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false);

            // Assert
            Assert.NotEmpty(usageAttr);
        }
    }

    public class AllowFirstFactorAttributeTests
    {
        [Fact]
        public void AllowFirstFactorAttribute_CanBeInstantiated()
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
        public void AuthorizeRoleAttribute_Constructor_UserRole_SetsRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("User");

            // Assert
            Assert.Equal("User", attr.Role);
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
        public void AuthorizeRoleAttribute_EmptyRole_SetsEmptyRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("");

            // Assert
            Assert.Equal("", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_HasCorrectAttributeUsage()
        {
            // Arrange
            var usageAttr = typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false);

            // Assert
            Assert.NotEmpty(usageAttr);
        }

        [Fact]
        public void AuthorizeRoleAttribute_AllowMultiple_IsTrue()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute)typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            // Assert
            Assert.True(usageAttr.AllowMultiple);
        }

        [Fact]
        public void AuthorizeRoleAttribute_CanApplyToClass()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute)typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            // Assert
            Assert.True((usageAttr.ValidOn & AttributeTargets.Class) != 0);
        }

        [Fact]
        public void AuthorizeRoleAttribute_CanApplyToMethod()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute)typeof(AuthorizeRoleAttribute)
                .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            // Assert
            Assert.True((usageAttr.ValidOn & AttributeTargets.Method) != 0);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        [InlineData("Manager")]
        [InlineData("SuperAdmin")]
        public void AuthorizeRoleAttribute_VariousRoles_SetsCorrectly(string role)
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute(role);

            // Assert
            Assert.Equal(role, attr.Role);
        }
    }
}
