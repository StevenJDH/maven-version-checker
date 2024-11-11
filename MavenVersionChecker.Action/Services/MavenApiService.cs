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
using System.Net.Http.Json;
using MavenVersionChecker.Action.Extensions;

namespace MavenVersionChecker.Action.Services;

internal class MavenApiService(IHttpClientFactory httpClientFactory) : IMavenApiService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    /// <inheritdoc />
    public async Task<MavenQueryResponse?> QueryLatestVersionAsync(string groupId, string artifactId,
        CancellationToken cancellationToken = default)
    {
        string queryParams = $"q=g:{groupId}+AND+a:{artifactId}&rows=1&wt=json";

        // We create transient client to avoid any potential for socket exhaustion and to allow for DNS rotation.
        var httpClient = _httpClientFactory.CreateClient("MavenApiClient");

        return await httpClient.GetFromJsonAsync($"/solrsearch/select?{queryParams}",
            SourceGenerationContext.Default.MavenQueryResponse, cancellationToken).ConfigureAwait(false);
    }
}