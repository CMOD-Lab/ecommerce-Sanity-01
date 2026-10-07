using EcommerceWebApi.Data;
using EcommerceWebApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWebApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Order> GetAll()
        {
            try
            {
                return _context.Orders.AsQueryable();
            }
            catch
            {
                throw;
            }
        }

        public Order? GetById(string id)
        {
            try
            {
                return _context.Orders.FirstOrDefault(x => x.Id == id);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> InsertAsync(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Order order)
        {
            try
            {
                _context.Orders.Update(order);
                return await Task.FromResult(true);
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null) return false;
                _context.Orders.Remove(order);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
