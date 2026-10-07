using EcommerceWebApi.Data;
using EcommerceWebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWebApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Product> GetAll()
        {
            try
            {
                return _context.Products.AsQueryable();
            }
            catch
            {
                throw;
            }
        }

        public Product? GetById(int id)
        {
            try
            {
                return _context.Products.FirstOrDefault(x => x.Id == id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> InsertAsync(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            try
            {
                _context.Products.Update(product);
                return await Task.FromResult(true);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return false;
                _context.Products.Remove(product);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
