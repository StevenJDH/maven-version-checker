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

using System.Text;

namespace MavenVersionChecker.Action.Data;

/// <summary>
/// Based on the GitHub Actions ToolKit, this class provides a set of types and methods to make creating actions easier.
/// 
/// Reference:
/// https://github.com/actions/toolkit/blob/415c42d27ca2a24f3801dd9406344aaea00b7866/packages/core/src/core.ts
/// </summary>
internal static class Core
{
    /// <summary>
    /// The code to exit an action.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// A code indicating that the action was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// A code indicating that the action was a failure.
        /// </summary>
        Failure = 1
    }

    /// <summary>
    /// Gets the value of an input.
    /// Unless trimWhitespace is set to false in InputOptions, the value is also trimmed.
    /// Returns an empty string if the value is not defined.
    /// </summary>
    /// <param name="name">Name of the input to get.</param>
    /// <param name="options">Optional. See <see cref="InputOptions"/>.</param>
    /// <returns>Input value or empty string.</returns>
    /// <exception cref="ArgumentNullException">When required input is not supplied.</exception>
    public static string GetInput(string name, InputOptions? options = default)
    {
        var input = Environment.GetEnvironmentVariable($"INPUT_{name.Replace(' ', '_').ToUpperInvariant()}",
            EnvironmentVariableTarget.Process);

        return options switch
        {
            { Required: true } when string.IsNullOrWhiteSpace(input) =>
                throw new ArgumentNullException(name, "Required input not supplied."),
            { TrimWhitespace: false } => input ?? "",
            _ => input?.Trim() ?? ""
        };
    }

    /// <summary>
    /// Sets the value of an output asynchronously.
    /// </summary>
    /// <param name="name">Name of the output to set.</param>
    /// <param name="value">Value to store. Must be converted to string.</param>
    /// <returns>A readonly list of passed arguments.</returns>
    public static async ValueTask<IReadOnlyList<string>> SetOutputAsync(string name, string value)
    {
        string outputName = name.Trim();
        string outputValue = value.Trim();

        await IssueFileCommand("OUTPUT", $"{outputName}={outputValue}");

        return [outputName, outputValue];
    }

    /// <summary>
    /// Sets an env variable for this action and future actions in the job asynchronously.
    /// </summary>
    /// <param name="name">Name of the variable to set.</param>
    /// <param name="value">Value to store. Must be converted to string.</param>
    /// <returns>A readonly list of passed arguments.</returns>
    public static async ValueTask<IReadOnlyList<string>> ExportVariableAsync(string name, string value)
    {
        string envName = name.Trim();
        string envValue = value.Trim();

        await IssueFileCommand("ENV", $"{envName}={envValue}");
        
        return [envName, envValue];
    }

    public static async ValueTask SetStepSummaryAsync(string markdown)
    {
        await IssueFileCommand("STEP_SUMMARY", markdown.Trim());
    }


    private static async ValueTask IssueFileCommand(string commandSuffix, string content)
    {
        string? filePath = Environment.GetEnvironmentVariable($"GITHUB_{commandSuffix}", EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"Unable to find environment variable for file command {commandSuffix} (GITHUB_{commandSuffix}).");
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Missing file at path: '{filePath}' for file command {commandSuffix} (GITHUB_{commandSuffix}).");
        }

        await using var textWriter = new StreamWriter(filePath, append: true, Encoding.UTF8);
        await textWriter.WriteLineAsync(content);
    }
}