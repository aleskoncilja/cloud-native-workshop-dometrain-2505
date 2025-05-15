using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using System.Net;

namespace Dometrain.Monolith.Api.Sdk;

public static class Extensions
{
    public static IServiceCollection AddDometrainApi(this IServiceCollection services, string baseUrl, string apiKey)
    {
        services.AddRefitClient<IStudentsApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                c.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }).AddResilienceHandler("smart-retry", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetryAttempts = 2,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = static args => ValueTask.FromResult(args is
                    {
                        Outcome.Result.StatusCode:
                        HttpStatusCode.RequestTimeout or
                        HttpStatusCode.TooManyRequests
                    })
                });
            });

        services.AddRefitClient<ICoursesApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                c.DefaultRequestHeaders.Add("x-api-key", apiKey);
            })
            .AddStandardResilienceHandler();

        services.AddRefitClient<IShoppingCartsApiClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseUrl);
                c.DefaultRequestHeaders.Add("x-api-key", apiKey);
            })
            .AddStandardResilienceHandler();

        services.AddHttpClient("dometrain-api", c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.DefaultRequestHeaders.Add("x-api-key", apiKey);
        });

        return services;
    }
}
