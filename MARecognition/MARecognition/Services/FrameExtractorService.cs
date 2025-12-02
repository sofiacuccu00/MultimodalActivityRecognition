namespace MARecognition.Services
{
    using OpenCvSharp;

    public class FrameExtractorService
    {
        public int ExtractFrames(string videoPath, string outputFolder, int fpsToExtract = 1)
        {
            Directory.CreateDirectory(outputFolder);
            using var capture = new VideoCapture(videoPath);
            if (!capture.IsOpened())
                throw new Exception("Cannot open video file.");

            double fps = capture.Fps;
            int totalFrames = capture.FrameCount;
            double duration = totalFrames / fps;

            Console.WriteLine($"Video FPS: {fps}, Total Frames: {totalFrames}, Duration: {duration:F2}s");

            int frameInterval = (int)(fps / fpsToExtract);
            int frameNumber = 0;
            int savedFrames = 0;

            using var frame = new Mat();
            while (capture.Read(frame))
            {
                if (frameNumber % frameInterval == 0)
                {
                    // fix frame dimension
                    Cv2.Resize(frame, frame, new Size(256, 256));
                    string filePath = Path.Combine(outputFolder, $"frame_{savedFrames:D4}.jpg");
                    Cv2.ImWrite(filePath, frame);
                    savedFrames++;
                }
                frameNumber++;
            }

            return savedFrames;
        }
    }

}

