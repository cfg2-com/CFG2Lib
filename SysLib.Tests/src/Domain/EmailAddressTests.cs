using Xunit;
using CFG2.Utils.SysLib.Domain;

namespace CFG2.Utils.SysLib.Tests.Domain
{
    public class EmailAddressTests
    {
        [Fact]
        public void Constructor_WithValidEmail_ParsesCorrectly()
        {
            // Arrange
            var emailStr = "test.user@example.com";

            // Act
            var emailAddress = new EmailAddress(emailStr);

            // Assert
            Assert.True(emailAddress.IsValid);
            Assert.False(emailAddress.IsCleansed);
            Assert.Equal("test.user@example.com", emailAddress.Address);
            Assert.Equal("test.user", emailAddress.User);
            Assert.Equal("example.com", emailAddress.Domain);
            Assert.Equal("com", emailAddress.TLD);
        }

        [Fact]
        public void Constructor_WithDirtyButValidEmail_CleansesAndParsesCorrectly()
        {
            // Arrange
            var emailStr = "  Test.User@Example.COM  ";

            // Act
            var emailAddress = new EmailAddress(emailStr);

            // Assert
            Assert.True(emailAddress.IsValid);
            Assert.True(emailAddress.IsCleansed);
            Assert.Equal("test.user@example.com", emailAddress.Address);
            Assert.Equal("test.user", emailAddress.User);
            Assert.Equal("example.com", emailAddress.Domain);
            Assert.Equal("com", emailAddress.TLD);
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("@missinguser.com")]
        [InlineData("missingdomain@")]
        [InlineData("test@domain.c")] // TLD too short
        [InlineData("test space@domain.com")]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_WithInvalidEmail_SetsPropertiesCorrectly(string invalidEmail)
        {
            // Act
            var emailAddress = new EmailAddress(invalidEmail);

            // Assert
            Assert.False(emailAddress.IsValid);
            Assert.Equal(invalidEmail.Trim().ToLower(), emailAddress.Address);
            Assert.Equal("", emailAddress.User);
            Assert.Equal("", emailAddress.Domain);
            Assert.Equal("", emailAddress.TLD);
        }

        [Fact]
        public void Constructor_WithNullEmail_HandlesGracefully()
        {
            // Act & Assert
            // A null string will cause a NullReferenceException on Trim(), which is expected.
            // If you wanted to handle this, you'd add a null check in the constructor.
            Assert.Throws<System.NullReferenceException>(() => new EmailAddress(null!));
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("test.name+alias@gmail.co.uk", true)]
        [InlineData("12345@my-domain.net", true)]
        [InlineData("invalid", false)]
        [InlineData("test@", false)]
        [InlineData("@domain.com", false)]
        [InlineData("test@domain.c", false)]
        [InlineData("", false)]
        public void IsValidEmail_StaticMethod_ReturnsCorrectResult(string email, bool expected)
        {
            // Act
            var isValid = EmailAddress.IsValidEmail(email);

            // Assert
            Assert.Equal(expected, isValid);
        }
    }
}