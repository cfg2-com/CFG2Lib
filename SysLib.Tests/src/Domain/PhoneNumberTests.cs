using Xunit;
using CFG2.Utils.SysLib.Domain;

namespace CFG2.Utils.SysLib.Tests.Domain
{
    public class PhoneNumberTests
    {
        [Fact]
        public void Constructor_ValidUSNumber_ParsesCorrectly()
        {
            // Arrange
            var number = "123-456-7890";

            // Act
            var phoneNumber = new PhoneNumber(number);

            // Assert
            Assert.True(phoneNumber.IsValid);
            Assert.Equal("+11234567890", phoneNumber.E164Format);
            Assert.Equal("1", phoneNumber.CountryCode);
            Assert.Equal("123", phoneNumber.AreaCode);
            Assert.Equal("456", phoneNumber.Exchange);
            Assert.Equal("7890", phoneNumber.LineNumber);
            Assert.Equal("+1 (123) 456-7890", phoneNumber.FormattedNumberI18N);
            Assert.Equal("123-456-7890", phoneNumber.FormattedNumberUS);
            Assert.Equal("(123) 456-7890", phoneNumber.FormattedNumberUSparens);
            Assert.Equal("123.456.7890", phoneNumber.FormattedNumberUSdots);
        }

        [Fact]
        public void Constructor_ValidUSNumberWithCountryCode_ParsesCorrectly()
        {
            // Arrange
            var number = "+1 (123) 456-7890";

            // Act
            var phoneNumber = new PhoneNumber(number);

            // Assert
            Assert.True(phoneNumber.IsValid);
            Assert.Equal("+11234567890", phoneNumber.E164Format);
            Assert.Equal("1", phoneNumber.CountryCode);
            Assert.Equal("123", phoneNumber.AreaCode);
            Assert.Equal("456", phoneNumber.Exchange);
            Assert.Equal("7890", phoneNumber.LineNumber);
            Assert.Equal("+1 (123) 456-7890", phoneNumber.FormattedNumberI18N);
            Assert.Equal("123-456-7890", phoneNumber.FormattedNumberUS);
            Assert.Equal("(123) 456-7890", phoneNumber.FormattedNumberUSparens);
            Assert.Equal("123.456.7890", phoneNumber.FormattedNumberUSdots);
        }

        [Fact]
        public void Constructor_ValidInternationalNumber_ParsesCorrectly()
        {
            // Arrange
            var number = "+44 20 7946 0958"; // Example UK number

            // Act
            var phoneNumber = new PhoneNumber(number);

            // Assert
            Assert.True(phoneNumber.IsValid);
            Assert.Equal("+442079460958", phoneNumber.E164Format);
            Assert.Equal("44", phoneNumber.CountryCode);
            Assert.Equal("+44 2079460958", phoneNumber.FormattedNumberI18N);
            // For non-NANP numbers, these are expected to be empty with the corrected logic
            Assert.Equal(string.Empty, phoneNumber.AreaCode);
            Assert.Equal(string.Empty, phoneNumber.Exchange);
            Assert.Equal(string.Empty, phoneNumber.LineNumber);
        }

        [Theory]
        [InlineData("+11234567890", true)] // Valid NANP with +
        [InlineData("11234567890", true)]  // Valid NANP without +
        [InlineData("+442079460958", true)]// Valid international with +
        [InlineData("442079460958", true)] // Valid international without +
        [InlineData("1234567", true)]      // Minimum length (7 digits)
        [InlineData("123456789012345", true)] // Maximum length (15 digits)
        [InlineData("123456", false)]      // Too short (6 digits)
        [InlineData("1234567890123456", false)] // Too long (16 digits)
        [InlineData("+0123456789", false)] // Starts with 0 after +
        [InlineData("0123456789", false)]  // Starts with 0
        [InlineData("", false)]            // Empty string
        [InlineData("not-a-number", false)]// Contains non-digit characters
        public void IsValidPhoneNumber_StaticMethod_ReturnsCorrectResult(string number, bool expected)
        {
            // Act
            var isValid = PhoneNumber.IsValidPhoneNumber(number);

            // Assert
            Assert.Equal(expected, isValid);
        }
    }
}