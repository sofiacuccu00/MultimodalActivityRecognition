using NAudio.Wave;

namespace MARecognition.Services
{
    public class AudioPeakDetectService
    {

        public double GetLoudestTimestamp(string audioFilePath)
        {
            if (!File.Exists(audioFilePath))
                throw new FileNotFoundException(audioFilePath);

            using var reader = new AudioFileReader(audioFilePath);

            float[] buffer = new float[reader.WaveFormat.SampleRate];
            int samplesRead;

            double maxAmplitude = 0;
            long maxSamplePosition = 0;
            long totalSamplesRead = 0;

            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    float amplitude = Math.Abs(buffer[i]);

                    if (amplitude > maxAmplitude)
                    {
                        maxAmplitude = amplitude;
                        maxSamplePosition = totalSamplesRead + i;
                    }
                }

                totalSamplesRead += samplesRead;
            }

            // Converts sample in seconds
            double seconds =
                (double)maxSamplePosition / reader.WaveFormat.SampleRate;

            return seconds;
        }
    }
}
