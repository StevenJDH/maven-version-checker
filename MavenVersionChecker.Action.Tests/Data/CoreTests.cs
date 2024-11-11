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

// Ignore Spelling: Env

using MavenVersionChecker.Action.Data;
using static MavenVersionChecker.Action.Data.Core;

namespace MavenVersionChecker.Action.Tests.Data;

[TestFixture]
public class CoreTests
{
    private readonly string _outputFile = Path.Combine(Directory.GetCurrentDirectory(), "output.txt");
    private readonly string _envFile = Path.Combine(Directory.GetCurrentDirectory(), "env.txt");
    private readonly string _summaryFile = Path.Combine(Directory.GetCurrentDirectory(), "summary.txt");

    [Test, Description("Should contain only expected statuses for exit codes.")]
    public void Should_ContainOnlyExpectedStatuses_ForExitCodes()
    {
        ExitCode[] expectedExitCodes = [ExitCode.Success, ExitCode.Failure];

        var exitCodes = Enum.GetValues(typeof(ExitCode));

        Assert.That(exitCodes, Is.EquivalentTo(expectedExitCodes));
    }

    [Test, Description("Should match correct exit code for each status.")]
    public void Should_MatchCorrectExitCode_ForEachStatus()
    {
        Assert.Multiple(() =>
        {
            Assert.That((int)ExitCode.Success, Is.EqualTo(0));
            Assert.That((int)ExitCode.Failure, Is.EqualTo(1));
        });
    }

    [Test, Description("Should return value when requested optional by default input is defined.")]
    public void Should_ReturnValue_When_RequestedOptionalByDefaultInputIsDefined()
    {
        Environment.SetEnvironmentVariable("INPUT_EXPORT-VARIABLES", "true");

        string input = Core.GetInput("export-variables");

        Assert.That(input, Is.EqualTo("true"));

        Environment.SetEnvironmentVariable("INPUT_EXPORT-VARIABLES", null);
    }

    [Test, Description("Should return empty value when requested optional input is not defined.")]
    public void Should_ReturnEmptyValue_When_RequestedOptionalInputIsNotDefined()
    {
        string input = Core.GetInput("foobar", new InputOptions(Required: false));

        Assert.That(input, Is.Empty);
    }

    [Test, Description("Should throw ArgumentNullException when requested required input is not defined.")]
    public void Should_ThrowArgumentNullException_When_RequestedRequiredInputIsNotDefined()
    {
        const string expectedMessage = "Required input not supplied. (Parameter 'foobar')";

        Assert.That(() => Core.GetInput("foobar", new InputOptions(Required: true)), Throws.ArgumentNullException
            .With.Message.EqualTo(expectedMessage));
    }

    [TestCase("  true  ", true, "true")]
    [TestCase("  true  ", false, "  true  ")]
    [TestCase(null, true, "")]
    [TestCase(null, false, "")]
    [Description("Should conditionally trim value when setting TrimWhitespace option.")]
    public void Should_ConditionallyTrimValue_When_SettingTrimWhitespaceOption(string? value, bool trimWhitespace, string expectedResult)
    {
        Environment.SetEnvironmentVariable("INPUT_EXPORT-VARIABLES", value);

        string input = Core.GetInput("export-variables", new InputOptions(TrimWhitespace: trimWhitespace));

        Assert.That(input, Is.EqualTo(expectedResult));

        Environment.SetEnvironmentVariable("INPUT_EXPORT-VARIABLES", null);
    }

    [Test, Description("Should set GitHub Output file for provided key value pairs.")]
    public async Task Should_SetGitHubOutputFile_ForProvidedKeyValuePairs()
    {
        Environment.SetEnvironmentVariable("GITHUB_OUTPUT", _outputFile);
        await File.Create(_outputFile).DisposeAsync();
        IReadOnlyList<string> expectedRows = ["test", "foobar"];
        string expectedOutputData = $"test=foobar{Environment.NewLine}";

        var result = await Core.SetOutputAsync(" test ", " foobar ");
        string outputData = await File.ReadAllTextAsync(_outputFile);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedRows));
            Assert.That(outputData, Is.EqualTo(expectedOutputData));
        });

        Environment.SetEnvironmentVariable("GITHUB_OUTPUT", null);
        File.Delete(_outputFile);
    }

    [Test, Description("Should throw ArgumentException when GitHub Output file path is not defined.")]
    public void Should_ThrowArgumentException_When_GitHubOutputFilePathIsNotDefined()
    {
        Environment.SetEnvironmentVariable("GITHUB_OUTPUT", null);
        const string expectedMessage = "Unable to find environment variable for file command OUTPUT (GITHUB_OUTPUT).";

        Assert.That(async () => await Core.SetOutputAsync("test", "foobar"), Throws.ArgumentException
            .With.Message.EqualTo(expectedMessage));
    }

    [Test, Description("Should throw FileNotFoundException when GitHub Output file is missing.")]
    public void Should_ThrowFileNotFoundException_When_GitHubOutputFileIsMissing()
    {
        Environment.SetEnvironmentVariable("GITHUB_OUTPUT", _outputFile);
        string expectedMessage = $"Missing file at path: '{_outputFile}' for file command OUTPUT (GITHUB_OUTPUT).";

        Assert.That(async () => await Core.SetOutputAsync("test", "foobar"), Throws.TypeOf<FileNotFoundException>()
            .With.Message.EqualTo(expectedMessage));
    }

    [Test, Description("Should set GitHub Env file for provided key value pairs.")]
    public async Task Should_SetGitHubEnvFile_ForProvidedKeyValuePairs()
    {
        Environment.SetEnvironmentVariable("GITHUB_ENV", _envFile);
        await File.Create(_envFile).DisposeAsync();
        IReadOnlyList<string> expectedRows = ["test", "foobar"];
        string expectedEnvData = $"test=foobar{Environment.NewLine}";

        var result = await Core.ExportVariableAsync(" test ", " foobar ");
        string envData = await File.ReadAllTextAsync(_envFile);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedRows));
            Assert.That(envData, Is.EqualTo(expectedEnvData));
        });

        Environment.SetEnvironmentVariable("GITHUB_ENV", null);
        File.Delete(_envFile);
    }

    [Test, Description("Should throw ArgumentException when GitHub Env file path is not defined.")]
    public void Should_ThrowArgumentException_When_GitHubEnvFilePathIsNotDefined()
    {
        Environment.SetEnvironmentVariable("GITHUB_ENV", null);
        const string expectedMessage = "Unable to find environment variable for file command ENV (GITHUB_ENV).";

        Assert.That(async () => await Core.ExportVariableAsync("test", "foobar"), Throws.ArgumentException
            .With.Message.EqualTo(expectedMessage));
    }

    [Test, Description("Should throw FileNotFoundException when GitHub Env file is missing.")]
    public void Should_ThrowFileNotFoundException_When_GitHubEnvFileIsMissing()
    {
        Environment.SetEnvironmentVariable("GITHUB_ENV", _envFile);
        string expectedMessage = $"Missing file at path: '{_envFile}' for file command ENV (GITHUB_ENV).";

        Assert.That(async () => await Core.ExportVariableAsync("test", "foobar"), Throws.TypeOf<FileNotFoundException>()
            .With.Message.EqualTo(expectedMessage));
    }

    [Test, Description("Should set GitHub Step Summary file for provided markdown content.")]
    public async Task Should_SetGitHubStepSummaryFile_ForProvidedMarkdownContent()
    {
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", _summaryFile);
        await File.Create(_summaryFile).DisposeAsync();
        string expectedSummaryData = $"foobar{Environment.NewLine}";

        await Core.SetStepSummaryAsync(" foobar ");
        string summaryData = await File.ReadAllTextAsync(_summaryFile);

        Assert.That(summaryData, Is.EqualTo(expectedSummaryData));

        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", null);
        File.Delete(_summaryFile);
    }

    [Test, Description("Should throw ArgumentException when GitHub Step Summary file path is not defined.")]
    public void Should_ThrowArgumentException_When_GitHubStepSummaryFilePathIsNotDefined()
    {
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", null);
        const string expectedMessage = "Unable to find environment variable for file command STEP_SUMMARY (GITHUB_STEP_SUMMARY).";

        Assert.That(async () => await Core.SetStepSummaryAsync("foobar"), Throws.ArgumentException
            .With.Message.EqualTo(expectedMessage));
    }

    [Test, Description("Should throw FileNotFoundException when GitHub Step Summary file is missing.")]
    public void Should_ThrowFileNotFoundException_When_GitHubStepSummaryFileIsMissing()
    {
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", _summaryFile);
        string expectedMessage = $"Missing file at path: '{_summaryFile}' for file command STEP_SUMMARY (GITHUB_STEP_SUMMARY).";

        Assert.That(async () => await Core.SetStepSummaryAsync("foobar"), Throws.TypeOf<FileNotFoundException>()
            .With.Message.EqualTo(expectedMessage));
    }
}