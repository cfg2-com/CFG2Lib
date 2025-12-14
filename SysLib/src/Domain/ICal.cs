using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CFG2.Utils.SysLib.Domain;

public class ICal
{
    private string _name;
    private List<ICalEvent> _events;

    public ICal(string name, List<ICalEvent>? events = null)
    {
        _name = name;
        _events = events ?? [];
    }

    public string Name => _name;
    public List<ICalEvent> Events => _events;

    public void AddEvent(ICalEvent newEvent)
    {
        _events.Add(newEvent);
    }

    public void WriteFeedToFile(string folder, string fileName)
    {
        if (!Directory.Exists(folder))
        {
            throw new FileNotFoundException($"Folder not found: {folder}");
        }

        string fullpath = Path.Combine(folder, fileName);
        File.WriteAllText(fullpath, GetFeedString());
    }

    public string GetFeedString()
    {
        DateTime utcNow = DateTime.UtcNow;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//CFG2//SysLib//EN");
        sb.AppendLine($"X-WR-CALNAME:{Name}");

        foreach (ICalEvent curEvent in _events)
        {
            string startStr = $"{curEvent.Start.ToUniversalTime():yyyyMMddTHHmmssZ}";
            string endStr = $"{curEvent.End.ToUniversalTime():yyyyMMddTHHmmssZ}";

            if (curEvent.AllDay)
            {
                startStr = $"{curEvent.Start.ToUniversalTime():yyyyMMdd}";
                endStr = $"{curEvent.End.ToUniversalTime():yyyyMMdd}";
            }

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{curEvent.ID}");
            sb.AppendLine($"DTSTAMP:{utcNow:yyyyMMddTHHmmssZ}"); // Last created/updated
            sb.AppendLine($"DTSTART:{startStr}");
            sb.AppendLine($"DTEND:{endStr}");
            sb.AppendLine($"SUMMARY:{curEvent.Summary}");
            sb.AppendLine($"DESCRIPTION:{curEvent.Description}");
            sb.AppendLine($"LOCATION:{curEvent.Location}");
            sb.AppendLine("END:VEVENT");
        }
        sb.AppendLine("END:VCALENDAR");

        return sb.ToString();
    }
}

public class ICalEvent
{
    private string _id;
    private string _desc;

    public ICalEvent(string id, string summary = "", string? description = "", string? location = "", bool allDay = false, DateTime? start = null, DateTime? end = null)
    {
        description ??= "";
        location ??= "";
        start ??= DateTime.UtcNow;
        end ??= start;

        _id = id;
        Summary = summary;
        Description = description;
        Location = location;
        Start = (DateTime)start;
        End = (DateTime)end;
        AllDay = allDay;
    }

    public string ID => _id;
    public string Summary { get; set; }
    public string Description {
        get => GetEventDescription(_desc);
        set => _desc = value;
    }
    public string Location { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }

    private static string GetEventDescription(string desc)
    {
        if (string.IsNullOrEmpty(desc))
            return string.Empty;

        // Replace actual newlines with \n
        string escaped = desc.Replace("\r\n", "\\n").Replace("\n", "\\n");

        // Fold lines to max 75 characters per iCalendar spec
        StringBuilder result = new StringBuilder();
        int pos = 0;
        while (pos < escaped.Length)
        {
            int len = Math.Min(75, escaped.Length - pos);
            if (pos > 0)
            {
                result.Append("\r\n "); // Fold with space continuation
            }
            result.Append(escaped.Substring(pos, len));
            pos += len;
        }
        return result.ToString();
    }
}