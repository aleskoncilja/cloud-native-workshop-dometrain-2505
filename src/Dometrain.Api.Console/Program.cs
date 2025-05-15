using Dometrain.Monolith.Api.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateApplicationBuilder();

host.Services.AddDometrainApi("http://localhost:5148", "ThisIsAlsoMeantToBeSecret");

var app = host.Build();
try
{
    var coursesClient = app.Services.GetRequiredService<ICoursesApiClient>();
    var studentsClient = app.Services.GetRequiredService<IStudentsApiClient>();

    var course = await coursesClient.GetAsync("c1d09acd-c43d-49b3-946b-a02562a084e2");
    var student = await studentsClient.GetAsync("005d25b1-bfc8-4391-b349-6cec00d1416c");

    var shoppingCartClient = app.Services.GetRequiredService<IShoppingCartsApiClient>();
    var cart = await shoppingCartClient.GetAsync();

}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex);
}
Console.WriteLine();
