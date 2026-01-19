namespace MARecognition.Miscellaneuous
{
    public class FusionRequest
    {
        public string? Context { get; set; } = string.Empty;

        // Example: { "vision": "walking", "audio": "running", "imu": "walking" }
        public Dictionary<string, string>? Predictions { get; set; }
            = new Dictionary<string, string>();
    }
}
