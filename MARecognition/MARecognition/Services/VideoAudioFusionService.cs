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
        private readonly EventLogManagerService _eventLogManager;

        public VideoAudioFusionService(
            FrameExtractorService frameExtractor,
            VideoAnalyzerService videoAnalyzer,
            AudioTranscriptionService audioTranscription,
            AudioActivityRecoService audioRecognition,
            EventLogManagerService eventLogManager)
        {
            _frameExtractor = frameExtractor;
            _videoAnalyzer = videoAnalyzer;
            _audioTranscription = audioTranscription;
            _audioRecognition = audioRecognition;
            _eventLogManager = eventLogManager;
        }


        public async Task<List<EventLogItem>> AnalyzeAsync(string videoPath, string audioPath, string framesFolder)
        {
            // Video extract frames
            int totalFrames = _frameExtractor.ExtractFrames(videoPath, framesFolder, fpsToExtract: 1);
            if (totalFrames < 3)
                return new List<EventLogItem>();

            // Video recognition
            var videoIntervals = await _videoAnalyzer.RecognizeVideoActions(framesFolder, totalFrames);
            var videoItems = videoIntervals
                .Select(interval => new EventLogItem(
                    activity: interval.Activity.Trim().ToLower().TrimEnd('.', ' '),
                    timestamp: interval.StartTimestamp,
                    caseId: null
                ))
                .ToList();

            // Audio transcription
            string transcription = await _audioTranscription.TranscribeAsync(audioPath);

            // Audio activity recognition
            var audioItems = await _audioRecognition.RecognizeActivitiesAsync(transcription, startTimeSeconds: 0);

            // Merge
            var finalLog = _eventLogManager.CreateMultimodalEventLog(videoItems, audioItems);

            return finalLog;
        }
    }
}
