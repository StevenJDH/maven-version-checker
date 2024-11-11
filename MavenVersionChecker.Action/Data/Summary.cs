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

using System.Text;

namespace MavenVersionChecker.Action.Data;

internal class Summary
{
    private readonly StringBuilder _buffer = new();

    /// <summary>
    /// Creates a markdown header and appends this to the summary.
    /// </summary>
    /// <param name="header">The header text.</param>
    /// <param name="level">Optional header level. Default is 1.</param>
    public void AppendHeader(string header, int level = 1)
    {
        if (level is < 1 or > 6)
        {
            throw new ArgumentException("Valid range for header level is from 1 to 6.", nameof(level));
        }

        string heading = new('#', level);
        
        _buffer.AppendLine($"{heading} {header.Trim()}{Environment.NewLine}");
    }

    /// <summary>
    /// Creates a markdown paragraph and appends this to the summary.
    /// </summary>
    /// <param name="text">The paragraph text.</param>
    public void AppendParagraph(string text) => _buffer.AppendLine($"{text.Trim()}{Environment.NewLine}");

    /// <summary>
    /// Creates a markdown table and appends this to the summary.
    /// </summary>
    /// <param name="columns">A readonly list of column names.</param>
    /// <param name="rows">A multidimensional readonly list containing rows and cells.</param>
    public void AppendTable(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        string header = CreateTableHeader(columns);
        string body = CreateTableBody(rows, columns.Count);

        _buffer.AppendLine(header);
        _buffer.Append(body);
        _buffer.AppendLine();
    }


    private static string CreateTableHeader(IReadOnlyList<string> columns)
    {
        if (columns == null || !columns.Any())
        {
            throw new ArgumentException("At least one column is required.", nameof(columns));
        }

        var headers = new StringBuilder("|");
        var divider = new StringBuilder("|");
         
        foreach (string header in columns)
        {
            headers.Append($" {header} |");
            divider.Append(" --- |");
        }

        return $"{headers}{Environment.NewLine}{divider}";
    }

    private static string CreateTableBody(IReadOnlyList<IReadOnlyList<string>> rows, int numOfColumns)
    {
        if (rows == null || !rows.Any())
        {
            throw new ArgumentException("At least one row is required.", nameof(rows));
        }

        var table = new StringBuilder();

        foreach (var row in rows)
        {
            if (row.Count != numOfColumns)
            {
                throw new ArgumentException("Number of row cells does not match number of columns.");
            }

            var entry = new StringBuilder("|");

            foreach (string cell in row)
            {
                entry.Append($" {cell} |");
            }

            table.AppendLine(entry.ToString());
        }

        return table.ToString();
    }

    public override string ToString()
    {
        return _buffer.ToString();
    }
}