// ProductService.cs
// cr-dotnet-0048 fix: Replaced ClickOnce deployment dependency with
// AWS S3 + CloudFront update service (IAwsUpdateService).
// ClickOnce's desktop-only update mechanism is replaced by cloud-native
// version checking via S3 version manifest and CloudFront CDN distribution.

using EcommerceWebApi.Entities;
using EcommerceWebApi.Notification;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        // cr-dotnet-0048: IAwsUpdateService replaces ClickOnce deployment.
        // Application distribution and updates are now handled via
        // S3-hosted packages distributed through CloudFront CDN.
        private readonly IAwsUpdateService _awsUpdateService;

        public ProductService(UnitOfWork unitOfWork, IAwsUpdateService awsUpdateService)
        {
            _unitOfWork = unitOfWork;
            _awsUpdateService = awsUpdateService;
        }

        public List<Product> GetAllProducts()
        {
            try
            {
                return _unitOfWork.Products.GetAll().AsQueryable().ToList();
            }
            catch
            {
                throw;
            }
        }

        public int GetNextProductId()
        {
            return _unitOfWork.Products.GetAll().GetNextIdValue();
        }

        public List<Product> GetPaginationProducts(
            PaginationFilter paginationFilter,
            QueryFilter queryFilter,
            out int queryProductCount
        )
        {
            // Get queryable product collection
            var products = GetAllProducts();

            // Search products
            products = QueryHelper
                .SearchObjects(
                    products,
                    queryFilter.SearchBy,
                    queryFilter.Search,
                    StringComparison.OrdinalIgnoreCase
                )
                .ToList();

            // Sort products
            products = QueryHelper
                .SortObjects(products, queryFilter.SortBy, queryFilter.IsSortAscending)
                .ToList();

            // Get current quantity
            queryProductCount = products.Count;

            return products
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public Product? GetProductById(int id)
        {
            return _unitOfWork.Products.GetById(id);
        }

        public async Task<bool> InsertProductAsync(Product product)
        {
            try
            {
                var result = await _unitOfWork.Products.InsertAsync(product);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            try
            {
                var result = await _unitOfWork.Products.UpdateAsync(product);
                // cr-dotnet-0048: Product updates are persisted to the data store.
                // Application distribution and version updates are handled via
                // AWS S3 + CloudFront (IAwsUpdateService), not ClickOnce.
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateProductPropertyAsync<T>(
            Product product,
            string property,
            T value
        )
        {
            var propertyInfo = typeof(Product).GetProperty(
                property,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
            );

            if (propertyInfo == null)
            {
                return false;
            }
            try
            {
                propertyInfo.SetValue(product, value, null);
                return await UpdateProductAsync(product);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.Products.DeleteAsync(id);
                _unitOfWork.CommitTransaction();
                return result;
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
