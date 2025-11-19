namespace MARecognition.Models
{
    public class EventLogItem
    {
        public string Activity { get; set; }
        public int Timestamp { get; set; }

        public EventLogItem(string activity, int timestamp)
        {
            Activity = activity;
            Timestamp = timestamp;
        }
    }

}
