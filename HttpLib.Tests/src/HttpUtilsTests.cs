using System.Net;
using Xunit;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace CFG2.Utils.HttpLib.Tests
{
    public class HttpUtilsTests
    {
        [Fact]
        public void Get_SuccessfulRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new HttpRequest("https://httpbin.org/get")
            {
                UrlParams = new Dictionary<string, string> { { "test_param", "test_value" } }
            };

            // ACT
            var response = HttpUtils.Get(request);

            // ASSERT
            Assert.True(response.Success);
            Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
            Assert.NotNull(response.Content);
            Assert.Contains("\"test_param\": \"test_value\"", response.Content);
        }

        [Fact]
        public void PostForm_SuccessfulRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new HttpRequest("https://httpbin.org/post")
            {
                FormParams = new Dictionary<string, string>
                {
                    { "customer", "Gemini" },
                    { "custtel", "555-555-5555" }
                }
            };

            // Act
            var response = HttpUtils.PostForm(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
            Assert.NotNull(response.Content);
            Assert.Contains("\"customer\": \"Gemini\"", response.Content);
            Assert.Contains("\"custtel\": \"555-555-5555\"", response.Content);
        }

        [Fact]
        public void PostJson_SuccessfulRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new HttpRequest("https://httpbin.org/post")
            {
                Json = "{\"name\":\"Gemini\",\"role\":\"Code Assistant\"}"
            };

            // Act
            var response = HttpUtils.PostJson(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
            Assert.NotNull(response.Content);
            Assert.Contains("\"json\": {\n    \"name\": \"Gemini\", \n    \"role\": \"Code Assistant\"\n  }", response.Content.Replace("\r", ""));
        }

        [Fact]
        public void Get_WithBearerToken_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new HttpRequest("https://httpbin.org/bearer")
            {
                AuthHeader = new AuthenticationHeaderValue("Bearer", "my-secret-token")
            };

            // Act
            var response = HttpUtils.Get(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
            Assert.NotNull(response.Content);
            Assert.Contains("\"authenticated\": true", response.Content);
            Assert.Contains("\"token\": \"my-secret-token\"", response.Content);
        }
    }
}
