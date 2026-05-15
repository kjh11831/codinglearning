// 기본 시스템 데이터 타입 및 제네릭 컬렉션(Dictionary 등)을 사용하기 위한 네임스페이스 선언
using System;
using System.Collections.Generic;

// 데이터베이스 모델(Model)과 전역 상수들을 모아둔 네임스페이스
// 프로그램 내에서 데이터를 주고받거나 저장할 때 기준이 되는 '틀(구조)' 정의
namespace codinglearning.Models
{
    // 애플리케이션 전체에서 공통으로 사용되는 상수(변하지 않는 값)들을 모아둔 정적(Static) 클래스
    public static class AppConstants
    {
        // Judge0 API(단순 코드 컴파일 및 예제 테스트 실행 서비스)에서 요구하는 프로그래밍 언어별 고유 ID를 매핑한 딕셔너리
        public static readonly Dictionary<string, int> Judge0LangIds = new Dictionary<string, int>
        {
            { "C#", 51 },
            { "C++", 54 },
            { "Java", 62 },
            { "Python", 71 } // 파이썬은 Judge0에서 71번 ID 사용
        };

        // Codeforces 사이트의 실제 제출 폼(select 상자)에서 사용하는 프로그래밍 언어별 고유 ID 값 딕셔너리
        public static readonly Dictionary<string, string> CFLangIds = new Dictionary<string, string>
        {
            { "C#", "65" },
            { "C++", "54" },
            { "Java", "60" },
            { "Python", "71" }
        };
    // AppConstants 클래스 종료
    }

    // 사용자의 한 번의 학습 세션(공부 시간, 휴식 기록 등)을 Firebase에 저장하기 위해 사용하는 데이터 모델
    public class SessionData
    {
        // 세션의 최종 상태를 나타냄 (예: "Break", "Active" 등)
        public string status { get; set; }
        // 세션이 완전히 종료된 시각을 문자열(yyyy-MM-dd HH:mm:ss 포맷)로 저장
        public string sessionEnd { get; set; }
        // 해당 세션 동안 사용자가 순수하게 집중한 총 학습 시간(초 단위) 저장
        public int sessionDuration { get; set; }
        // 세션 내에서 키보드나 마우스 등 마지막 활동이 감지된 시간 저장
        public string lastActiveTime { get; set; }
    }

    // 사용자가 문제를 풀고 제출한 단일 기록(정답/오답 내역, 코드 원본 등)을 Firebase에 저장하기 위한 데이터 모델
    public class SubmissionRecord
    {
        // 사용자가 실제로 작성하여 제출했던 소스 코드 원본 문자열
        public string code { get; set; }
        // 채점 결과 저장 (예: 정답이면 "correct", 오답이면 "wrong")
        public string status { get; set; }
        // 사용한 프로그래밍 언어 저장 (예: "C#", "C++" 등)
        public string language { get; set; }
        // 문제를 채점받은 날짜와 시간 저장
        public string date { get; set; }
        // 풀이한 문제의 제목(이름) 저장
        public string title { get; set; }
        // 해당 문제의 Codeforces 공식 난이도(Rating) 저장
        public string diff { get; set; }
        // 문제와 관련된 알고리즘 분류 태그들(예: "dp", "greedy" 등) 저장
        public string tags { get; set; }
    }

    // 오답 노트에 등록되는 문제의 정보 및 복습 주기를 관리하기 위한 구/신버전 호환 데이터 모델
    public class WrongProblemData
    {
        // 틀린 문제의 제목 저장
        public string title { get; set; }
        // 틀린 문제의 난이도 저장
        public string diff { get; set; }
        // 틀린 문제의 알고리즘 태그 정보 저장
        public string tags { get; set; }
        // 이 문제가 처음으로 오답 노트에 등록된 날짜 저장
        public string addedDate { get; set; }
        // 에빙하우스 망각 곡선 등을 바탕으로 다시 풀어봐야 할 "복습 예정일" 저장
        public string reviewDate { get; set; }
        // 오답 노트에 등록된 후, 결국 다시 풀어서 "정답(해결)"을 맞추었는지를 나타내는 부울(Boolean) 플래그
        public bool solvedAfter { get; set; }
    }
}