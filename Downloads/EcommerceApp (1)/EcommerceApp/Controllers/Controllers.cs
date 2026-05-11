using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController(IProductService productService) => _productService = productService;

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                FeaturedProducts = await _productService.GetFeaturedAsync(6),
                Categories = await _productService.GetCategoriesAsync(),
                NewArrivals = await _productService.GetNewArrivalsAsync(4)
            };
            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }

    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private const int PageSize = 12;

        public ProductsController(IProductService productService) => _productService = productService;

        public async Task<IActionResult> Index(string? search, int? categoryId, string sortBy = "name", int page = 1)
        {
            var total = await _productService.GetCountAsync(search, categoryId);
            var vm = new ProductListViewModel
            {
                Products = await _productService.GetAllAsync(search, categoryId, sortBy, page, PageSize),
                Categories = await _productService.GetCategoriesAsync(),
                SearchTerm = search,
                CategoryId = categoryId,
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)total / PageSize),
                TotalProducts = total
            };
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }
    }

    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        private string GetSessionId()
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _cartService.GetOrCreateCartAsync(userId, GetSessionId());
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            await _cartService.AddToCartAsync(userId, GetSessionId(), productId, quantity);
            TempData["Success"] = "Item added to cart!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            await _cartService.UpdateQuantityAsync(userId, GetSessionId(), cartItemId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);
            await _cartService.RemoveFromCartAsync(userId, GetSessionId(), cartItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User);
            await _cartService.ClearCartAsync(userId, GetSessionId());
            return RedirectToAction("Index");
        }
    }

    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ICartService cartService, IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        private string GetSessionId() => HttpContext.Session.GetString("CartSessionId") ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var cart = await _cartService.GetOrCreateCartAsync(userId, GetSessionId());

            if (!cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            var user = await _userManager.GetUserAsync(User);
            var vm = new CheckoutViewModel
            {
                Cart = cart,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                City = user?.City ?? string.Empty,
                State = user?.State ?? string.Empty,
                ZipCode = user?.ZipCode ?? string.Empty,
                Country = user?.Country ?? "US"
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;

            if (!ModelState.IsValid)
            {
                model.Cart = await _cartService.GetOrCreateCartAsync(userId, GetSessionId());
                return View("Index", model);
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, model);
                TempData["Success"] = $"Order #{order.OrderNumber} placed successfully!";
                return RedirectToAction("Confirmation", new { id = order.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Cart = await _cartService.GetOrCreateCartAsync(userId, GetSessionId());
                return View("Index", model);
            }
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderByIdAsync(id, userId);
            if (order == null) return NotFound();
            return View(order);
        }
    }

    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderByIdAsync(id, userId);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
