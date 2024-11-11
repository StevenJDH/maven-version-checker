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

// Ignore Spelling: Pom

using MavenVersionChecker.Action.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MavenVersionChecker.Action.Components;

internal sealed class MavenPom(string location, Dictionary<string, string>? parentProperties)
{
    // ReSharper disable InconsistentNaming
    private static readonly XNamespace MAVEN_NAMESPACE = "http://maven.apache.org/POM/4.0.0";
    private static readonly XName GROUP_ID = MAVEN_NAMESPACE + "groupId";
    private static readonly XName ARTIFACT_ID = MAVEN_NAMESPACE + "artifactId";
    private static readonly XName VERSION = MAVEN_NAMESPACE + "version";
    // ReSharper restore InconsistentNaming

    private readonly XDocument _pomXmlDoc = XDocument.Load(location);
    private readonly string _parentDir = Path.GetDirectoryName(location)!;
    private readonly Dictionary<string, string>? _parentProperties = parentProperties;

    public enum ArtifactType
    {
        Parent,
        Dependency,
        Plugin
    }

    public MavenPom(string location) : this(location, null)
    {
    }

    public Dictionary<string, string> Properties
    {
        get
        {
            var currentProperties = _pomXmlDoc.Descendants(MAVEN_NAMESPACE + "properties")
                .Elements()
                .ToDictionary(prop => prop.Name.LocalName, prop => prop.Value);

            return _parentProperties == null ? currentProperties : currentProperties.Union(_parentProperties.Where(k => !currentProperties.ContainsKey(k.Key)))
                .ToDictionary(k => k.Key, v => v.Value);
        }
    }

    public Dictionary<string, string> ModuleLookup => _pomXmlDoc.Descendants(MAVEN_NAMESPACE + "modules")
        .Elements(MAVEN_NAMESPACE + "module")
        .Select(m => m.Value)
        .ToDictionary(m => m, m => Path.Combine(_parentDir, m, "pom.xml"));

    public MavenArtifact? ParentArtifact => ParseMavenArtifacts(ArtifactType.Parent).FirstOrDefault();

    public List<MavenArtifact> Dependencies => ParseMavenArtifacts(ArtifactType.Dependency);

    public List<MavenArtifact> Plugins => ParseMavenArtifacts(ArtifactType.Plugin);

    private List<MavenArtifact> ParseMavenArtifacts(ArtifactType artifactType)
    {
        return _pomXmlDoc.Descendants(MAVEN_NAMESPACE + artifactType.ToString().ToLowerInvariant())
            .Where(artifact =>
            {
                string? version = artifact.Element(VERSION)?.Value;
                return version is not null and not "${project.parent.version}";
            })
            .Select(artifact => new MavenArtifact
            {
                GroupId = artifact.Element(GROUP_ID)!.Value,
                ArtifactId = artifact.Element(ARTIFACT_ID)!.Value,
                Version = ResolveProperty(artifact.Element(VERSION)!.Value)
            })
            .ToList();
    }

    private string ResolveProperty(string propertyReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyReference);

        if (!propertyReference.Contains("$"))
        {
            return propertyReference;
        }

        const string pattern = @"^\${|}$";
        string propertyName = Regex.Replace(propertyReference, pattern, "", RegexOptions.None, TimeSpan.FromMinutes(1));

        return Properties.GetValueOrDefault(propertyName, propertyReference);
    }
}