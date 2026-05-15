// 시스템 유틸리티, 컬렉션, 비동기 처리를 위한 기본 네임스페이스 선언
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// Firebase Realtime Database와 통신하기 위한 FireSharp 라이브러리 네임스페이스 선언
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
// 데이터 저장을 위한 모델(구조체) 네임스페이스 선언
using codinglearning.Models;

namespace codinglearning.Services
{
    // Firebase 데이터베이스와의 데이터 쓰기/읽기 작업을 전담하는 매니저 클래스
    public class FirebaseManager
    {
        // 현재 앱을 사용하는 사용자의 고유 ID (여기서는 테스트용 ID로 하드코딩되어 있음)
        private IFirebaseClient client;
        // 현재 앱을 사용하는 사용자의 고유 ID (여기서는 테스트용 ID로 하드코딩되어 있음)
        private string uid = "user_test_01";

        // Firebase 데이터베이스와의 초기 연결을 설정하고 객체를 생성하는 메서드
        public bool Initialize()
        {
            // Firebase 프로젝트 설정 객체 생성
            IFirebaseConfig config = new FirebaseConfig
            {
                // 데이터베이스 접근 권한을 위한 비밀키(Auth Secret) 설정
                AuthSecret = "0B1EXHVODFmQ59TcJHQCIV8hyPg6hErBNyMFJw2k",
                // Firebase Realtime Database의 루트 URL 주소 설정
                BasePath = "https://codinglearning-bfe8f-default-rtdb.firebaseio.com/"
            };
            // 설정한 구성값을 바탕으로 실제 Firebase 클라이언트 인스턴스 생성
            client = new FireSharp.FirebaseClient(config);
            // 클라이언트 객체가 정상적으로 생성되었는지 여부(true/false) 반환
            return client != null;
        }

        // 사용자의 현재 학습 상태(세션 데이터)를 데이터베이스에 덮어쓰기(Set) 방식으로 저장하는 비동기 메서드
        public async Task SaveSessionAsync(SessionData data)
        {
            // 'learningStatus/유저ID' 경로에 데이터를 저장합니다. (기존 데이터가 있으면 덮어씀)
            await client.SetAsync($"learningStatus/{uid}", data);
        }

        // 완료된 학습 세션의 로그를 누적 기록하기 위해 추가(Push) 방식으로 저장하는 비동기 메서드
        public async Task PushSessionLogAsync(SessionData data)
        {
            // 'sessionLogs/유저ID' 경로에 데이터 푸시 (고유 해시값이 생성되며 리스트처럼 데이터가 누적됨)
            await client.PushAsync($"sessionLogs/{uid}", data);
        }

        // 데이터베이스에 누적된 모든 학습 세션 로그를 가져오는 비동기 메서드
        public async Task<Dictionary<string, SessionData>> GetAllSessionLogsAsync()
        {
            // 해당 유저의 모든 세션 로그 경로로 GET 요청 보냄
            FirebaseResponse res = await client.GetAsync($"sessionLogs/{uid}");
            // 만약 데이터가 없어서 반환된 본문이 "null" 문자열이라면 null 반환
            if (res.Body == "null") return null;
            // JSON 형태의 응답 데이터를 파싱하여 C#의 딕셔너리 자료구조로 변환 후 반환
            return res.ResultAs<Dictionary<string, SessionData>>();
        }

        // 사용자가 코드포스 문제를 채점(제출)했을 때, 그 결과와 내역을 저장하는 핵심 비동기 메서드
        public async Task SaveSubmissionAsync(string problemId, SubmissionRecord record, string diff, string tags)
        {
            // 해당 문제의 시도 내역(attempts) 리스트에 이번 제출 기록(정답/오답, 코드 등)을 누적(Push) 추가
            await client.PushAsync($"submissions/{uid}/{problemId}/attempts", record);

            // 만약 이번 채점 결과가 오답("wrong") 이라면 오답 노트에 등록해야 함
            if (record.status == "wrong")
            {
                // 오답 노트에 저장할 새 데이터 모델 구성
                var wrongData = new WrongProblemData
                {
                    // 발생 일자
                    addedDate = record.date,
                    // 에빙하우스 복습 주기 등을 고려하여 현재 시간 기준 3일 뒤를 복습 예정일로 지정
                    reviewDate = DateTime.Now.AddDays(3).ToString("yyyy-MM-dd HH:mm:ss"),

                    // 아직 해결하지 못했으므로 false로 설정
                    solvedAfter = false,

                    // 문제 제목, 난이도, 태그 등 메타 데이터 삽입
                    title = record.title,
                    diff = diff,
                    tags = tags
                };
                // 'wrongList/유저ID/문제ID' 경로에 이 오답 데이터를 덮어쓰기(Set) 형태로 저장
                await client.SetAsync($"wrongList/{uid}/{problemId}", wrongData);
            }
            else // 제출 결과가 정답(correct)일 경우
            {
                // 혹시 이전에 틀려서 오답 노트(wrongList)에 올라가 있는 문제인지 확인하기 위해 데이터 조회
                var res = await client.GetAsync($"wrongList/{uid}/{problemId}");
                // 만약 오답 노트에 해당 문제가 존재한다면 (응답이 null이 아니라면)
                if (res.Body != "null")
                {
                    // 오답 노트를 드디어 해결했다는 의미로 'solvedAfter' 속성만 true로 부분 업데이트(Update)
                    await client.UpdateAsync($"wrongList/{uid}/{problemId}", new { solvedAfter = true });
                }
            }
        }

        // 데이터베이스에 등록되어 있는 사용자의 오답 노트 전체 목록을 가져오는 비동기 메서드
        public async Task<Dictionary<string, dynamic>> GetWrongListAsync()
        {
            // 오답 리스트 경로에 GET 요청
            FirebaseResponse res = await client.GetAsync($"wrongList/{uid}");
            // 데이터가 없으면 null 반환
            if (res.Body == "null") return null;
            // 데이터를 동적(dynamic) 딕셔너리 형태로 파싱하여 반환
            return res.ResultAs<Dictionary<string, dynamic>>();
        }

        // 사용자가 제출했던 모든 문제의 전체 시도 내역(성공, 실패 포함)을 가져오는 비동기 메서드
        public async Task<Dictionary<string, dynamic>> GetAllSubmissionsAsync()
        {
            // 전체 제출 내역 경로에 GET 요청
            FirebaseResponse res = await client.GetAsync($"submissions/{uid}");
            // 데이터가 없으면 null 반환
            if (res.Body == "null") return null;
            // JSON 데이터를 딕셔너리로 변환하여 반환
            return res.ResultAs<Dictionary<string, dynamic>>();
        }
    }
}