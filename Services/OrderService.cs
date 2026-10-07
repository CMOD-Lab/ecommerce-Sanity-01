using EcommerceWebApi.Entities;
using EcommerceWebApi.Utilities;
using System.Reflection;

namespace EcommerceWebApi.Services
{
    /// <summary>
    /// Handles order business logic.
    /// Application distribution and update checking is handled via AWS S3 + CloudFront
    /// through <see cref="IApplicationUpdateService"/>, replacing the former ClickOnce
    /// deployment model.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductService _productService;
        // Replaces ClickOnce ApplicationDeployment.CurrentDeployment update checks:
        // version checking and package distribution are now handled by S3 + CloudFront.
        private readonly IApplicationUpdateService _updateService;

        public OrderService(UnitOfWork unitOfWork, ProductService productService,
            IApplicationUpdateService updateService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _updateService = updateService;
        }

        public enum OrderResult
        {
            Success,
            Fail,
            QuantityNotEnough,
            ProductNotFound,
            InvalidInput
        }

        private async Task<Order?> ConstructOrderAsync(
            string userId,
            Dictionary<int, int> productList
        )
        {
            var products = new Dictionary<int, Product>();
            foreach (var pair in productList)
            {
                var product = _productService.GetProductById(pair.Key);
                if (product == null || product.Quantity < pair.Value)
                {
                    return null;
                }
                products.Add(pair.Key, product);
            }

            Order order =
                new()
                {
                    UserId = userId,
                    ProductList = productList,
                    // cr-dotnet-0121: Replaced DateTime.Now with DateTimeOffset.UtcNow for cloud-safe UTC timestamps
                    Created = DateTimeOffset.UtcNow.UtcDateTime,
                    Status = OrderStatus.Pending
                };

            foreach (var pair in productList)
            {
                var product = products[pair.Key];
                product.Quantity -= pair.Value;
                await _productService.UpdateProductAsync(product);
            }

            return order;
        }

        private async Task<OrderResult> RefillProductAsync(Order order)
        {
            foreach (var pair in order.ProductList)
            {
                var product = _productService.GetProductById(pair.Key);
                if (product == null)
                {
                    return OrderResult.ProductNotFound;
                }
                product.Quantity += pair.Value;
                await _productService.UpdateProductAsync(product);
            }
            return OrderResult.Success;
        }

        public List<Order> GetAllOrders()
        {
            try
            {
                return _unitOfWork.Orders.GetAll().AsQueryable().ToList();
            }
            catch
            {
                throw;
            }
        }

        public List<Order> GetPaginationOrders(
            PaginationFilter paginationFilter,
            QueryFilter queryFilter,
            out int queryOrderCount
        )
        {
            var orders = GetAllOrders();

            orders = QueryHelper
                .SearchObjects(
                    orders,
                    queryFilter.SearchBy,
                    queryFilter.Search,
                    StringComparison.OrdinalIgnoreCase
                )
                .ToList();

            queryOrderCount = orders.Count;

            return orders
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();
        }

        public Order? GetOrderById(string id)
        {
            return _unitOfWork.Orders.GetById(id);
        }

        public async Task<OrderResult> InsertOrderAsync(
            string userId,
            Dictionary<int, int> productList
        )
        {
            try
            {
                _unitOfWork.StartTransaction();
                var order = await ConstructOrderAsync(userId, productList);
                if (order == null)
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.ProductNotFound;
                }
                var result = await _unitOfWork.Orders.InsertAsync(order);

                if (result)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        public async Task<OrderResult> UpdateOrderAsync(Order order)
        {
            try
            {
                var result = await _unitOfWork.Orders.UpdateAsync(order);
                return result ? OrderResult.Success : OrderResult.Fail;
            }
            catch
            {
                throw;
            }
        }

        public async Task<OrderResult> UpdateOrderPropertyAsync<T>(
            Order order,
            string property,
            T value
        )
        {
            var propertyInfo = typeof(Order).GetProperty(
                property,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
            );

            if (propertyInfo == null)
            {
                return OrderResult.InvalidInput;
            }
            try
            {
                propertyInfo.SetValue(order, value, null);
                return await UpdateOrderAsync(order);
            }
            catch
            {
                throw;
            }
        }

        public async Task<OrderResult> UpdateOrderStatusAsync(Order order, OrderStatus status)
        {
            try
            {
                _unitOfWork.StartTransaction();
                if (order.Status == status)
                {
                    _unitOfWork.AbortTransaction();
                    throw new InvalidOperationException($"Current status already is {status}");
                }
                order.Status = status;
                // cr-dotnet-0121: Replaced DateTime.Now with DateTimeOffset.UtcNow for cloud-safe UTC timestamps
                order.Updated = DateTimeOffset.UtcNow.UtcDateTime;
                if (status == OrderStatus.Canceled)
                {
                    var fillResult = await RefillProductAsync(order);
                    if (fillResult != OrderResult.Success)
                    {
                        _unitOfWork.AbortTransaction();
                        return fillResult;
                    }
                }
                var result = await UpdateOrderAsync(order);

                if (result == OrderResult.Success)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        public async Task<OrderResult> DeleteOrderAsync(string id)
        {
            try
            {
                _unitOfWork.StartTransaction();
                var order = GetOrderById(id);
                if (order == null)
                {
                    return OrderResult.ProductNotFound;
                }

                if (order.Status != OrderStatus.Canceled)
                {
                    var fillResult = await RefillProductAsync(order);
                    if (fillResult != OrderResult.Success)
                    {
                        _unitOfWork.AbortTransaction();
                        return fillResult;
                    }
                }

                var deleteResult = await _unitOfWork.Orders.DeleteAsync(id);
                if (deleteResult)
                {
                    _unitOfWork.CommitTransaction();
                    return OrderResult.Success;
                }
                else
                {
                    _unitOfWork.AbortTransaction();
                    return OrderResult.Fail;
                }
            }
            catch
            {
                _unitOfWork.AbortTransaction();
                throw;
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
