using MARecognition.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MARecognition.Services
{
    public class EventLogManagerService
    {
        /// <summary>
        /// Scrive la lista di EventLogItem in un CSV.
        /// </summary>
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

        /// <summary>
        /// Rimuove le azioni consecutive duplicate.
        /// </summary>
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


        /// <summary>
        /// Crea il log multimodale combinando video e audio.
        /// Togli le azioni video dopo il drop audio e unisci con l'evento drop audio.
        /// </summary>
        public List<EventLogItem> CreateMultimodalEventLog(
            List<EventLogItem> videoItems,
            double audioDropTime,
            string audioActivityName = "drop")
        {
            if (videoItems == null)
                throw new ArgumentNullException(nameof(videoItems));

            // Accorpa duplicati consecutivi
            var cleanedVideo = ClearEventLog(videoItems);

            // Tieni solo azioni video precedenti al drop audio
            var filteredVideo = cleanedVideo
                .Where(v => v.Timestamp < Math.Floor(audioDropTime))
                .ToList();

            // Crea evento drop audio
            var audioEvent = new EventLogItem(audioActivityName, (int)Math.Floor(audioDropTime));



            // Unisci e ordina per timestamp
            var finalLog = filteredVideo.Append(audioEvent)
                                        .OrderBy(e => e.Timestamp)
                                        .ToList();

            return finalLog;
        }
    }
}
