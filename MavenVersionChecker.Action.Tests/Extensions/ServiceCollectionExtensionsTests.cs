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

// Ignore Spelling: Api

using System.Text.Json;
using MavenVersionChecker.Action.Chaos;
using MavenVersionChecker.Action.Data;
using MavenVersionChecker.Action.Extensions;
using MavenVersionChecker.Action.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace MavenVersionChecker.Action.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test, Description("Should add action inputs for dependency injection.")]
    public void Should_AddActionInputs_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();

        var inputs = provider.GetService<ActionInputs>();

        Assert.Multiple(() =>
        {
            Assert.That(inputs, Is.Not.Null);
            Assert.That(inputs, Is.InstanceOf<ActionInputs>());
        });
    }

    [Test, Description("Should add chaos manager for dependency injection.")]
    public void Should_AddChaosManager_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();

        var chaosManager = provider.GetService<IChaosManager>();

        Assert.Multiple(() =>
        {
            Assert.That(chaosManager, Is.Not.Null);
            Assert.That(chaosManager, Is.InstanceOf<IChaosManager>());
        });
    }

    [Test, Description("Should add standard resilience handler options for dependency injection.")]
    public void Should_AddStandardResilienceHandlerOptions_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();
        var expectedAttemptTimeout = TimeSpan.FromSeconds(10);
        var expectedTotalRequestTimeout = TimeSpan.FromSeconds(30);
        const int expectedRateLimiterPermitLimit = 1000;
        const int expectedRateLimiterQueueLimit = 0;
        const int expectedRetryMaxRetryAttempts = 3;
        const DelayBackoffType expectedRetryBackoffType = DelayBackoffType.Exponential;
        var expectedCbBreakDuration = TimeSpan.FromSeconds(5);
        const double expectedCbFailureRatio = 0.1;
        var expectedCbSamplingDuration = TimeSpan.FromSeconds(30);
        const int expectedCbMinimumThroughput = 100;

        var monitor = provider.GetRequiredService<IOptionsMonitor<HttpStandardResilienceOptions>>();
        var options = monitor.Get("MavenApiClient-standard"); // Calculated as {httpClientName}-{pipelineIdentifier}

        Assert.Multiple(() =>
        {
            Assert.That(monitor, Is.Not.Null);
            Assert.That(monitor, Is.InstanceOf<IOptionsMonitor<HttpStandardResilienceOptions>>());
            Assert.That(options, Is.Not.Null);
            Assert.That(options, Is.InstanceOf<HttpStandardResilienceOptions>());
            Assert.That(options.AttemptTimeout.Timeout, Is.EqualTo(expectedAttemptTimeout));
            Assert.That(options.TotalRequestTimeout.Timeout, Is.EqualTo(expectedTotalRequestTimeout));
            Assert.That(options.RateLimiter.DefaultRateLimiterOptions.PermitLimit, Is.EqualTo(expectedRateLimiterPermitLimit));
            Assert.That(options.RateLimiter.DefaultRateLimiterOptions.QueueLimit, Is.EqualTo(expectedRateLimiterQueueLimit));
            Assert.That(options.Retry.UseJitter, Is.True);
            Assert.That(options.Retry.MaxRetryAttempts, Is.EqualTo(expectedRetryMaxRetryAttempts));
            Assert.That(options.Retry.BackoffType, Is.EqualTo(expectedRetryBackoffType));
            Assert.That(options.CircuitBreaker.BreakDuration, Is.EqualTo(expectedCbBreakDuration));
            Assert.That(options.CircuitBreaker.FailureRatio, Is.EqualTo(expectedCbFailureRatio));
            Assert.That(options.CircuitBreaker.SamplingDuration, Is.EqualTo(expectedCbSamplingDuration));
            Assert.That(options.CircuitBreaker.MinimumThroughput, Is.EqualTo(expectedCbMinimumThroughput));
        });
    }

    [Test, Description("Should add http client factory and configured keyed http client for dependency injection.")]
    public void Should_AddHttpClientFactoryAndConfiguredKeyedHttpClient_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();
        var expectedBaseAddress = new Uri("https://search.maven.org");
        const string expectedUserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:132.0) Gecko/20100101 Firefox/132.0";


        var clientFactory = provider.GetService<IHttpClientFactory>();
        var client = clientFactory!.CreateClient("MavenApiClient");

        Assert.Multiple(() =>
        {
            Assert.That(clientFactory, Is.Not.Null);
            Assert.That(clientFactory, Is.InstanceOf<IHttpClientFactory>());
            Assert.That(client, Is.Not.Null);
            Assert.That(client, Is.InstanceOf<HttpClient>());
            Assert.That(client.BaseAddress, Is.EqualTo(expectedBaseAddress));
            Assert.That(string.Join(" ", client.DefaultRequestHeaders.UserAgent), Is.EqualTo(expectedUserAgent));

        });
    }

    [Test, Description("Should add maven api service for dependency injection.")]
    public void Should_AddMavenApiService_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();

        var service = provider.GetService<IMavenApiService>();

        Assert.Multiple(() =>
        {
            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.InstanceOf<IMavenApiService>());
        });
    }

    [Test, Description("Should add maven version checker processor for dependency injection.")]
    public void Should_AddMavenVersionCheckerProcessor_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddLogging()
            .AddGitHubActionServices()
            .BuildServiceProvider();

        var processor = provider.GetService<MavenVersionCheckerProcessor>();

        Assert.Multiple(() =>
        {
            Assert.That(processor, Is.Not.Null);
            Assert.That(processor, Is.InstanceOf<MavenVersionCheckerProcessor>());
        });
    }

    [Test, Description("Should remove http message handler builder filter for dependency injection.")]
    public void Should_RemoveHttpMessageHandlerBuilderFilter_ForDependencyInjection()
    {
        var services = new ServiceCollection();
        var provider = services
            .AddGitHubActionServices()
            .BuildServiceProvider();

        var messageHandlerBuilderFilter = provider.GetService<IHttpMessageHandlerBuilderFilter>();

        Assert.Multiple(() =>
        {
            Assert.That(messageHandlerBuilderFilter, Is.Null);
        });
    }

    [Test, Description("Should use SourceGenerationContext for serializing and deserializing maven query response.")]
    public void Should_UseSourceGenerationContext_ForSerializingAndDeserializingMavenQueryResponse()
    {
        var mavenQueryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "999.9.9" }]
            }
        };

        string json = JsonSerializer.Serialize(mavenQueryResponse, SourceGenerationContext.Default.MavenQueryResponse);
        var model = JsonSerializer.Deserialize<MavenQueryResponse>(json, SourceGenerationContext.Default.MavenQueryResponse);

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model!.Result.NumberFound, Is.EqualTo(1));
            Assert.That(model!.Result.Artifacts, Has.Count.EqualTo(1));
            Assert.That(model!.Result.Artifacts[0].LatestVersion, Is.EqualTo("999.9.9"));
        });
    }

    [Test, Description("Should use SourceGenerationContext for serializing and deserializing dictionaries.")]
    public void Should_UseSourceGenerationContext_ForSerializingAndDeserializingDictionaries()
    {
        const string expectedKey = "dependencies";
        const string expectedValue = "foo:bar:2.0.0";
        var dictionary = new Dictionary<string, List<string>>
        {
            { expectedKey, [expectedValue] }
        };

        string json = JsonSerializer.Serialize(dictionary, SourceGenerationContext.Default.DictionaryStringListString);
        var model = JsonSerializer.Deserialize(json, SourceGenerationContext.Default.DictionaryStringListString);

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model!, Contains.Key(expectedKey));
            Assert.That(model!.GetValueOrDefault(expectedKey), Has.Count.EqualTo(1));
            Assert.That(model!.GetValueOrDefault(expectedKey)![0], Is.EqualTo(expectedValue));
        });
    }
}