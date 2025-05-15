using Dometrain.Monolith.Api.Contracts.Carts;
using Dometrain.Monolith.Api.ShoppingCarts;

namespace Dometrain.Monolith.Api.Students;

public static class ShoppingCartMapper
{
    public static ShoppingCart MapToStudent(this ShoppingCartRequest request)
    {
        return new ShoppingCart()
        {
            StudentId = request.StudentId,
            CourseIds = request.CourseIds
        };
    }

    public static ShoppingCartResponse? MapToResponse(this ShoppingCart? cart)
    {
        if (cart is null)
        {
            return null;
        }

        return new ShoppingCartResponse
        {
            StudentId = cart.StudentId,
            CourseIds = cart.CourseIds
        };
    }
}
