using MARecognition.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MARecognition.Services
{
    public class EventLogManagerService
    {

        // write EventLogItem list in a CSV
        public void WriteEventLog(List<EventLogItem> items, string filePath)
        {
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("No items to write.");

            using var writer = new StreamWriter(filePath);
            writer.WriteLine("case_id,activity,timestamp");

            foreach (var item in items)
            {
                writer.WriteLine($"{item.CaseId},{item.Activity},{item.Timestamp}");
            }
        }

        // Removes duplicate consecutive actions 
        public List<EventLogItem> ClearEventLog(List<EventLogItem> items)
        {
            if (items == null || items.Count == 0)
                return new List<EventLogItem>();

            var result = new List<EventLogItem>();
            EventLogItem? previous = null;

            foreach (var item in items)
            {
                var activity = item.Activity.Trim().ToLower().TrimEnd('.', ' ');

                if (previous == null || previous.Activity != activity)
                {
                    result.Add(new EventLogItem(activity, item.Timestamp, item.CaseId));
                    previous = new EventLogItem(activity, item.Timestamp, item.CaseId);
                }
            }

            return result;
        }


        //Creates multimodal log combining video and audio
        public List<EventLogItem> CreateMultimodalEventLog(
            List<EventLogItem> videoItems,
            List<EventLogItem> audioItems)
        {
            if (videoItems == null) videoItems = new List<EventLogItem>();
            if (audioItems == null) audioItems = new List<EventLogItem>();

            // Merge and sort per timestamp
            var finalLog = videoItems.Concat(audioItems)
                                     .OrderBy(e => e.Timestamp)
                                     .ToList();

            // removes duplicates
            return ClearEventLog(finalLog);
        }

    }
}
