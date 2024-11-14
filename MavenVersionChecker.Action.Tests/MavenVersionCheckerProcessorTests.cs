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

// Ignore Spelling: Pom Json

using MavenVersionChecker.Action.Data;
using MavenVersionChecker.Action.Services;
using MavenVersionChecker.Action.Tests.Support;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static MavenVersionChecker.Action.Data.Core;

namespace MavenVersionChecker.Action.Tests;

[TestFixture]
public class MavenVersionCheckerProcessorTests
{
    // ReSharper disable InconsistentNaming
    private const string OUTPUT_FILE = "./Locals/output.txt";
    private const string SUMMARY_FILE = "./Locals/summary.txt";
    private const string SINGLE_PROJECT = "./../../../Sample/Single/pom.xml";
    private const string MULTI_PROJECT = "./../../../Sample/Multi/pom.xml";
    private const string EXPECTED_MULTI_MODULE_DETECTED_MESSAGE = "Multi-Module project detected.";
    // ReSharper restore InconsistentNaming

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Environment.SetEnvironmentVariable("GITHUB_OUTPUT", OUTPUT_FILE);
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", SUMMARY_FILE);
    }

    [SetUp]
    public void SetUp()
    {
        File.Create(OUTPUT_FILE).Dispose();
        File.Create(SUMMARY_FILE).Dispose();
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("INPUT_LOCATION", null);
        File.Delete(SUMMARY_FILE);
        File.Delete(OUTPUT_FILE);
    }

    [Test, Description("Should process pom and set outputs for single module project.")]
    public async Task Should_ProcessPomAndSetOutputs_ForSingleModuleProject()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", SINGLE_PROJECT);
        var inputs = new ActionInputs();
        var mockLogger = new Mock<ILogger<MavenVersionCheckerProcessor>>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(mockLogger.Object, inputs, mockMavenApiService.Object);
        var queryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact {LatestVersion = "999.9.9"}]
            }
        };
        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedHasUpdates = "true";
        const string expectedNumberOfUpdates = "14";
        const string expectedUpdateJson = """
        {
          "parents": [
            "org.springframework.boot:spring-boot-starter-parent:999.9.9"
          ],
          "dependencies": [
            "io.awspring.cloud:spring-cloud-aws-dependencies:999.9.9",
            "org.springframework.cloud:spring-cloud-dependencies:999.9.9",
            "org.mongodb:mongodb-driver-core:999.9.9",
            "org.mongodb:mongodb-driver-sync:999.9.9",
            "org.mongodb:bson:999.9.9",
            "com.amazonaws:aws-java-sdk-sts:999.9.9",
            "co.elastic.logging:logback-ecs-encoder:999.9.9",
            "io.github.stevenjdh:jsonschema2pojo-fake-annotator:999.9.9"
          ],
          "plugins": [
            "org.apache.maven.plugins:maven-surefire-plugin:999.9.9",
            "org.apache.maven.plugins:maven-compiler-plugin:999.9.9",
            "io.github.git-commit-id:git-commit-id-maven-plugin:999.9.9",
            "org.jsonschema2pojo:jsonschema2pojo-maven-plugin:999.9.9",
            "org.jacoco:jacoco-maven-plugin:999.9.9"
          ]
        }
        """;

        mockMavenApiService.Setup(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(outputPairs.GetValueOrDefault("has_updates"), Is.EqualTo(expectedHasUpdates));
            Assert.That(outputPairs.GetValueOrDefault("number_of_updates"), Is.EqualTo(expectedNumberOfUpdates));
            Assert.That(outputPairs.GetValueOrDefault("update_json"), Is.EqualTo(expectedUpdateJson).IgnoreWhiteSpace);
        });
        mockLogger.VerifyLog(LogLevel.Information, Times.Never);
    }

    [Test, Description("Should process pom and set outputs for multi-module project.")]
    public async Task Should_ProcessPomAndSetOutputs_ForMultiModuleProject()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", MULTI_PROJECT);
        var inputs = new ActionInputs();
        var mockLogger = new Mock<ILogger<MavenVersionCheckerProcessor>>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(mockLogger.Object, inputs, mockMavenApiService.Object);
        var queryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "999.9.9" }]
            }
        };
        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedHasUpdates = "true";
        const string expectedNumberOfUpdates = "16";
        const string expectedUpdateJson = """
        {
          "parents": [
            "org.springframework.boot:spring-boot-starter-parent:999.9.9"
          ],
          "dependencies": [
            "io.awspring.cloud:spring-cloud-aws-dependencies:999.9.9",
            "org.springframework.cloud:spring-cloud-dependencies:999.9.9",
            "org.mongodb:mongodb-driver-core:999.9.9",
            "org.mongodb:mongodb-driver-sync:999.9.9",
            "org.mongodb:bson:999.9.9",
            "com.amazonaws:aws-java-sdk-sts:999.9.9",
            "co.elastic.logging:logback-ecs-encoder:999.9.9",
            "org.apache.commons:commons-csv:999.9.9",
            "com.h2database:h2:999.9.9",
            "io.github.stevenjdh:jsonschema2pojo-fake-annotator:999.9.9"
          ],
          "plugins": [
            "org.jacoco:jacoco-maven-plugin:999.9.9",
            "org.apache.maven.plugins:maven-surefire-plugin:999.9.9",
            "org.apache.maven.plugins:maven-compiler-plugin:999.9.9",
            "io.github.git-commit-id:git-commit-id-maven-plugin:999.9.9",
            "org.jsonschema2pojo:jsonschema2pojo-maven-plugin:999.9.9"
          ]
        }
        """;

        mockMavenApiService.Setup(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(outputPairs.GetValueOrDefault("has_updates"), Is.EqualTo(expectedHasUpdates));
            Assert.That(outputPairs.GetValueOrDefault("number_of_updates"), Is.EqualTo(expectedNumberOfUpdates));
            Assert.That(outputPairs.GetValueOrDefault("update_json"), Is.EqualTo(expectedUpdateJson).IgnoreWhiteSpace);
        });
        mockLogger.VerifyLog(LogLevel.Information, Times.Once, EXPECTED_MULTI_MODULE_DETECTED_MESSAGE);
    }

    [Test, Description("Should generate summary for single module project.")]
    public async Task Should_GenerateSummary_ForSingleModuleProject()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", SINGLE_PROJECT);
        var inputs = new ActionInputs();
        var logger = new NullLogger<MavenVersionCheckerProcessor>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(logger, inputs, mockMavenApiService.Object);
        var queryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "999.9.9" }]
            }
        };
        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedSummary = """
        ## Maven Version Checker Action
        
        Below is a list of all the checked artifacts and their update status.
        
        | Type | Artifact | Version | Update |
        | --- | --- | --- | --- |
        | Parent | org.springframework.boot:spring-boot-starter-parent | 2.7.18 | 999.9.9 |
        | Dependency | io.awspring.cloud:spring-cloud-aws-dependencies | 2.4.4 | 999.9.9 |
        | Dependency | org.springframework.cloud:spring-cloud-dependencies | 2021.0.9 | 999.9.9 |
        | Dependency | org.mongodb:mongodb-driver-core | 4.11.5 | 999.9.9 |
        | Dependency | org.mongodb:mongodb-driver-sync | 4.11.5 | 999.9.9 |
        | Dependency | org.mongodb:bson | 4.11.5 | 999.9.9 |
        | Dependency | com.amazonaws:aws-java-sdk-sts | 1.12.395 | 999.9.9 |
        | Dependency | co.elastic.logging:logback-ecs-encoder | 1.6.0 | 999.9.9 |
        | Dependency | io.github.stevenjdh:jsonschema2pojo-fake-annotator | 1.0.0-SNAPSHOT | 999.9.9 |
        | Plugin | org.apache.maven.plugins:maven-surefire-plugin | 2.22.2 | 999.9.9 |
        | Plugin | org.apache.maven.plugins:maven-compiler-plugin | 3.13.0 | 999.9.9 |
        | Plugin | io.github.git-commit-id:git-commit-id-maven-plugin | 5.0.0 | 999.9.9 |
        | Plugin | org.jsonschema2pojo:jsonschema2pojo-maven-plugin | 1.1.2 | 999.9.9 |
        | Plugin | org.jacoco:jacoco-maven-plugin | 0.8.9 | 999.9.9 |
        
        """;

        mockMavenApiService.Setup(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);
        string summary = await File.ReadAllTextAsync(SUMMARY_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(summary, Is.EqualTo(expectedSummary));
        });
    }

    [Test, Description("Should generate summary for multi-module project.")]
    public async Task Should_GenerateSummary_ForMultiModuleProject()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", MULTI_PROJECT);
        var inputs = new ActionInputs();
        var logger = new NullLogger<MavenVersionCheckerProcessor>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(logger, inputs, mockMavenApiService.Object);
        var queryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "999.9.9" }]
            }
        };
        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedSummary = """
        ## Maven Version Checker Action
        
        Below is a list of all the checked artifacts and their update status.
        
        ### parent-pom
        
        | Type | Artifact | Version | Update |
        | --- | --- | --- | --- |
        | Parent | org.springframework.boot:spring-boot-starter-parent | 2.7.18 | 999.9.9 |
        | Dependency | io.awspring.cloud:spring-cloud-aws-dependencies | 2.4.4 | 999.9.9 |
        | Dependency | org.springframework.cloud:spring-cloud-dependencies | 2021.0.9 | 999.9.9 |
        | Dependency | org.mongodb:mongodb-driver-core | 4.11.5 | 999.9.9 |
        | Dependency | org.mongodb:mongodb-driver-sync | 4.11.5 | 999.9.9 |
        | Dependency | org.mongodb:bson | 4.11.5 | 999.9.9 |
        | Dependency | com.amazonaws:aws-java-sdk-sts | 1.12.395 | 999.9.9 |
        | Dependency | co.elastic.logging:logback-ecs-encoder | 1.6.0 | 999.9.9 |
        | Plugin | org.jacoco:jacoco-maven-plugin | 0.8.9 | 999.9.9 |
        | Plugin | org.apache.maven.plugins:maven-surefire-plugin | 2.22.2 | 999.9.9 |
        | Plugin | org.apache.maven.plugins:maven-compiler-plugin | 3.13.0 | 999.9.9 |
        | Plugin | io.github.git-commit-id:git-commit-id-maven-plugin | 5.0.0 | 999.9.9 |
        
        ### foobar-a
        
        | Type | Artifact | Version | Update |
        | --- | --- | --- | --- |
        | Dependency | org.apache.commons:commons-csv | 1.9.0 | 999.9.9 |
        
        ### foobar-b
        
        | Type | Artifact | Version | Update |
        | --- | --- | --- | --- |
        | Dependency | com.h2database:h2 | 2.3.232 | 999.9.9 |
        
        ### foobar-shared-library
        
        | Type | Artifact | Version | Update |
        | --- | --- | --- | --- |
        | Dependency | io.github.stevenjdh:jsonschema2pojo-fake-annotator | 1.0.0-SNAPSHOT | 999.9.9 |
        | Plugin | org.jsonschema2pojo:jsonschema2pojo-maven-plugin | 1.1.2-override | 999.9.9 |
        
        """;

        mockMavenApiService.Setup(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);
        string summary = await File.ReadAllTextAsync(SUMMARY_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(summary, Is.EqualTo(expectedSummary));
        });
    }

    [Test, Description("Should return failure status when an exception is thrown.")]
    public async Task Should_ReturnFailureStatus_When_AnExceptionIsThrown()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", SINGLE_PROJECT);
        var inputs = new ActionInputs();
        var logger = new InMemoryLogger<MavenVersionCheckerProcessor>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(logger, inputs, mockMavenApiService.Object);

        const ExitCode expectedExitCode = ExitCode.Failure;
        const int expectedOutputPairs = 0;
        const int expectedNumberOfLogEntries = 1;
        const string expectedErrorMessage = "This is a test error message.";

        mockMavenApiService.Setup(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Throws(new HttpRequestException(expectedErrorMessage));

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);
        string summary = await File.ReadAllTextAsync(SUMMARY_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(summary, Is.EqualTo(string.Empty));
            Assert.That(logger.GetLogMessages(), Has.Count.EqualTo(expectedNumberOfLogEntries));
            Assert.That(logger.GetLogMessages(), Contains.Item((LogLevel.Error, expectedErrorMessage)));
        });
    }

    [Test, Description("Should only have artifact updates for update_json output.")]
    public async Task Should_OnlyHaveArtifactUpdates_ForUpdateJsonOutput()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", SINGLE_PROJECT);
        var inputs = new ActionInputs();
        var mockLogger = new Mock<ILogger<MavenVersionCheckerProcessor>>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(mockLogger.Object, inputs, mockMavenApiService.Object);
        var parentQueryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "2.7.18" }]
            }
        };
        var queryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 1,
                Artifacts = [new Artifact { LatestVersion = "999.9.9" }]
            }
        };
        var noResultsQueryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 0,
                Artifacts = []
            }
        };
        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedHasUpdates = "true";
        const string expectedNumberOfUpdates = "12";
        const string expectedUpdateJson = """
        {
          "dependencies": [
            "io.awspring.cloud:spring-cloud-aws-dependencies:999.9.9",
            "org.springframework.cloud:spring-cloud-dependencies:999.9.9",
            "org.mongodb:mongodb-driver-core:999.9.9",
            "org.mongodb:mongodb-driver-sync:999.9.9",
            "org.mongodb:bson:999.9.9",
            "com.amazonaws:aws-java-sdk-sts:999.9.9",
            "co.elastic.logging:logback-ecs-encoder:999.9.9",
            "io.github.stevenjdh:jsonschema2pojo-fake-annotator:999.9.9"
          ],
          "plugins": [
            "org.apache.maven.plugins:maven-surefire-plugin:999.9.9",
            "org.apache.maven.plugins:maven-compiler-plugin:999.9.9",
            "io.github.git-commit-id:git-commit-id-maven-plugin:999.9.9",
            "org.jsonschema2pojo:jsonschema2pojo-maven-plugin:999.9.9"
          ]
        }
        """;

        mockMavenApiService.SetupSequence(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentQueryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(queryResponse)
            .ReturnsAsync(noResultsQueryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(outputPairs.GetValueOrDefault("has_updates"), Is.EqualTo(expectedHasUpdates));
            Assert.That(outputPairs.GetValueOrDefault("number_of_updates"), Is.EqualTo(expectedNumberOfUpdates));
            Assert.That(outputPairs.GetValueOrDefault("update_json"), Is.EqualTo(expectedUpdateJson).IgnoreWhiteSpace);
        });
        mockLogger.VerifyLog(LogLevel.Information, Times.Never);
    }

    [Test, Description("Should set outputs correctly when no updates are available.")]
    public async Task Should_SetOutputsCorrectly_When_NoUpdatesAreAvailable()
    {
        // Arrange
        Environment.SetEnvironmentVariable("INPUT_LOCATION", SINGLE_PROJECT);
        var inputs = new ActionInputs();
        var mockLogger = new Mock<ILogger<MavenVersionCheckerProcessor>>();
        var mockMavenApiService = new Mock<IMavenApiService>();
        var processor = new MavenVersionCheckerProcessor(mockLogger.Object, inputs, mockMavenApiService.Object);
        var noResultsQueryResponse = new MavenQueryResponse
        {
            Result = new QueryResult
            {
                NumberFound = 0,
                Artifacts = []
            }
        };

        const ExitCode expectedExitCode = ExitCode.Success;
        const int expectedOutputPairs = 3;
        const string expectedHasUpdates = "false";
        const string expectedNumberOfUpdates = "0";
        const string expectedUpdateJson = "{}";

        mockMavenApiService.SetupSequence(x => x.QueryLatestVersionAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse)
            .ReturnsAsync(noResultsQueryResponse);

        // Act
        var exitCode = await processor.StartProcessingPomAsync();
        var outputPairs = LoadPairs(OUTPUT_FILE);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(expectedExitCode));
            Assert.That(outputPairs, Has.Count.EqualTo(expectedOutputPairs));
            Assert.That(outputPairs.GetValueOrDefault("has_updates"), Is.EqualTo(expectedHasUpdates));
            Assert.That(outputPairs.GetValueOrDefault("number_of_updates"), Is.EqualTo(expectedNumberOfUpdates));
            Assert.That(outputPairs.GetValueOrDefault("update_json"), Is.EqualTo(expectedUpdateJson).IgnoreWhiteSpace);
        });
        mockLogger.VerifyLog(LogLevel.Information, Times.Never);
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IReadOnlyDictionary<string, string> LoadPairs(string path)
#pragma warning restore CA1859
    {
        var pairs = new Dictionary<string, string>();
        using var reader = new StreamReader(path);

        while (reader.ReadLine() is { } line)
        {
            string[] parts = line.Split('=');

            if (parts.Length is not 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string val = parts[1].Trim();

            pairs[key] = val;
        }

        return pairs;
    }
}