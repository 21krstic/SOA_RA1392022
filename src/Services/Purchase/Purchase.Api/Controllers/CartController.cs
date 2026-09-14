using Microsoft.AspNetCore.Mvc;
using Purchase.Api.Data;
using Purchase.Api.Models;
using Purchase.Api.Services;

namespace Purchase.Api.Controllers;

public record AddCartItemRequest(string TourId, string TourName, decimal Price);

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly CartsRepository _carts;
    private readonly PurchaseTokensRepository _tokens;
    private readonly TourServiceClient _tourClient;

    public CartController(CartsRepository carts, PurchaseTokensRepository tokens, TourServiceClient tourClient)
    {
        _carts = carts;
        _tokens = tokens;
        _tourClient = tourClient;
    }

    [HttpGet("{touristId}")]
    public async Task<ActionResult<ShoppingCart>> GetCart(string touristId) =>
        Ok(await _carts.GetOrCreateAsync(touristId));

    [HttpPost("{touristId}/items")]
    public async Task<ActionResult<ShoppingCart>> AddItem(string touristId, AddCartItemRequest request)
    {
        var tour = await _tourClient.GetTourAsync(request.TourId);
        if (tour is null) return NotFound("Tour not found.");
        if (tour.Status != "Published") return BadRequest("Only published tours can be purchased.");

        var cart = await _carts.GetOrCreateAsync(touristId);

        if (cart.Items.Any(i => i.TourId == request.TourId))
            return Conflict("Tour is already in the cart.");

        cart.Items.Add(new CartItem { TourId = request.TourId, TourName = request.TourName, Price = request.Price });
        await _carts.ReplaceAsync(cart);
        return Ok(cart);
    }

    [HttpDelete("{touristId}/items/{tourId}")]
    public async Task<ActionResult<ShoppingCart>> RemoveItem(string touristId, string tourId)
    {
        var cart = await _carts.GetOrCreateAsync(touristId);
        cart.Items.RemoveAll(i => i.TourId == tourId);
        await _carts.ReplaceAsync(cart);
        return Ok(cart);
    }

    [HttpPost("{touristId}/checkout")]
    public async Task<ActionResult<List<PurchaseToken>>> Checkout(string touristId)
    {
        var cart = await _carts.GetOrCreateAsync(touristId);
        if (cart.Items.Count == 0) return BadRequest("Cart is empty.");

        var tokens = cart.Items.Select(item => new PurchaseToken
        {
            TouristId = touristId,
            TourId = item.TourId,
            TourName = item.TourName,
            PricePaid = item.Price
        }).ToList();

        await _tokens.InsertManyAsync(tokens);
        await _carts.ClearAsync(touristId);

        return Ok(tokens);
    }
}
