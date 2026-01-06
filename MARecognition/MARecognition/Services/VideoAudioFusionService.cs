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
            // 1️⃣ Estrarre frames dal video
            int totalFrames = _frameExtractor.ExtractFrames(videoPath, framesFolder, fpsToExtract: 1);
            if (totalFrames < 3)
                return new List<EventLogItem>();

            // 2️⃣ Riconoscimento azioni video
            var videoIntervals = await _videoAnalyzer.RecognizeVideoActions(framesFolder, totalFrames);

            // Genera un CaseId unico per questo video/audio
            var caseId = Guid.NewGuid().ToString();

            var videoItems = videoIntervals
                .Select(interval => new EventLogItem(
                    activity: interval.Activity.Trim().ToLower().TrimEnd('.', ' '),
                    timestamp: interval.StartTimestamp,
                    caseId: caseId
                ))
                .ToList();

            // 3️⃣ Trascrizione audio
            string transcription = await _audioTranscription.TranscribeAsync(audioPath);

            // 4️⃣ Riconoscimento attività audio
            var audioItems = await _audioRecognition.RecognizeActivitiesAsync(transcription, startTimeSeconds: 0);

            // Assegna lo stesso CaseId anche alle attività audio
            audioItems.ForEach(item => item.CaseId = caseId);

            // 5️⃣ Fusion multimodale e rimozione duplicati
            var finalLog = _eventLogManager.CreateMultimodalEventLog(videoItems, audioItems);

            // 6️⃣ Scrittura CSV pronto per download
            Directory.CreateDirectory("output"); // crea cartella se non esiste
            string csvPath = Path.Combine("output", "multimodal_log.csv");
            _eventLogManager.WriteEventLog(finalLog, csvPath);


            // 7️⃣ Ritorna il log finale
            return finalLog;
        }

    }
}
