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

using MavenVersionChecker.Action.Extensions;
using MavenVersionChecker.Action;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddGitHubActionServices())
    .ConfigureLogging((_, logging) =>
    {
        logging.AddGitHubConsoleLogger();
        // Removes the noisy Polly info logs. Reference: https://github.com/App-vNext/Polly/issues/1958#issuecomment-1932281918
        logging.AddFilter("Polly", LogLevel.Warning);
    })
    .Build();

Console.WriteLine($"""
    Maven Version Checker 1.0.0.24111
    Copyright (C) 2024{(DateTime.Now.Year.Equals(2024) ? "" : $"-{DateTime.Now.Year}")} Steven Jenkins De Haro

    """);

var processor = host.Services.GetRequiredService<MavenVersionCheckerProcessor>();
return (int)await processor.StartProcessingPomAsync();