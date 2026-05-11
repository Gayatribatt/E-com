using EcommerceApp.Data;
using EcommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync(string? search = null, int? categoryId = null, string sortBy = "name", int page = 1, int pageSize = 12);
        Task<int> GetCountAsync(string? search = null, int? categoryId = null);
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetFeaturedAsync(int count = 6);
        Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8);
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Product>> GetAllAsync(string? search = null, int? categoryId = null, string sortBy = "name", int page = 1, int pageSize = 12)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> GetCountAsync(string? search = null, int? categoryId = null)
        {
            var query = _context.Products.Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            return await query.CountAsync();
        }

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.Include(p => p.Category).Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> GetFeaturedAsync(int count = 6) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsActive && p.IsFeatured).Take(count).ToListAsync();

        public async Task<IEnumerable<Product>> GetNewArrivalsAsync(int count = 8) =>
            await _context.Products.Include(p => p.Category).Where(p => p.IsActive).OrderByDescending(p => p.CreatedAt).Take(count).ToListAsync();

        public async Task<IEnumerable<Category>> GetCategoriesAsync() =>
            await _context.Categories.Include(c => c.Products).ToListAsync();

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }

    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string? userId, string sessionId);
        Task AddToCartAsync(string? userId, string sessionId, int productId, int quantity = 1);
        Task UpdateQuantityAsync(string? userId, string sessionId, int cartItemId, int quantity);
        Task RemoveFromCartAsync(string? userId, string sessionId, int cartItemId);
        Task ClearCartAsync(string? userId, string sessionId);
        Task MergeCartsAsync(string userId, string sessionId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context) => _context = context;

        public async Task<Cart> GetOrCreateCartAsync(string? userId, string sessionId)
        {
            Cart? cart = null;

            if (!string.IsNullOrEmpty(userId))
                cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

            cart ??= await _context.Carts.Include(c => c.CartItems).ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, SessionId = sessionId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task AddToCartAsync(string? userId, string sessionId, int productId, int quantity = 1)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var existing = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = productId, Quantity = quantity });

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string? userId, string sessionId, int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return;

            if (quantity <= 0)
                _context.CartItems.Remove(item);
            else
                item.Quantity = quantity;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string? userId, string sessionId, int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string? userId, string sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
        }

        public async Task MergeCartsAsync(string userId, string sessionId)
        {
            var sessionCart = await _context.Carts.Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.UserId == null);

            if (sessionCart == null) return;

            var userCart = await GetOrCreateCartAsync(userId, sessionId);

            foreach (var item in sessionCart.CartItems)
            {
                var existing = userCart.CartItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existing != null)
                    existing.Quantity += item.Quantity;
                else
                    userCart.CartItems.Add(new CartItem { CartId = userCart.Id, ProductId = item.ProductId, Quantity = item.Quantity });
            }

            _context.Carts.Remove(sessionCart);
            await _context.SaveChangesAsync();
        }
    }

    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, CheckoutViewModel model);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id, string userId);
        Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<Order> CreateOrderAsync(string userId, CheckoutViewModel model)
        {
            var cart = await _cartService.GetOrCreateCartAsync(userId, string.Empty);

            if (!cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var subtotal = cart.CartItems.Sum(i => i.Subtotal);
            var shipping = subtotal >= 50 ? 0 : 9.99m;
            var tax = subtotal * 0.08m;

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                SubTotal = subtotal,
                ShippingCost = shipping,
                Tax = tax,
                Total = subtotal + shipping + tax,
                ShippingFirstName = model.FirstName,
                ShippingLastName = model.LastName,
                ShippingAddress = model.Address,
                ShippingCity = model.City,
                ShippingState = model.State,
                ShippingZipCode = model.ZipCode,
                ShippingCountry = model.Country,
                Notes = model.Notes,
                OrderItems = cart.CartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product?.Price ?? 0
                }).ToList()
            };

            _context.Orders.Add(order);

            // Update stock
            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.StockQuantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId, string.Empty);

            return order;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId) =>
            await _context.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToListAsync();

        public async Task<Order?> GetOrderByIdAsync(int id, string userId) =>
            await _context.Orders.Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;
            order.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
