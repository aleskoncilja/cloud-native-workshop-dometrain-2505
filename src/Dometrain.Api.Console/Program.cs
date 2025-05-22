using Dometrain.Monolith.Api.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateApplicationBuilder();

host.Services.AddDometrainApi("http://localhost:5148", "ThisIsAlsoMeantToBeSecret");

var app = host.Build();

var coursesClient = app.Services.GetRequiredService<ICoursesApiClient>();
var studentsClient = app.Services.GetRequiredService<IStudentsApiClient>();

var course = await coursesClient.GetAsync("5d795c87-cb7a-4bbd-beed-cf9a168d620d");
var student = await studentsClient.GetAsync("eee280d3-3d1c-4cd2-8a9b-86357c5a6245");

Console.WriteLine();
