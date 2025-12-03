namespace MARecognition.Models
{
    public class EventLogItem
    {
        public string Activity { get; set; }
        public int Timestamp { get; set; }
        public string CaseId { get; set; } 
        public EventLogItem(string activity, int timestamp, string caseId = null)
        {
            Activity = activity;
            Timestamp = timestamp;
            CaseId = caseId;
        }

    }



}
