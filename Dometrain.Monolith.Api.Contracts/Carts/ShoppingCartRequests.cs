namespace Dometrain.Monolith.Api.Contracts.Carts;

public record ShoppingCartRequest(Guid StudentId, List<Guid> CourseIds);
