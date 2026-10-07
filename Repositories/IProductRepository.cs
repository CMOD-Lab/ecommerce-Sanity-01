using EcommerceWebApi.Entities;

namespace EcommerceWebApi.Repositories
{
    public interface IProductRepository
    {
        Task<bool> DeleteAsync(int id);
        IQueryable<Product> GetAll();
        Product? GetById(int id);
        Task<bool> InsertAsync(Product product);
        Task<bool> UpdateAsync(Product product);
    }
}
