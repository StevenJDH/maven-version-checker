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
using Polly;

namespace MavenVersionChecker.Action.Chaos;

internal class ChaosManager : IChaosManager
{
    private readonly bool _isChaosEnabled;

    public ChaosManager(ILogger<ChaosManager> logger)
    {
        _isChaosEnabled = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Chaos") ?? false;

        if (_isChaosEnabled)
        {
            logger.LogWarning("Chaos strategies are active.\n");
        }
    }

    public ValueTask<bool> IsChaosEnabledAsync(ResilienceContext context)
    {

        return ValueTask.FromResult(_isChaosEnabled);
    }

    public ValueTask<double> GetInjectionRateAsync(ResilienceContext context)
    {
        return ValueTask.FromResult(_isChaosEnabled ? 0.1 : 0.0);
    }
}