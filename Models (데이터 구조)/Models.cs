using System;

namespace codinglearning.Models
{
    public class SessionData
    {
        public string status { get; set; }
        public string sessionEnd { get; set; }
        public int sessionDuration { get; set; }
        public string lastActiveTime { get; set; }
    }

    public class SubmissionRecord
    {
        public string code { get; set; }
        public string status { get; set; }
        public string language { get; set; }
        public string date { get; set; }
        public string title { get; set; }
    }

    public class WrongProblemData
    {
        public string title { get; set; }
        public string diff { get; set; }
        public string tags { get; set; }
        public string addedDate { get; set; }
        public string reviewDate { get; set; }
        public bool solvedAfter { get; set; }
    }
}