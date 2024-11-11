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

namespace MavenVersionChecker.Action.Tests.Data;

[TestFixture]
public class SummaryTests
{
    [Test, Description("Should return markdown header with H1 default for string with no level specified.")]
    public void Should_ReturnMarkdownHeaderWithH1Default_ForStringWithNoLevelSpecified()
    {
        var summary = new Summary();
        string expectedMarkdown = $"# Test Title{Environment.NewLine}{Environment.NewLine}";

        summary.AppendHeader("Test Title");
        string markdown = summary.ToString();

        Assert.That(markdown, Is.EqualTo(expectedMarkdown));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [Description("Should return markdown header with specified level for valid level range.")]
    public void Should_ReturnMarkdownHeaderWithSpecifiedLevel_ForValidLevelRange(int level)
    {
        var summary = new Summary();
        string expectedMarkdown = $"{new string('#', level)} Test Title{Environment.NewLine}{Environment.NewLine}";

        summary.AppendHeader("Test Title", level);
        string markdown = summary.ToString();

        Assert.That(markdown, Is.EqualTo(expectedMarkdown));
    }

    [TestCase(0)]
    [TestCase(7)]
    [Description("Should throw ArgumentException for invalid header level.")]
    public void Should_ThrowArgumentException_ForInvalidHeaderLevel(int level)
    {
        var summary = new Summary();
        string expectedMessage = "Valid range for header level is from 1 to 6. (Parameter 'level')";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendHeader("Test Title", level));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test, Description("Should return markdown paragraph for passed string.")]
    public void Should_ReturnMarkdownParagraph_ForPassedString()
    {
        var summary = new Summary();
        string expectedMarkdown = $"This is a test.{Environment.NewLine}{Environment.NewLine}";

        summary.AppendParagraph("This is a test.");
        string markdown = summary.ToString();

        Assert.That(markdown, Is.EqualTo(expectedMarkdown));
    }

    [Test, Description("Should return markdown table for passed lists of columns and rows.")]
    public void Should_ReturnMarkdownTable_ForPassedListsOfColumnsAndRows()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = ["col1", "col2"];
        IReadOnlyList<IReadOnlyList<string>> rows = [["row1-cell1", "row1-cell2"], ["row2-cell1", "row2-cell2"]];
        string expectedMarkdown = """
            | col1 | col2 |
            | --- | --- |
            | row1-cell1 | row1-cell2 |
            | row2-cell1 | row2-cell2 |

            
            """;

        summary.AppendTable(columns, rows);
        string markdown = summary.ToString();

        Assert.That(markdown, Is.EqualTo(expectedMarkdown));
    }

    [Test, Description("Should throw ArgumentException when number of cells do not match columns.")]
    public void Should_ThrowArgumentException_When_NumberOfCellsDoNotMatchColumns()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = ["col1"];
        IReadOnlyList<IReadOnlyList<string>> rows = [["row1-cell1", "row1-cell2"], ["row2-cell1", "row2-cell2"]];
        string expectedMessage = "Number of row cells does not match number of columns.";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendTable(columns, rows));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test, Description("Should throw ArgumentException when no columns are provided.")]
    public void Should_ThrowArgumentException_When_NoColumnsAreProvided()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = [];
        IReadOnlyList<IReadOnlyList<string>> rows = [["row1-cell1", "row1-cell2"]];
        string expectedMessage = "At least one column is required. (Parameter 'columns')";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendTable(columns, rows));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test, Description("Should throw ArgumentException when passing null instance of column list.")]
    public void Should_ThrowArgumentException_When_PassingNullInstanceOfColumnList()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = null!;
        IReadOnlyList<IReadOnlyList<string>> rows = [["row1-cell1", "row1-cell2"]];
        string expectedMessage = "At least one column is required. (Parameter 'columns')";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendTable(columns, rows));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test, Description("Should throw ArgumentException when no rows are provided.")]
    public void Should_ThrowArgumentException_When_NoRowsAreProvided()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = ["col1"];
        IReadOnlyList<IReadOnlyList<string>> rows = [];
        string expectedMessage = "At least one row is required. (Parameter 'rows')";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendTable(columns, rows));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    [Test, Description("Should throw ArgumentException when passing null instance of row list.")]
    public void Should_ThrowArgumentException_When_PassingNullInstanceOfRowList()
    {
        var summary = new Summary();
        IReadOnlyList<string> columns = ["col1"];
        IReadOnlyList<IReadOnlyList<string>> rows = null!;
        string expectedMessage = "At least one row is required. (Parameter 'rows')";

        var exception = Assert.Throws<ArgumentException>(() => summary.AppendTable(columns, rows));

        Assert.Multiple(() =>
        {
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }
}