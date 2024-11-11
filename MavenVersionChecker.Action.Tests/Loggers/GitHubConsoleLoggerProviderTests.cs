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

using MavenVersionChecker.Action.Extensions;
using MavenVersionChecker.Action.Loggers;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MavenVersionChecker.Action.Tests.Loggers;

[TestFixture]
public class GitHubConsoleLoggerProviderTests
{
    [Test, Description("Should create custom log entry when custom provider is configured.")]
    public async Task Should_CreateCustomLogEntry_When_CustomProviderIsConfigured()
    {
        var originalConsoleOut = Console.Out;
        await using var sw = new StringWriter();
        Console.SetOut(sw);
        using var loggerFactory = LoggerFactory.Create(c => c.AddGitHubConsoleLogger());
        var logger = loggerFactory.CreateLogger("Test");

        logger.LogWarning("Hello World!");

        string output = sw.ToString();
        Assert.That(output, Contains.Substring("::warning::Hello World!"));

        Console.SetOut(originalConsoleOut);
    }

    [Test, Description("Should return ILogger instance when provider creates GitHubConsoleLogger.")]
    public void Should_ReturnILoggerInstance_When_ProviderCreatesGitHubConsoleLogger()
    {
        var provider = new GitHubConsoleLoggerProvider();

        var logger = provider.CreateLogger("Test");

        Assert.That(logger, Is.InstanceOf<GitHubConsoleLogger>());
        Assert.That(logger, Is.AssignableTo<ILogger>());
    }

    [Test, Description("Should return same ILogger instance for same category name.")]
    public void Should_ReturnSameILoggerInstance_ForSameCategoryName()
    {
        var provider = new GitHubConsoleLoggerProvider();
        string categoryName = "Test";

        var loggerA = provider.CreateLogger(categoryName);
        var loggerB = provider.CreateLogger(categoryName);

        Assert.That(loggerA, Is.SameAs(loggerB));
    }

    [Test, Description("Should return same ILogger instance for same category name using different casing.")]
    public void Should_ReturnSameILoggerInstance_ForSameCategoryNameUsingDifferentCasing()
    {
        var provider = new GitHubConsoleLoggerProvider();

        var loggerA = provider.CreateLogger("Test");
        var loggerB = provider.CreateLogger("TEST");

        Assert.That(loggerA, Is.SameAs(loggerB));
    }

    [Test, Description("Should return new ILogger instance for different category name.")]
    public void Should_ReturnNewILoggerInstance_ForDifferentCategoryName()
    {
        var provider = new GitHubConsoleLoggerProvider();

        var loggerA = provider.CreateLogger("Test1");
        var loggerB = provider.CreateLogger("Test2");

        Assert.That(loggerA, Is.Not.SameAs(loggerB));
    }

    [Test, Description("Should return new ILogger instance when same category name is disposed.")]
    public void Should_ReturnNewILoggerInstance_When_SameCategoryNameIsDisposed()
    {
        var provider = new GitHubConsoleLoggerProvider();
        string categoryName = "Test";

        var loggerA = provider.CreateLogger(categoryName);
        provider.Dispose();
        var loggerB = provider.CreateLogger(categoryName);

        Assert.That(loggerA, Is.Not.SameAs(loggerB));
    }
}