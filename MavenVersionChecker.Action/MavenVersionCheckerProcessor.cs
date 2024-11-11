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

// Ignore Spelling: Pom

using System.Text;
using MavenVersionChecker.Action.Data;
using Microsoft.Extensions.Logging;
using static MavenVersionChecker.Action.Data.Core;
using MavenVersionChecker.Action.Components;
using static MavenVersionChecker.Action.Components.MavenPom;
using MavenVersionChecker.Action.Services;
using System.Text.Json;
using MavenVersionChecker.Action.Extensions;

namespace MavenVersionChecker.Action;

internal sealed class MavenVersionCheckerProcessor(ILogger<MavenVersionCheckerProcessor> logger,ActionInputs inputs,
    IMavenApiService service)
{
    private readonly ILogger<MavenVersionCheckerProcessor> _logger = logger;
    private readonly ActionInputs _inputs = inputs;
    private readonly IMavenApiService _service = service;
    private readonly Dictionary<string, List<string>> _availableUpdates = [];

    public async ValueTask<ExitCode> StartProcessingPomAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var pom = new MavenPom(_inputs.Location);
            var moduleLookup = pom.ModuleLookup;
            var summary = new Summary();
            IReadOnlyList<string> columns = ["Type", "Artifact", "Version", "Update"];
            const string parentName = "parent-pom";

            summary.AppendHeader("Maven Version Checker Action", 2);
            summary.AppendParagraph("Below is a list of all the checked artifacts and their update status.");

            if (moduleLookup.Count == 0)
            {
                var rows = await ProcessArtifactsAsync(pom, isParent: true);

                if (rows.Count > 0)
                {
                    summary.AppendTable(columns, rows);
                }
            }
            else
            {
                _logger.LogInformation("Multi-Module project detected.");
                var rows = await ProcessArtifactsAsync(pom, isParent: true, parentName);

                if (rows.Count > 0)
                {
                    summary.AppendHeader(parentName, 3);
                    summary.AppendTable(columns, rows);
                }

                var parentProperties = pom.Properties;

                foreach (var entry in moduleLookup)
                {
                    var modulePom = new MavenPom(entry.Value, parentProperties);
                    var moduleRows = await ProcessArtifactsAsync(modulePom, isParent: false, entry.Key);
                    if (moduleRows.Count <= 0) continue;
                    summary.AppendHeader(entry.Key, 3);
                    summary.AppendTable(columns, moduleRows);
                }
            }

            int updatesCount = _availableUpdates.Values.Sum(l => l.Count);

            await SetStepSummaryAsync(summary.ToString().TrimEnd());
            await SetOutputAsync("has_updates", (updatesCount > 0).ToString().ToLowerInvariant());
            await SetOutputAsync("number_of_updates", updatesCount.ToString());
            await SetOutputAsync("update_json", JsonSerializer.Serialize(_availableUpdates,
                SourceGenerationContext.Default.DictionaryStringListString));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return ExitCode.Failure;
        }

        return ExitCode.Success;
    }

    private async Task<List<List<string>>> ProcessArtifactsAsync(MavenPom pom, bool isParent, string? section = null)
    {
        var parentArtifact = pom.ParentArtifact;
        var dependencies = pom.Dependencies;
        var plugins = pom.Plugins;
        List<List<string>> rows = [];

        if (dependencies.Count > 0 || plugins.Count > 0)
        {
            if (section != null) Console.WriteLine($"\n### {section} ###\n");
            Console.WriteLine("{0,-15} {1,-15} {2,-15} {3}\n{4}",
                "Type",
                "Version",
                "Update",
                "Artifact",
                new string('-', 100));
        }

        if (isParent && parentArtifact != null)
        {
            await AggregateInformationAsync(ArtifactType.Parent, [parentArtifact], rows);
        }

        await AggregateInformationAsync(ArtifactType.Dependency, dependencies, rows);
        await AggregateInformationAsync(ArtifactType.Plugin, plugins, rows);

        return rows;
    }

    private async Task AggregateInformationAsync(ArtifactType artifactType, List<MavenArtifact> artifacts, List<List<string>> rows)
    {
        foreach (var artifact in artifacts)
        {
            var queryResponse = await _service.QueryLatestVersionAsync(artifact.GroupId, artifact.ArtifactId);
            
            string latestVersion = queryResponse?.Result.NumberFound > 0 
                ? queryResponse.Result.Artifacts[0].LatestVersion 
                : string.Empty;
            
            string updateStatus = queryResponse?.Result.NumberFound switch
            {
                > 0 when !artifact.Version.Equals(latestVersion) => latestVersion,
                > 0 when artifact.Version.Equals(latestVersion) => "✔️", // :heavy_check_mark:
                _ => "🔴" // :red_circle:
            };

            Console.WriteLine("{0,-15} {1,-15} {2,-15} {3}:{4}",
                artifactType,
                artifact.Version,
                updateStatus,
                artifact.GroupId,
                artifact.ArtifactId);

            rows.Add([artifactType.ToString(), $"{artifact.GroupId}:{artifact.ArtifactId}", artifact.Version, updateStatus]);

            if (updateStatus.StartsWith(':'))
            {
                continue;
            }

            if (_availableUpdates.TryGetValue(Pluralize(artifactType).ToLowerInvariant(), out var outputArtifacts))
            {
                outputArtifacts.Add($"{artifact.GroupId}:{artifact.ArtifactId}:{updateStatus}");
                continue;
            }

            _availableUpdates.Add(Pluralize(artifactType).ToLowerInvariant(),
                [$"{artifact.GroupId}:{artifact.ArtifactId}:{updateStatus}"]);
        }
    }

    private static string Pluralize(ArtifactType artifactType) => artifactType switch
    {
        ArtifactType.Parent => "Parents",
        ArtifactType.Dependency => "Dependencies",
        ArtifactType.Plugin => "Plugins",
        _ => throw new ArgumentOutOfRangeException(nameof(artifactType), $"Artifact type '{artifactType}' not expected."),
    };
}