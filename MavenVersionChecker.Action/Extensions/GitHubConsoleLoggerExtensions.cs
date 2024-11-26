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

using MavenVersionChecker.Action.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace MavenVersionChecker.Action.Extensions;

public static class GitHubConsoleLoggerExtensions
{
    public static ILoggingBuilder AddGitHubConsoleLogger(this ILoggingBuilder builder)
    {
        builder.ClearProviders();
        
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

        if (env is "Development" or "Chaos")
        {
            builder.AddConsole();
            return builder;
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, GitHubConsoleLoggerProvider>());

        return builder;
    }

    /// <summary>
    /// Formats and writes a notice log message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="message">Format string of the log message in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>logger.LogNotice("Processing request from {Address}", address)</example>
    public static void LogNotice(this ILogger logger, string? message, params object?[] args)
    {
        logger.Log(LogLevel.Information, new EventId(3500, "Notice"), message, args);
    }

    /// <summary>
    /// Formats and writes a group start log title.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="title">Format string of the group title in message template format. Example: <c>"User {User} logged in from {Address}"</c></param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <example>logger.LogGroupStart("My Title")</example>
    public static void LogGroupStart(this ILogger logger, string? title, params object?[] args)
    {
        logger.Log(LogLevel.Information, new EventId(3400, "GroupStart"), title, args);
    }

    /// <summary>
    /// Formats and writes a group ending log message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <example>logger.LogGroupEnd()</example>
    public static void LogGroupEnd(this ILogger logger)
    {
        logger.Log(LogLevel.Information, new EventId(3401, "GroupEnd"), String.Empty, []);
    }

    /// <summary>
    /// Marks a sensitive value or phrase as masked to protect against showing in logs. It's recommended to do
    /// this before passing a secret between jobs using '>> GITHUB_OUTPUT'.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
    /// <param name="entry">Sensitive value or phrase to be masked.</param>
    /// <example>logger.AddMask("Password")</example>
    public static void AddMask(this ILogger logger, string? entry)
    {
        logger.Log(LogLevel.Information, new EventId(3300, "AddMask"), entry, []);
    }
}