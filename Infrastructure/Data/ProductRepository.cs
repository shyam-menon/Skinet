using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly StoreContext _context;
        private readonly ILogger _logger;

        public ProductRepository(StoreContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductBrand>> GetProductBrandsAsync()
        {
            _logger.LogInformation("Inside the repository to call GetProductBrands");
            return await _context.ProductBrands.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            _logger.LogInformation($"Inside the repository to call GetProduct Brand by ID :{id}");
            return await _context.Products
                 .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                    .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            _logger.LogInformation("Inside the repository to call GetProducts");
            return await _context.Products
                .Include(p => p.ProductType)
                    .Include(p => p.ProductBrand)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProductType>> GetProductTypesAsync()
        {
            _logger.LogInformation("Inside the repository to call GetProductTypes");
            return await _context.ProductTypes.ToListAsync();
        }
    }
}