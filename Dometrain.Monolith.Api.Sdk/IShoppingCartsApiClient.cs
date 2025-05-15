using Dometrain.Monolith.Api.Contracts.Carts;
using Refit;

namespace Dometrain.Monolith.Api.Sdk;

public interface IShoppingCartsApiClient
{
    [Get("/cart/me")]
    Task<ShoppingCartResponse?> GetAsync();
}