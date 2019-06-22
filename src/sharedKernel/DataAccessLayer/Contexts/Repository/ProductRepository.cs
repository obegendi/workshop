using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Contexts.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataAccessLayer.Contexts.Repository
{
    public class ProductRepository
    {
        private readonly ProductContext _context;

        public ProductRepository(IOptions<Settings> settings)
        {
            _context = new ProductContext(settings);
        }

        public async Task<IEnumerable<Product>> GetAll()
        {
            try
            {
                return await _context.Products
                    .Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<Product> Get(int id)
        {
            try
            {
                return await _context.Products
                    .Find(product => product.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task Add(Product item)
        {
            try
            {
                await _context.Products.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
