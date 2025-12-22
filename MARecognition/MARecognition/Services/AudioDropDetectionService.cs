using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace MARecognition.Services
{
    public class AudioDropDetectionService
    {

        //Detect the loudest impact ("drop") in a WAV file.
        // returns Time in seconds of the loudest sound event
        public double DetectDropEvent(string audioFilePath, int windowMs = 10)
        {
            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException($"File not found: {audioFilePath}");

            var sampleList = new List<float>();
            int sampleRate;

            using (var reader = new AudioFileReader(audioFilePath)) 
            {
                sampleRate = reader.WaveFormat.SampleRate;
                float[] buffer = new float[reader.WaveFormat.Channels];
                int read;

                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // If stereo, average channels to mono
                    float sampleValue = buffer.Take(read).Average();
                    sampleList.Add(sampleValue);
                }
            }

            if (sampleList.Count == 0)
                throw new InvalidOperationException("No audio samples read.");

            // Normalize
            var maxAbs = sampleList.Max(s => Math.Abs(s));
            if (maxAbs > 0)
                sampleList = sampleList.Select(s => s / maxAbs).ToList();

            // Compute envelope (absolute value)
            var envelope = sampleList.Select(Math.Abs).ToArray();

            // Smoothing window
            int windowSize = (int)(sampleRate * windowMs / 1000.0);
            if (windowSize < 1) windowSize = 1;

            var smoothed = new double[envelope.Length];
            for (int i = 0; i < envelope.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(envelope.Length - 1, i + windowSize / 2);
                smoothed[i] = 0;
                for (int j = start; j <= end; j++)
                    smoothed[i] += envelope[j];
                smoothed[i] /= (end - start + 1);
            }

            // Find peak
            int peakIndex = Array.IndexOf(smoothed, smoothed.Max());
            double peakTime = (double)peakIndex / sampleRate;

            return peakTime;
        }

        // Write detected drop times or activities to a text file.
        public void WriteActivitiesToFile(List<string> activities, string outputFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? "");
            File.WriteAllLines(outputFilePath, activities);
        }
    }
}
