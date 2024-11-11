/*
 * This file is part of Maven Version Checker <https://github.com/StevenJDH/maven-version-checker>.
 * Copyright (C) 2024 Steven Jenkins De Haro.
 *
 * Maven Version Checker is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Maven Version Checker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Maven Version Checker.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Net;
using MavenVersionChecker.Action.Data;
using MavenVersionChecker.Action.Services;
using Microsoft.Extensions.DependencyInjection;
using MavenVersionChecker.Action.Chaos;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Simmy;
using Polly.Simmy.Fault;
using Polly.Simmy.Latency;
using Polly.Simmy.Outcomes;
using System.Text.Json.Serialization;

namespace MavenVersionChecker.Action.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitHubActionServices(this IServiceCollection services)
    {
        services.AddSingleton<ActionInputs>();
        services.TryAddSingleton<IChaosManager, ChaosManager>();
        services.AddSingleton<MavenVersionCheckerProcessor>();
        
        var httpClientBuilder = services.AddHttpClient("MavenApiClient", c =>
        {
            c.BaseAddress = new Uri("https://search.maven.org");
            c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; rv:132.0) Gecko/20100101 Firefox/132.0");
        });
        httpClientBuilder.AddStandardResilienceHandler();
        httpClientBuilder.AddResilienceHandler("chaos", (builder, context) =>
        {
            var chaosManager = context.ServiceProvider.GetRequiredService<IChaosManager>();

            builder
                .AddConcurrencyLimiter(10, 100)
                .AddChaosLatency(new ChaosLatencyStrategyOptions
                {
                    EnabledGenerator = args => chaosManager.IsChaosEnabledAsync(args.Context),
                    InjectionRateGenerator = args => chaosManager.GetInjectionRateAsync(args.Context),
                    Latency = TimeSpan.FromSeconds(5)
                })

                .AddChaosFault(new ChaosFaultStrategyOptions
                {
                    EnabledGenerator = args => chaosManager.IsChaosEnabledAsync(args.Context),
                    InjectionRateGenerator = args => chaosManager.GetInjectionRateAsync(args.Context),
                    FaultGenerator = new FaultGenerator().AddException(() => 
                        new InvalidOperationException("Injected by chaos fault strategy!"))
                })
                .AddChaosOutcome(new ChaosOutcomeStrategyOptions<HttpResponseMessage>
                {
                    EnabledGenerator = args => chaosManager.IsChaosEnabledAsync(args.Context),
                    InjectionRateGenerator = args => chaosManager.GetInjectionRateAsync(args.Context),
                    OutcomeGenerator = new OutcomeGenerator<HttpResponseMessage>().AddResult(() => 
                        new HttpResponseMessage(HttpStatusCode.InternalServerError))
                });
        });

        services.AddSingleton<IMavenApiService, MavenApiService>();

        // Removes the noisy HttpClient info logs added when using the extension.
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

        return services;
    }
}

/// <summary>
/// Utility class for JSON (de)serialization source generation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
[JsonSerializable(typeof(MavenQueryResponse))]
internal partial class SourceGenerationContext : JsonSerializerContext { }