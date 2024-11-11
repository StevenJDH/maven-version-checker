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

// Ignore Spelling: Api

using MavenVersionChecker.Action.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace MavenVersionChecker.Action.Tests.Services
{
    [TestFixture]
    public class MavenApiServiceTests
    {
        [Test, Description("Should return latest version for queried artifact.")]
        public async Task Should_ReturnLatestVersion_ForQueriedArtifact()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var service = CreateMavenApiService(mockHttpMessageHandler);
            string expectedQueryResponse = JsonSerializer.Serialize(new
            {
                response = new
                {
                    numFound = 1,
                    docs = new[] {
                        new { latestVersion = "1.0.0" }
                    }
                }
            });
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedQueryResponse, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
            const int expectedNumberFound = 1;
            const string expectedLatestVersion = "1.0.0";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            // Act
            var queryResponse = await service.QueryLatestVersionAsync("foo", "bar");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(queryResponse, Is.Not.Null);
                Assert.That(queryResponse!.Result.NumberFound, Is.EqualTo(expectedNumberFound));
                Assert.That(queryResponse.Result.Artifacts, Has.Count.EqualTo(expectedNumberFound));
                Assert.That(queryResponse.Result.Artifacts[0].LatestVersion, Is.EqualTo(expectedLatestVersion));
            });
        }

        [Test, Description("Should throw HttpRequestException for bad responses.")]
        public void Should_ThrowHttpRequestException_ForBadResponses()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var service = CreateMavenApiService(mockHttpMessageHandler);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            const string expectedMessage = "Response status code does not indicate success: 500 (Internal Server Error).";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            Assert.That(async () => await service.QueryLatestVersionAsync("foo", "bar"), Throws.TypeOf<HttpRequestException>()
                .And.InstanceOf<Exception>()
                .And.Message.EqualTo(expectedMessage)
                .And.InnerException.Null);
        }

        private static MavenApiService CreateMavenApiService(Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            mockHttpClientFactory
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return new MavenApiService(mockHttpClientFactory.Object);
        }
    }
}