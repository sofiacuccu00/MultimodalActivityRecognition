using MARecognition.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MARecognition.Services
{
    public class VideoAudioFusionService
    {
        private readonly FrameExtractorService _frameExtractor;
        private readonly VideoAnalyzerService _videoAnalyzer;
        private readonly AudioDropDetectionService _audioService;
        private readonly EventLogManagerService _eventLogManager;

        public VideoAudioFusionService(
            FrameExtractorService frameExtractor,
            VideoAnalyzerService videoAnalyzer,
            AudioDropDetectionService audioService,
            EventLogManagerService eventLogManager)
        {
            _frameExtractor = frameExtractor;
            _videoAnalyzer = videoAnalyzer;
            _audioService = audioService;
            _eventLogManager = eventLogManager;
        }


        // Analyses video and audio and returns the final multimodal log
        public async Task<List<EventLogItem>> AnalyzeAsync(string videoPath, string audioPath, string framesFolder)
        {
            // frames extraction
            int totalFrames = _frameExtractor.ExtractFrames(videoPath, framesFolder, fpsToExtract: 1);
            if (totalFrames < 3)
                return new List<EventLogItem>(); // non abbastanza frame

            // video frames analisys
            var videoIntervals = await _videoAnalyzer.RecognizeVideoActions(framesFolder, totalFrames);

            // trasforming EventLogInterval -> EventLogItem
            var videoItems = videoIntervals
                .Select(interval => new EventLogItem(
                    activity: interval.Activity.Trim().ToLower().TrimEnd('.', ' '), // deletes capital letters and periods
                    timestamp: interval.StartTimestamp,
                    caseId: null
                ))
                .ToList();


            // Audioanalyse (Drop detection)
            double dropTime = _audioService.DetectDropEvent(audioPath);

            // Creates final multimodal log (video + drop audio)
            var finalLog = _eventLogManager.CreateMultimodalEventLog(videoItems, dropTime);

            return finalLog;
        }
    }
}
