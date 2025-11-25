using Xunit;
using CFG2.Utils.SysLib;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CFG2.Utils.SysLib.Tests
{
    public class MiscUtilsTests : IDisposable
    {
        public void Dispose()
        {
            // Reset the static state after each test to ensure test isolation
            MiscUtils.Reinitialize();
        }

        [Fact]
        public void GenerateCorrelationId_DefaultPrefix_ReturnsCorrectFormat()
        {
            // Act
            var correlationId = MiscUtils.GenerateCorrelationId();

            // Assert
            Assert.StartsWith("CID-", correlationId);
            Assert.Matches(@"^CID-\d{10,}$", correlationId); // Have to match 10 or more because of possible counter suffix
        }

        [Fact]
        public void GenerateCorrelationId_CustomPrefix_ReturnsCorrectFormat()
        {
            // Act
            var correlationId = MiscUtils.GenerateCorrelationId("test");

            // Assert
            Assert.StartsWith("TEST-", correlationId);
            Assert.Matches(@"^TEST-\d{10,}$", correlationId);
        }

        [Fact]
        public void GenerateCorrelationId_NullOrEmptyPrefix_DefaultsToCid()
        {
            // Act
            var correlationId1 = MiscUtils.GenerateCorrelationId(null);
            var correlationId2 = MiscUtils.GenerateCorrelationId("");

            // Assert
            Assert.StartsWith("CID-", correlationId1);
            Assert.StartsWith("CID-", correlationId2);
        }

        [Fact]
        public void GenerateCorrelationId_MultipleCallsInSameMinute_IncrementsCounter()
        {
            // Arrange
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");

            // Act
            var id1 = MiscUtils.GenerateCorrelationId("SEQ");
            var id2 = MiscUtils.GenerateCorrelationId("SEQ");
            var id3 = MiscUtils.GenerateCorrelationId("SEQ");

            // Assert
            Assert.Equal($"SEQ-{timestamp}", id1);
            Assert.Equal($"SEQ-{timestamp}1", id2);
            Assert.Equal($"SEQ-{timestamp}2", id3);
        }

        [Theory]
        [InlineData("Some subject -> CORRID-12345", "CORRID-12345")]
        [InlineData("Another subject (COR-12345)", "COR-12345")]
        [InlineData("Subject without id", "")]
        public void GetCorrelationId_FromSubject_ReturnsCorrectId(string subject, string expectedId)
        {
            // Act
            var correlationId = MiscUtils.GetCorrelationId(subject);

            // Assert
            Assert.Equal(expectedId, correlationId);
        }

        [Fact]
        public void GetCorrelationId_FromDetail_ReturnsCorrectId()
        {
            // Arrange
            var subject = "Some subject";
            var detail = "Some info\ncorrelation: DETAIL-ID-6789\nmore info";

            // Act
            var correlationId = MiscUtils.GetCorrelationId(subject, detail);

            // Assert
            Assert.Equal("DETAIL-ID-6789", correlationId);
        }

        [Fact]
        public void GetCorrelationId_PrefersSubjectArrowOverParenAndDetail()
        {
            // Arrange
            var subject = "Subject (PAREN-ID) -> ARROW-ID";
            var detail = "correlation: DETAIL-ID";

            // Act
            var correlationId = MiscUtils.GetCorrelationId(subject, detail);

            // Assert
            Assert.Equal("ARROW-ID", correlationId);
        }

        [Fact]
        public void GetCorrelationId_CheckFlags_DisablesChecks()
        {
            // Arrange
            var subjectWithArrow = "Subject -> ARROW-ID";
            var subjectWithParen = "Subject (PAREN-ID)";

            // Act
            var result1 = MiscUtils.GetCorrelationId(subjectWithArrow, arrowCheck: false);
            var result2 = MiscUtils.GetCorrelationId(subjectWithParen, parenCheck: false);

            // Assert
            Assert.Equal("", result1);
            Assert.Equal("", result2);
        }

        [Theory]
        [InlineData("Subject -> ID-123", "ID-123", "Subject")]
        [InlineData("Subject (ID-123)", "ID-123", "Subject")]
        [InlineData("Subject -> ID-123", "WRONG-ID", "Subject -> ID-123")]
        [InlineData("Subject (ID-123)", "WRONG-ID", "Subject (ID-123)")]
        [InlineData("Subject without ID", "ID-123", "Subject without ID")]
        [InlineData("Subject", null, "Subject")]
        [InlineData("Subject", "", "Subject")]
        [InlineData("Subject (ID-123) -> ID-123", "ID-123", "Subject")]
        public void StripCorrelationId_VariousScenarios_StripsCorrectly(string subject, string correlationId, string expectedSubject)
        {
            // Act
            var result = MiscUtils.StripCorrelationId(subject, correlationId);

            // Assert
            Assert.Equal(expectedSubject, result);
        }

        [Fact]
        public void StripCorrelationId_CheckFlags_DisablesStripping()
        {
            // Arrange
            var subjectWithArrow = "Subject -> ARROW-ID";
            var subjectWithParen = "Subject (PAREN-ID)";

            // Assert
            Assert.Equal(subjectWithArrow, MiscUtils.StripCorrelationId(subjectWithArrow, "ARROW-ID", arrowCheck: false));
            Assert.Equal(subjectWithParen, MiscUtils.StripCorrelationId(subjectWithParen, "PAREN-ID", parenCheck: false));
        }
    }
}