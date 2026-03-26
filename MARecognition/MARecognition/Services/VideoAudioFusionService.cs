using MARecognition.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MARecognition.Services
{
    public class VideoAudioFusionService
    {
        private readonly FrameExtractorService _frameExtractor;
        private readonly VideoAnalyzerService _videoAnalyzer;
        private readonly AudioTranscriptionService _audioTranscription;
        private readonly AudioActivityRecoService _audioRecognition;
        private readonly AudioPeakDetectService _peakDetect;
        private readonly EventLogManagerService _eventLogManager;

        public VideoAudioFusionService(
            FrameExtractorService frameExtractor,
            VideoAnalyzerService videoAnalyzer,
            AudioTranscriptionService audioTranscription,
            AudioActivityRecoService audioRecognition,
            AudioPeakDetectService peakDetect,
            EventLogManagerService eventLogManager)
        {
            _frameExtractor = frameExtractor;
            _videoAnalyzer = videoAnalyzer;
            _audioTranscription = audioTranscription;
            _audioRecognition = audioRecognition;
            _peakDetect = peakDetect;
            _eventLogManager = eventLogManager;
        }

        public async Task<List<EventLogItem>> AnalyzeAsync(string videoPath, string audioPath, string framesFolder)
        {
            // Step 1: Extract video frames
            int totalFrames = _frameExtractor.ExtractFrames(videoPath, framesFolder, fpsToExtract: 1);
            if (totalFrames < 3)
                return new List<EventLogItem>();

            // Step 2: Video recognition
            var videoIntervals = await 
                _videoAnalyzer.RecognizeVideoActions(framesFolder, totalFrames);

            var caseId = Guid.NewGuid().ToString();
            var videoItems = videoIntervals
                .Select(interval => new EventLogItem(
                    activity: interval.Activity.Trim().ToLower(),
                    timestamp: interval.StartTimestamp,
                    caseId: caseId
                ))
                .ToList();

            // Step 3: Audio transcription
            string transcription = await _audioTranscription.TranscribeAsync(audioPath);

            // Step 4: Audio activity recognition
            var audioItems = await _audioRecognition.RecognizeActivitiesAsync(transcription, startTimeSeconds: 0);

            // Step 5: Audio peak detection
            double peakTime = _peakDetect.GetLoudestTimestamp(audioPath);
            // Add peak as a "drop" event if it is not already in audioItems
            if (!audioItems.Any(a => a.Activity == "drop"))
            {
                audioItems.Add(new EventLogItem("drop", (int)peakTime, caseId));
            }

            // Assign CaseId to audio events
            audioItems.ForEach(a => a.CaseId = caseId);

            // Step 6: Multimodal fusion
            var fusedLog = FuseVideoAudioLogs(videoItems, audioItems);

            // Write CSV
            Directory.CreateDirectory("output");
            string csvPath = Path.Combine("output", "multimodal_log.csv");
            _eventLogManager.WriteEventLog(fusedLog, csvPath);

            return fusedLog;
        }

        private List<EventLogItem> FuseVideoAudioLogs(List<EventLogItem> videoItems, List<EventLogItem> audioItems)
        {
            // Remove video drop events (we trust audio peak as reference)
            videoItems = videoItems.Where(v => v.Activity != "drop").ToList();

            // Merge video and audio events
            var merged = videoItems.Concat(audioItems).ToList();

            // Sort by timestamp
            merged = merged.OrderBy(e => e.Timestamp).ToList();

            // Remove consecutive duplicates
            var fused = new List<EventLogItem>();
            EventLogItem? last = null;
            foreach (var item in merged)
            {
                if (last == null || last.Activity != item.Activity)
                {
                    fused.Add(item);
                    last = item;
                }
            }

            return fused;
        }
    }
}