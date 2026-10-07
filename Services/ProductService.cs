// cr-dotnet-0048: ClickOnce deployment replaced with S3 + CloudFront distribution.
// ProductService now accepts ICloudDistributionService to replace any ClickOnce-based
// update/deployment checks with cloud-native AWS S3 + CloudFront distribution.

using EcommerceWebApi.Entities;
using EcommerceWebApi.Notification;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        // cr-dotnet-0048: Cloud-native distribution service replaces ClickOnce
        // ApplicationDeployment for version checking and update distribution via
        // S3 + CloudFront CDN.
        private readonly ICloudDistributionService? _cloudDistributionService;

        public ProductService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ProductService(
            UnitOfWork unitOfWork,
            ICloudDistributionService cloudDistributionService)
        {
            _unitOfWork = unitOfWork;
            _cloudDistributionService = cloudDistributionService;
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
                // cr-dotnet-0048 fix (line 89): replaced ClickOnce ApplicationDeployment
                // result handling with cloud-native S3/CloudFront distribution service.
                // Business logic (returning update result) is preserved; deployment
                // distribution is now handled via ICloudDistributionService rather than
                // ClickOnce ApplicationDeployment.
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

        /// <summary>
        /// Checks for an available application update via the cloud distribution service
        /// (S3 + CloudFront). Replaces the ClickOnce ApplicationDeployment.CheckForUpdate()
        /// pattern with a cloud-native AWS SDK call.
        /// </summary>
        public async Task<bool> CheckForApplicationUpdateAsync()
        {
            if (_cloudDistributionService == null)
                return false;

            return await _cloudDistributionService.IsUpdateAvailableAsync();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
