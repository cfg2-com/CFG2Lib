using Xunit;
using CFG2.Utils.SysLib;
using System;
using System.IO;

namespace CFG2.Utils.SysLib.Tests
{
    public class SysUtilsTests : IDisposable
    {
        private readonly string _testVarName = "SysUtilsTestVar";
        private string? _tempFile1;
        private string? _tempFile2;

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(_testVarName, null);
            if (_tempFile1 != null && File.Exists(_tempFile1))
            {
                File.Delete(_tempFile1);
            }
            if (_tempFile2 != null && File.Exists(_tempFile2))
            {
                File.Delete(_tempFile2);
            }
        }

        [Fact]
        public void EnvVarExists_ReturnsTrueForExistingVar()
        {
            // Arrange
            Environment.SetEnvironmentVariable(_testVarName, "test_value");

            // Act & Assert
            Assert.True(SysUtils.EnvVarExists(_testVarName));
        }

        [Fact]
        public void EnvVarExists_ReturnsFalseForNonExistingVar()
        {
            // Act & Assert
            Assert.False(SysUtils.EnvVarExists("NON_EXISTENT_VAR_12345"));
        }

        [Fact]
        public void GetEnvVar_ReturnsCorrectValue()
        {
            // Arrange
            var expectedValue = "hello world";
            SysUtils.SetEnvVar(_testVarName, expectedValue);

            // Act
            var actualValue = SysUtils.GetEnvVar(_testVarName);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetEnvVar_ThrowsOnNonExistentVar()
        {
            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => SysUtils.GetEnvVar("NON_EXISTENT_VAR_12345"));
        }

        [Theory]
        [InlineData("File / Name? >", "File - Name")]
        [InlineData("Re: Fwd: Important#Doc&Info", "ImportantDocandInfo")]
        [InlineData("File: with; special*chars<", "File with specialchars")]
        public void GetFileNameSafeString_SanitizesCorrectly(string input, string expected)
        {
            // Act
            var result = SysUtils.GetFileNameSafeString(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFileNameSafeString_WithEmailPrefixesFalse_DoesNotRemovePrefixes()
        {
            // Arrange
            var input = "Re: Fwd: My File";

            // Act
            var result = SysUtils.GetFileNameSafeString(input, removeEmailPrefixes: false);

            // Assert
            Assert.Equal("Re Fwd My File", result);
        }

        [Fact]
        public void PrependTextToFile_AddsTextToBeginning()
        {
            // Arrange
            _tempFile1 = Path.GetTempFileName();
            File.WriteAllText(_tempFile1, "world");
            var textToPrepend = "Hello";

            // Act
            SysUtils.PrependTextToFile(_tempFile1, textToPrepend);

            // Assert
            var content = File.ReadAllText(_tempFile1);
            Assert.StartsWith(textToPrepend, content);
            Assert.EndsWith("world", content);
        }

        [Fact]
        public void IsFileDifferent_ReturnsFalseForIdenticalFiles()
        {
            // Arrange
            _tempFile1 = Path.GetTempFileName();
            _tempFile2 = Path.GetTempFileName();
            var content = "line1\nline2\nline3";
            File.WriteAllText(_tempFile1, content);
            File.WriteAllText(_tempFile2, content);

            // Act
            var result = SysUtils.IsFileDifferent(_tempFile1, _tempFile2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsFileDifferent_ReturnsTrueForDifferentFiles()
        {
            // Arrange
            _tempFile1 = Path.GetTempFileName();
            _tempFile2 = Path.GetTempFileName();
            File.WriteAllText(_tempFile1, "line1\nline2\nline3");
            File.WriteAllText(_tempFile2, "line1\nDIFFERENT\nline3");

            // Act
            var result = SysUtils.IsFileDifferent(_tempFile1, _tempFile2);

            // Assert
            Assert.True(result);
        }
    }
}