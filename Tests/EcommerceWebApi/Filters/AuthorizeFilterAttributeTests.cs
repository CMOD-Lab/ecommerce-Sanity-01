using System;
using Xunit;
using EcommerceWebApi.Filters;

namespace EcommerceWebApi.Tests.Filters
{
    public class AuthorizeFilterAttributeTests
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
        public void AllowFirstFactorAttribute_CanBeInstantiated()
        {
            // Arrange & Act
            var attr = new AllowFirstFactorAttribute();

            // Assert
            Assert.NotNull(attr);
        }

        [Fact]
        public void AuthorizeRoleAttribute_WithRole_SetsRoleCorrectly()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("Admin");

            // Assert
            Assert.Equal("Admin", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_WithUserRole_SetsRoleCorrectly()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("User");

            // Assert
            Assert.Equal("User", attr.Role);
        }

        [Fact]
        public void AuthorizeRoleAttribute_WithEmptyRole_SetsEmptyRole()
        {
            // Arrange & Act
            var attr = new AuthorizeRoleAttribute("");

            // Assert
            Assert.Equal("", attr.Role);
        }

        [Fact]
        public void AllowAnonymousAttribute_IsAttribute()
        {
            // Assert
            Assert.True(typeof(AllowAnonymousAttribute).IsSubclassOf(typeof(Attribute)));
        }

        [Fact]
        public void AllowFirstFactorAttribute_IsAttribute()
        {
            // Assert
            Assert.True(typeof(AllowFirstFactorAttribute).IsSubclassOf(typeof(Attribute)));
        }

        [Fact]
        public void AuthorizeRoleAttribute_IsAttribute()
        {
            // Assert
            Assert.True(typeof(AuthorizeRoleAttribute).IsSubclassOf(typeof(Attribute)));
        }

        [Fact]
        public void AuthorizeRoleAttribute_AllowsMultiple_IsTrue()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
                typeof(AuthorizeRoleAttribute),
                typeof(AttributeUsageAttribute)
            );

            // Assert
            Assert.NotNull(usageAttr);
            Assert.True(usageAttr.AllowMultiple);
        }

        [Fact]
        public void AllowAnonymousAttribute_AttributeUsage_TargetsMethod()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
                typeof(AllowAnonymousAttribute),
                typeof(AttributeUsageAttribute)
            );

            // Assert
            Assert.NotNull(usageAttr);
            Assert.True((usageAttr.ValidOn & AttributeTargets.Method) != 0);
        }

        [Fact]
        public void AllowFirstFactorAttribute_AttributeUsage_TargetsMethod()
        {
            // Arrange
            var usageAttr = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
                typeof(AllowFirstFactorAttribute),
                typeof(AttributeUsageAttribute)
            );

            // Assert
            Assert.NotNull(usageAttr);
            Assert.True((usageAttr.ValidOn & AttributeTargets.Method) != 0);
        }
    }
}
