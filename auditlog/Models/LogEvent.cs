namespace groveale.Models
{
    public class LogEvent
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string EventMessage { get; set; }
        public string EventDetails { get; set; }
        public string EventCategory { get; set; }
        public DateTime EventTime { get; set; }
    }
}