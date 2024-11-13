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

using Microsoft.Extensions.Logging;
using Moq;
using System.Text.RegularExpressions;

namespace MavenVersionChecker.Action.Tests.Support;

internal static class MoqExtensions
{
    /// <summary>
    /// Verifies that a specific invocation of a logger matches the given arguments passed on the mock.
    /// </summary>
    /// <typeparam name="T">Instance of <see cref="ILogger{T}"/>.</typeparam>
    /// <param name="logger">Instance of <see cref="Mock{ILogger{T}}"/>.</param>
    /// <param name="level">Log level of the entry.</param>
    /// <param name="times">The number of times a method is expected to be called.</param>
    /// <param name="regex">Literal or regex string for matching log message.</param>
    /// <example>
    /// <code>
    /// logger.VerifyLog(LogLevel.Information, Times.Exactly(1), "This is a simple log example.");
    /// logger.VerifyLog(LogLevel.Warning, Times.Exactly(2), "[Tt]his\sis a regex log example.*");
    /// </code>
    /// </example>
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times, string? regex = null) =>
        logger.Verify(
            m => m.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => regex == null || Regex.IsMatch(v.ToString() ?? "", regex)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    /// <summary>
    /// Verifies that a specific invocation of a logger matches the given arguments passed on the mock.
    /// </summary>
    /// <typeparam name="T">Instance of <see cref="ILogger{T}"/>.</typeparam>
    /// <param name="logger">Instance of <see cref="Mock{ILogger{T}}"/>.</param>
    /// <param name="level">Log level of the entry.</param>
    /// <param name="times">The number of times a method is expected to be called.</param>
    /// <param name="regex">Literal or regex string for matching log message.</param>
    /// <example>
    /// <code>
    /// logger.VerifyLog(LogLevel.Information, Times.Once, "This is a simple log example.");
    /// logger.VerifyLog(LogLevel.Warning, Times.Once, "[Tt]his\sis a regex log example.*");
    /// </code>
    /// </example>
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Func<Times> times, string? regex = null) =>
        logger.Verify(
            m => m.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => regex == null || Regex.IsMatch(v.ToString() ?? "", regex)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
}