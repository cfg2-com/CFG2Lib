using Xunit;
using CFG2.Utils.HttpLib;
using System.Collections.Generic;

namespace CFG2.Utils.HttpLib.Tests
{
    public class HttpRequestTests
    {
        [Fact]
        public void Constructor_SetsBaseUrl()
        {
            // Arrange
            var baseUrl = "https://api.example.com";

            // Act
            var request = new HttpRequest(baseUrl);

            // Assert
            Assert.Equal(baseUrl, request.Url);
        }

        [Fact]
        public void Url_Property_ReturnsBaseUrl_When_NoUrlParams()
        {
            // Arrange
            var baseUrl = "https://api.example.com/data";
            var request = new HttpRequest(baseUrl);

            // Act
            var url = request.Url;

            // Assert
            Assert.Equal(baseUrl, url);
        }

        [Fact]
        public void Url_Property_ConstructsUrlWithParameters_When_UrlParamsProvided()
        {
            // Arrange
            var baseUrl = "https://api.example.com/search";
            var request = new HttpRequest(baseUrl)
            {
                UrlParams = new Dictionary<string, string>
                {
                    { "q", "gemini code assist" },
                    { "lang", "en" }
                }
            };

            // Act
            var url = request.Url;

            // Assert
            var expectedUrl = "https://api.example.com/search?q=gemini%20code%20assist&lang=en";
            Assert.Equal(expectedUrl, url);
        }

        [Fact]
        public void Url_Property_HandlesSpecialCharactersInParameters()
        {
            // Arrange
            var baseUrl = "https://api.example.com/login";
            var request = new HttpRequest(baseUrl)
            {
                UrlParams = new Dictionary<string, string>
                {
                    { "user", "test@example.com" },
                    { "redirect", "https://example.com/path?a=1&b=2" }
                }
            };

            // Act
            var url = request.Url;

            // Assert
            var expectedUrl = "https://api.example.com/login?user=test%40example.com&redirect=https%3A%2F%2Fexample.com%2Fpath%3Fa%3D1%26b%3D2";
            Assert.Equal(expectedUrl, url);
        }

        [Fact]
        public void ID_IsNotNullOrEmpty()
        {
            // Arrange
            var request = new HttpRequest("https://example.com");

            // Act
            var id = request.ID;

            // Assert
            Assert.False(string.IsNullOrEmpty(id));
        }
    }
}
