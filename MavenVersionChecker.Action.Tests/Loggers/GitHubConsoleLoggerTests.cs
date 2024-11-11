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

// Ignore Spelling: Workflow Maskable

using MavenVersionChecker.Action.Extensions;
using MavenVersionChecker.Action.Loggers;
using Microsoft.Extensions.Logging;

namespace MavenVersionChecker.Action.Tests.Loggers;

[TestFixture]
public class GitHubConsoleLoggerTests
{
    private readonly TextWriter _originalConsoleOut = Console.Out;
    private StringWriter _sw = null!;
    private readonly GitHubConsoleLogger _logger = new();

    [SetUp]
    public void SetUp()
    {
        _sw = new StringWriter();
        Console.SetOut(_sw);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalConsoleOut);
        _sw.Dispose();
    }

    [Test, Description("Should output trace log with prefix for GitHub workflow command.")]
    public void Should_OutputTraceLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogTrace("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::debug::Trace: foobar"));
    }

    [Test, Description("Should output debug log with prefix for GitHub workflow command.")]
    public void Should_OutputDebugLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogDebug("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::debug::foobar"));
    }

    [Test, Description("Should output maskable phrase with prefix for GitHub workflow command.")]
    public void Should_OutputMaskablePhraseWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.AddMask("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::add-mask::foobar"));
    }

    [Test, Description("Should output log group title with prefix for GitHub workflow command.")]
    public void Should_OutputLogGroupTitleWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogGroupStart("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::group::foobar"));
    }

    [Test, Description("Should output log group ending indicator for GitHub workflow command.")]
    public void Should_OutputLogGroupEndingIndicator_ForGitHubWorkflowCommand()
    {
        _logger.LogGroupEnd();

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::endgroup::"));
    }

    [Test, Description("Should output notice log with prefix for GitHub workflow command.")]
    public void Should_OutputNoticeLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogNotice("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::notice::foobar"));
    }

    [Test, Description("Should output plain log without any prefixes for info log level.")]
    public void Should_OutputPlainLogWithoutAnyPrefixes_ForInfoLogLevel()
    {
        _logger.LogInformation("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("foobar"));
    }

    [Test, Description("Should output warning log with prefix for GitHub workflow command.")]
    public void Should_OutputWarningLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogWarning("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::warning::foobar"));
    }

    [Test, Description("Should output error log with prefix for GitHub workflow command.")]
    public void Should_OutputErrorLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogError("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::error::foobar"));
    }

    [Test, Description("Should output critical log with prefix for GitHub workflow command.")]
    public void Should_OutputCriticalLogWithPrefix_ForGitHubWorkflowCommand()
    {
        _logger.LogCritical("foobar");

        string output = _sw.ToString();
        Assert.That(output, Does.StartWith("::error::Critical: foobar"));
    }

    [Test, Description("Should log nothing when log level is none.")]
    public void Should_LogNothing_When_LogLevelIsNone()
    {
        _logger.Log(LogLevel.None, "foobar");

        string output = _sw.ToString();
        Assert.That(output, Is.Empty);
    }

    [Test, Description("Should indicate log level is disabled when set to none.")]
    public void Should_IndicateLogLevelIsDisabled_When_SetToNone()
    {
        bool result = _logger.IsEnabled(LogLevel.None);

        Assert.That(result, Is.False);
    }

    [TestCase(LogLevel.Trace)]
    [TestCase(LogLevel.Debug)]
    [TestCase(LogLevel.Information)]
    [TestCase(LogLevel.Warning)]
    [TestCase(LogLevel.Error)]
    [TestCase(LogLevel.Critical)]
    [Description("Should indicate log level is enabled when not set to none.")]
    public void Should_IndicateLogLevelIsEnabled_When_NotSetToNone(LogLevel logLevel)
    {
        bool result = _logger.IsEnabled(logLevel);

        Assert.That(result, Is.True);
    }

    [Test, Description("Should return null for unimplemented logical operation scope.")]
    public void Should_ReturnNull_ForUnimplementedLogicalOperationScope()
    {
        IDisposable? result = _logger.BeginScope("Test");

        Assert.That(result, Is.Null);
    }
}