using System;

namespace codinglearning.Managers
{
    public class LearningSessionManager
    {
        public string CurrentStatus { get; private set; } = "Break";
        public DateTime SessionStart { get; private set; }
        public DateTime LastActiveTime { get; private set; }

        public void StartSession()
        {
            SessionStart = DateTime.Now;
            LastActiveTime = DateTime.Now;
            CurrentStatus = "Active";
        }

        public void RecordUserAction()
        {
            LastActiveTime = DateTime.Now;
            if (CurrentStatus != "Active")
            {
                CurrentStatus = "Active";
            }
        }

        public void UpdateIdleState()
        {
            // 5분 이상 무반응 시 Idle 상태로 전환[cite: 1]
            if (CurrentStatus == "Active" && (DateTime.Now - LastActiveTime).TotalMinutes > 5)
            {
                CurrentStatus = "Idle";
            }
        }

        public void StopSession()
        {
            CurrentStatus = "Break";
        }

        public TimeSpan GetCurrentDuration()
        {
            return DateTime.Now - SessionStart;
        }

        public int GetTotalSeconds()
        {
            return (int)(DateTime.Now - SessionStart).TotalSeconds;
        }
    }
}