using CFG2.Utils.SysLib.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace CFG2.Utils.SysLib.Tests.Domain
{
    public class ICalTests
    {
        [Fact]
        public void ICalEvent_Constructor_SetsProperties()
        {
            // Arrange
            var id = "test-id";
            var summary = "Test Summary";
            var description = "Test Description";
            var location = "Test Location";
            var start = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2025, 1, 1, 11, 0, 0, DateTimeKind.Utc);
            var allDay = false;

            // Act
            var calEvent = new ICalEvent(id, summary, description, location, allDay, start, end);

            // Assert
            Assert.Equal(id, calEvent.ID);
            Assert.Equal(summary, calEvent.Summary);
            Assert.Equal(description, calEvent.Description);
            Assert.Equal(location, calEvent.Location);
            Assert.Equal(start, calEvent.Start);
            Assert.Equal(end, calEvent.End);
            Assert.Equal(allDay, calEvent.AllDay);
        }

        [Fact]
        public void ICal_Constructor_SetsNameAndEvents()
        {
            // Arrange
            var events = new List<ICalEvent> { new("event1") };

            // Act
            var iCal = new ICal("TestCal", events);

            // Assert
            Assert.Equal("TestCal", iCal.Name);
            Assert.Single(iCal.Events);
        }

        [Fact]
        public void ICal_AddEvent_AddsEventToList()
        {
            // Arrange
            var iCal = new ICal("TestCal");
            var newEvent = new ICalEvent("event2");

            // Act
            iCal.AddEvent(newEvent);

            // Assert
            Assert.Single(iCal.Events);
            Assert.Equal(newEvent, iCal.Events[0]);
        }

        [Fact]
        public void ICal_GetFeedString_NoEvents()
        {
            // Arrange
            var iCal = new ICal("Test Cal");

            // Act
            var feed = iCal.GetFeedString();

            // Assert
            Assert.StartsWith("BEGIN:VCALENDAR", feed);
            Assert.Contains("VERSION:2.0", feed);
            Assert.Contains("PRODID:-//CFG2//SysLib//EN", feed);
            Assert.Contains("X-WR-CALNAME:Test Cal", feed);
            Assert.EndsWith("END:VCALENDAR\r\n", feed);
            Assert.DoesNotContain("BEGIN:VEVENT", feed);
        }

        [Fact]
        public void ICal_GetFeedString_OneEvent()
        {
            // Arrange
            var start = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2025, 12, 1, 11, 0, 0, DateTimeKind.Utc);
            var calEvent = new ICalEvent("uid1", "Summary", "Description", "Location", false, start, end);
            var iCal = new ICal("MyCalendar", new List<ICalEvent> { calEvent });

            // Act
            var feed = iCal.GetFeedString();

            // Assert
            Assert.Contains("BEGIN:VEVENT", feed);
            Assert.Contains("UID:uid1", feed);
            Assert.Contains($"DTSTART:{start:yyyyMMddTHHmmssZ}", feed);
            Assert.Contains($"DTEND:{end:yyyyMMddTHHmmssZ}", feed);
            Assert.Contains("SUMMARY:Summary", feed);
            Assert.Contains("DESCRIPTION:Description", feed);
            Assert.Contains("LOCATION:Location", feed);
            Assert.Contains("END:VEVENT", feed);
        }

        [Fact]
        public void ICal_GetFeedString_OneAllDayEvent()
        {
            // Arrange
            var start = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2025, 12, 2, 0, 0, 0, DateTimeKind.Utc);
            var calEvent = new ICalEvent("uid2", "All Day Event", "", "", true, start, end);
            var iCal = new ICal("MyCalendar", new List<ICalEvent> { calEvent });

            // Act
            var feed = iCal.GetFeedString();

            // Assert
            Assert.Contains("BEGIN:VEVENT", feed);
            Assert.Contains("UID:uid2", feed);
            Assert.Contains($"DTSTART:{start:yyyyMMdd}", feed);
            Assert.Contains($"DTEND:{end:yyyyMMdd}", feed);
            Assert.DoesNotContain("DTSTART:20251201T", feed);
            Assert.Contains("SUMMARY:All Day Event", feed);
            Assert.Contains("END:VEVENT", feed);
        }

        [Fact]
        public void WriteFeedToFile_CreatesFile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), "ICalTests");
            Directory.CreateDirectory(tempDir);
            var fileName = "test.ics";
            var filePath = Path.Combine(tempDir, fileName);
            var iCal = new ICal("Test Calendar");
            iCal.AddEvent(new ICalEvent("event1", "Test Event"));

            // Act
            iCal.WriteFeedToFile(tempDir, fileName);

            // Assert
            Assert.True(File.Exists(filePath));
            var fileContent = File.ReadAllText(filePath);

            Assert.Equal(iCal.GetFeedString(), fileContent);

            // Cleanup
            File.Delete(filePath);
            Directory.Delete(tempDir);
        }

        [Fact]
        public void WriteFeedToFile_ThrowsExceptionForBadDirectory()
        {
            // Arrange
            var iCal = new ICal("Test Calendar");
            var badDir = Path.Combine(Path.GetTempPath(), "nonexistent_dir");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => iCal.WriteFeedToFile(badDir, "test.ics"));
        }
    }
}
