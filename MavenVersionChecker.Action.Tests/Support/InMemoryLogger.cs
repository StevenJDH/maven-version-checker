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

namespace MavenVersionChecker.Action.Tests.Support;

internal class InMemoryLogger<T> : ILogger<T>
{
    private readonly List<(LogLevel Level, string Entry)> _logEntries = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
        Func<TState, Exception?, string> formatter) => _logEntries.Add((logLevel, formatter(state, exception)));

    public IReadOnlyList<(LogLevel Level, string Entry)> GetLogMessages() => _logEntries.AsReadOnly();
}