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

using MavenVersionChecker.Action.Chaos;
using MavenVersionChecker.Action.Tests.Support;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;

namespace MavenVersionChecker.Action.Tests.Chaos;

[TestFixture]
public class ChaosManagerTests
{
    // ReSharper disable InconsistentNaming
    private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
    private const string CHAOS_ENV = "Chaos";
    private const string CHAOS_LOG_MESSAGE = "Chaos strategies are active.\n";
    // ReSharper restore InconsistentNaming

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, null);
    }

    [Test, Description("Should enable chaos strategies when environment is set to chaos.")]
    public async Task Should_EnableChaosStrategies_When_EnvironmentIsSetToChaos()
    {
        Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, CHAOS_ENV);
        var mockLogger = new Mock<ILogger<ChaosManager>>();
        var manager = new ChaosManager(mockLogger.Object);

        var context = ResilienceContextPool.Shared.Get();
        bool status = await manager.IsChaosEnabledAsync(context);
        ResilienceContextPool.Shared.Return(context);

        Assert.That(status, Is.True);
        mockLogger.VerifyLog(LogLevel.Warning, Times.Once, CHAOS_LOG_MESSAGE);
    }

    [Test, Description("Should disable chaos strategies when environment is not set to chaos.")]
    public async Task Should_DisableChaosStrategies_When_EnvironmentIsNotSetToChaos()
    {
        var mockLogger = new Mock<ILogger<ChaosManager>>();
        var manager = new ChaosManager(mockLogger.Object);

        var context = ResilienceContextPool.Shared.Get();
        bool status = await manager.IsChaosEnabledAsync(context);
        ResilienceContextPool.Shared.Return(context);

        Assert.That(status, Is.False);
        mockLogger.VerifyLog(LogLevel.Warning, Times.Never);
    }

    [Test, Description("Should return injection rate greater than zero when environment is set to chaos.")]
    public async Task Should_ReturnInjectionRateGreaterThanZero_When_EnvironmentIsSetToChaos()
    {
        Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, CHAOS_ENV);
        var mockLogger = new Mock<ILogger<ChaosManager>>();
        var manager = new ChaosManager(mockLogger.Object);

        var context = ResilienceContextPool.Shared.Get();
        double injectionRate = await manager.GetInjectionRateAsync(context);
        ResilienceContextPool.Shared.Return(context);

        Assert.That(injectionRate, Is.EqualTo(0.1));
        mockLogger.VerifyLog(LogLevel.Warning, Times.Once, CHAOS_LOG_MESSAGE);
    }

    [Test, Description("Should return injection rate of zero when environment is not set to chaos.")]
    public async Task Should_ReturnInjectionRateOfZero_When_EnvironmentIsNotSetToChaos()
    {
        var mockLogger = new Mock<ILogger<ChaosManager>>();
        var manager = new ChaosManager(mockLogger.Object);

        var context = ResilienceContextPool.Shared.Get();
        double injectionRate = await manager.GetInjectionRateAsync(context);
        ResilienceContextPool.Shared.Return(context);

        Assert.That(injectionRate, Is.EqualTo(0.0));
        mockLogger.VerifyLog(LogLevel.Warning, Times.Never);
    }
}