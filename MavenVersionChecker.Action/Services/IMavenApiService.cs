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

using MavenVersionChecker.Action.Data;

namespace MavenVersionChecker.Action.Services;

internal interface IMavenApiService
{
    /// <summary>
    /// Queries for the latest version of an artifact,
    /// </summary>
    /// <param name="groupId">The groupId of a dependency or a plugin.</param>
    /// <param name="artifactId">The artifactId of a dependency or a plugin.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
    /// </param>
    /// <returns></returns>
    Task<MavenQueryResponse?> QueryLatestVersionAsync(string groupId, string artifactId,
        CancellationToken cancellationToken = default);
}