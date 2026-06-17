using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApp_Part3
{
    public class LogEntry
    {
        public string Timestamp { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class LogBook
    {
        private List<LogEntry> entries = new List<LogEntry>();
        private int maxEntries = 100;

        public event Action<string>? ActivityLogged;

        public void Log(string action, string description, string category = "General")
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Action = action,
                Description = description,
                Category = category
            };

            entries.Add(entry);

            if (entries.Count > maxEntries)
                entries.RemoveAt(0);

            ActivityLogged?.Invoke(description);
        }

        public List<LogEntry> GetRecentEntries(int count)
        {
            return entries
                .OrderByDescending(e => e.Timestamp)
                .Take(Math.Min(count, entries.Count))
                .ToList();
        }

        public string GetFormattedLog(int count)
        {
            var entries = GetRecentEntries(count);
            if (!entries.Any())
                return "📜 No recent activity to display.";

            var result = "📜 RECENT ACTIVITY LOG\n────────────────────────────────────────────────\n";
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                result += $"{i + 1}. {entry.Timestamp} - {entry.Action}\n";
                result += $"   {entry.Description}\n\n";
            }
            return result;
        }

        public void ClearLog()
        {
            entries.Clear();
            ActivityLogged?.Invoke("Activity log cleared");
        }

        public int Count => entries.Count;
    }
}