using System.Net;

namespace CFG2.Utils.HttpLib.Tests
{
    public class HttpResponseTests
    {
        [Fact]
        public void Properties_CanBeSetAndGet()
        {
            // Arrange
            var response = new HttpResponse();
            var expectedSuccess = true;
            var expectedContent = "{\"message\":\"Hello, World!\"}";
            var expectedStatus = HttpStatusCode.OK;
            var expectedDebug = "Request ID: 12345";

            // Act
            response.Success = expectedSuccess;
            response.Content = expectedContent;
            response.HttpStatusCode = expectedStatus;
            response.DebugInfo = expectedDebug;

            // Assert
            Assert.Equal(expectedSuccess, response.Success);
            Assert.Equal(expectedContent, response.Content);
            Assert.Equal(expectedStatus, response.HttpStatusCode);
            Assert.Equal(expectedDebug, response.DebugInfo);
        }
    }
}
