namespace Dometrain.Monolith.Api.Contracts.Carts;

public class ShoppingCartResponse
{
    public required Guid StudentId { get; set; }

    public List<Guid> CourseIds { get; set; } = [];
}
