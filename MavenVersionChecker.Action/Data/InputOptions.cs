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

namespace MavenVersionChecker.Action.Data;

/// <summary>
/// Options got getting the action inputs.
/// </summary>
/// <param name="Required">Optional. Whether the input is required. If required and not present, will throw and exception. Defaults to false.</param>
/// <param name="TrimWhitespace">Optional. Whether leading/trailing whitespace will be trimmed for the input. Defaults to true.</param>
public readonly record struct InputOptions(bool Required = false, bool TrimWhitespace = true);