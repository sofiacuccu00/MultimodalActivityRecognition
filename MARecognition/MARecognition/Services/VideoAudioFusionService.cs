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

        /// <summary>
        /// Analizza video e audio e restituisce il log finale multimodale.
        /// </summary>
        /// <param name="videoPath">Percorso video (mp4)</param>
        /// <param name="audioPath">Percorso audio (wav)</param>
        /// <param name="framesFolder">Cartella dove salvare i frame</param>
        /// <returns>Log multimodale pronto per process mining</returns>
        public async Task<List<EventLogItem>> AnalyzeAsync(string videoPath, string audioPath, string framesFolder)
        {
            // 1️⃣ Estrai i frame dal video
            int totalFrames = _frameExtractor.ExtractFrames(videoPath, framesFolder, fpsToExtract: 1);
            if (totalFrames < 3)
                return new List<EventLogItem>(); // non abbastanza frame

            // 2️⃣ Analizza i frame (Video)
            var videoIntervals = await _videoAnalyzer.RecognizeVideoActions(framesFolder, totalFrames);

            // 3️⃣ Trasforma EventLogInterval -> EventLogItem
            var videoItems = videoIntervals
                .Select(interval => new EventLogItem(
                    activity: interval.Activity.Trim().ToLower().TrimEnd('.', ' '), // rimuove maiuscole e punti
                    timestamp: interval.StartTimestamp,
                    caseId: null
                ))
                .ToList();


            // 4️⃣ Analizza l’audio (Drop detection)
            double dropTime = _audioService.DetectDropEvent(audioPath);

            // 5️⃣ Crea log multimodale finale (video + drop audio)
            var finalLog = _eventLogManager.CreateMultimodalEventLog(videoItems, dropTime);

            return finalLog;
        }
    }
}
