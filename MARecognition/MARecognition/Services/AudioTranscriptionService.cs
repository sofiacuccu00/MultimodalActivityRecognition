using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MARecognition.Services
{
    public class AudioTranscriptionService
    {
        private readonly HttpClient _http;

        public AudioTranscriptionService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:11434")
            };
        }

        public async Task<string> TranscribeAsync(string audioFilePath)
        {
            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException(audioFilePath);

            // payload with local file path
            var payload = new
            {
                model = "karanchopda333/whisper",
                audio_file = Path.GetFullPath(audioFilePath), 
                prompt = "Transcribe the audio accurately. Describe any non-verbal sounds briefly.",
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);

            var response = await _http.PostAsync(
                "/api/generate",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ollama error {response.StatusCode}: {err}");
            }

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            return doc.RootElement
                      .GetProperty("response")
                      .GetString()?
                      .Trim() ?? "";
        }
    }
}
