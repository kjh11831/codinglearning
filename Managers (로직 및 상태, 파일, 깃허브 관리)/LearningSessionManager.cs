// 기본 시스템 라이브러리를 사용하기 위한 네임스페이스 선언
using System;

// 코딩 학습 애플리케이션의 '매니저' 역할을 하는 클래스들을 모아둔 네임스페이스
namespace codinglearning.Managers
{
    // 사용자의 학습 세션(공부 시간, 휴식, 자리 비움 등)을 관리하는 클래스
    public class LearningSessionManager
    {
        // 현재 세션의 상태(Active, Idle, Break 등)를 나타내는 문자열 속성
        // 외부에서는 읽기만 가능하며(get), 클래스 내부에서만 값을 변경(private set)할 수 있음
        // 기본값은 "Break"
        public string CurrentStatus { get; private set; } = "Break";

        // 세션(학습)이 처음 시작된 정확한 시각을 저장하는 속성
        // 역시 내부에서만 설정 가능
        public DateTime SessionStart { get; private set; }

        // 사용자가 마지막으로 마우스나 키보드 등의 활동을 한 시각을 기록하는 속성
        public DateTime LastActiveTime { get; private set; }

        // 새로운 학습 세션을 시작할 때 호출되는 메서드
        public void StartSession()
        {
            // 세션 시작 시간을 현재 PC 시간으로 기록
            SessionStart = DateTime.Now;
            // 세션이 시작되었으므로 마지막 활동 시간도 현재 시간으로 초기화
            LastActiveTime = DateTime.Now;
            // 상태를 학습 중임을 나타내는 "Active"로 변경
            CurrentStatus = "Active";
        }

        // 사용자가 화면을 클릭하거나 키보드를 입력하는 등 활동을 했을 때 호출되는 메서드
        public void RecordUserAction()
        {
            // 마지막 활동 시간을 지금 이 순간으로 갱신
            LastActiveTime = DateTime.Now;
            // 만약 현재 상태가 "Active"(학습 중)가 아니라면 (예: Idle 상태였다면)
            if (CurrentStatus != "Active")
            {
                // 상태를 다시 "Active"로 전환하여 학습 중임을 표시함
                CurrentStatus = "Active";
            }
        }

        // 사용자가 일정 시간 동안 아무런 활동이 없을 때 상태를 업데이트하기 위한 메서드 (보통 UI 타이머 등에서 주기적으로 호출됨)
        public void UpdateIdleState()
        {
            // 현재 상태가 "Active"이고, 지금 시간과 마지막 활동 시간의 차이(경과 시간)가 5분을 초과했다면
            if (CurrentStatus == "Active" && (DateTime.Now - LastActiveTime).TotalMinutes > 5)
            {
                // 상태를 자리 비움 또는 휴식을 의미하는 "Idle"로 변경
                CurrentStatus = "Idle";
            }
        }

        // 진행 중이던 학습 세션을 완전히 종료하고 휴식 상태로 들어갈 때 호출되는 메서드
        public void StopSession()
        {
            // 상태를 "Break"(휴식)로 변경
            CurrentStatus = "Break";
        }

        // 세션이 시작된 이후 지금까지 총 얼마의 시간이 흘렀는지(현재 진행 시간)를 계산하여 반환하는 메서드
        public TimeSpan GetCurrentDuration()
        {
            // 현재 시간에서 세션 시작 시간을 뺀 결과(TimeSpan 객체)를 반환
            return DateTime.Now - SessionStart;
        }

        // 총 누적 학습 시간을 '초' 단위의 정수로 계산하여 반환하는 메서드 (데이터베이스나 파일 저장 용도로 주로 쓰임)
        public int GetTotalSeconds()
        {
            // 현재 시간에서 시작 시간을 뺀 총 초(TotalSeconds)를 int형으로 형변환하여 반환
            return (int)(DateTime.Now - SessionStart).TotalSeconds;
        }
    }
}