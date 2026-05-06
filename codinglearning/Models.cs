using System;

namespace codinglearning.Models
{
    // Firebase에 저장할 세션 데이터[cite: 1]
    public class SessionData
    {
        public string status { get; set; }
        public string sessionEnd { get; set; }
        public int sessionDuration { get; set; }
        public string lastActiveTime { get; set; }
        public bool hwConnected { get; set; }
    }

    // 풀이 시도 기록 데이터[cite: 1]
    public class SubmissionRecord
    {
        public string code { get; set; }
        public string status { get; set; }
        public string language { get; set; }
        public string date { get; set; }
        public string title { get; set; }
    }

    // 오답 노트 데이터[cite: 1]
    public class WrongProblemData
    {
        public string title { get; set; }
        public string diff { get; set; }
        public string tags { get; set; }
        public string addedDate { get; set; }
        public bool solvedAfter { get; set; }
    }
}