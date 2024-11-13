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

using System.Reflection;

namespace MavenVersionChecker.Action.Tests;

[TestFixture]
public class ProgramTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Environment.SetEnvironmentVariable("INPUT_LOCATION", null);
    }

    [Test, Description("Should load and exit when no location input is provided.")]
    public async Task Should_LoadAndExit_When_NoLocationInputIsProvided()
    {
        var program = typeof(Program).GetTypeInfo();
        var mainMethod = program.DeclaredMethods.Single(m => m.Name == "<Main>$");
        var originalConsoleOut = Console.Out;
        await using var sw = new StringWriter();
        Console.SetOut(sw);
        const string expectedMessage = "::error::Required input not supplied. (Parameter 'location')";

        int exitCode = await (Task<int>) mainMethod.Invoke(null, [Array.Empty<string>()])!;
        string output = sw.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(output, Contains.Substring(expectedMessage));
        });

        Console.SetOut(originalConsoleOut);
    }

    [Test, Description("Should have matching version in console for version defined in assembly.")]
    public async Task Should_HaveMatchingVersionInConsole_ForVersionDefinedInAssembly()
    {
        var program = typeof(Program).GetTypeInfo();
        var mainMethod = program.DeclaredMethods.Single(m => m.Name == "<Main>$");
        var originalConsoleOut = Console.Out;
        await using var sw = new StringWriter();
        Console.SetOut(sw);
        string expectedTitleVersion = $"Maven Version Checker {typeof(Program).Assembly.GetName().Version!.ToString()}";

        int exitCode = await (Task<int>)mainMethod.Invoke(null, [Array.Empty<string>()])!;
        string output = sw.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(output, Contains.Substring(expectedTitleVersion));
        });

        Console.SetOut(originalConsoleOut);
    }
}