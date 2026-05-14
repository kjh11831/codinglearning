using System;
using System.Collections.Generic;

namespace codinglearning.Models
{
    public static class AppConstants
    {
        // Judge0 채점 서버용 언어 ID (ApiService.cs 에서 사용)
        public static readonly Dictionary<string, int> Judge0LangIds = new Dictionary<string, int>
        {
            { "C#", 51 },
            { "C++", 54 },
            { "Java", 62 },
            { "Python", 71 }
        };

        // Codeforces 자동 제출용 언어 ID (Form1.cs 에서 사용)
        public static readonly Dictionary<string, string> CFLangIds = new Dictionary<string, string>
        {
            { "C#", "65" },
            { "C++", "54" },
            { "Java", "60" },
            { "Python", "71" }
        };
    }

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
        public string diff { get; set; }
        public string tags { get; set; }
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