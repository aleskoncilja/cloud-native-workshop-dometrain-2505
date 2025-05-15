namespace Dometrain.Cart.Api.Contracts.Carts;

public record ShoppingCartRequest(Guid StudentId, List<Guid> CourseIds);
