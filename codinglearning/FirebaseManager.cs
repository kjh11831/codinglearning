using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using codinglearning.Models;

namespace codinglearning.Services
{
    public class FirebaseManager
    {
        private IFirebaseClient client;
        private string uid = "user_test_01"; // 임시 유저 ID

        public bool Initialize()
        {
            IFirebaseConfig config = new FirebaseConfig
            {
                AuthSecret = "0B1EXHVODFmQ59TcJHQCIV8hyPg6hErBNyMFJw2k",
                BasePath = "https://codinglearning-bfe8f-default-rtdb.firebaseio.com/"
            };

            client = new FireSharp.FirebaseClient(config);
            return client != null;
        }

        // 1. 현재 학습 상태 저장
        public async Task SaveSessionAsync(SessionData data)
        {
            await client.SetAsync($"learningStatus/{uid}", data);
        }

        // 2. 학습 기록 '누적' 저장 (그래프용)
        public async Task PushSessionLogAsync(SessionData data)
        {
            await client.PushAsync($"sessionLogs/{uid}", data);
        }

        // 3. 누적된 전체 학습 기록 불러오기 (그래프용)
        public async Task<Dictionary<string, SessionData>> GetAllSessionLogsAsync()
        {
            FirebaseResponse res = await client.GetAsync($"sessionLogs/{uid}");
            if (res.Body == "null") return null;
            return res.ResultAs<Dictionary<string, SessionData>>();
        }

        public async Task SaveSubmissionAsync(string problemId, SubmissionRecord record, string diff, string tags)
        {
            // 1. 제출 기록 누적[cite: 1]
            await client.PushAsync($"submissions/{uid}/{problemId}/attempts", record);

            // 2. 오답 노트 갱신[cite: 1]
            if (record.status == "wrong")
            {
                var wrongData = new WrongProblemData
                {
                    addedDate = record.date,
                    solvedAfter = false,
                    title = record.title,
                    diff = diff,
                    tags = tags
                };
                await client.SetAsync($"wrongList/{uid}/{problemId}", wrongData);
            }
            else // 정답일 경우 해결 처리
            {
                var res = await client.GetAsync($"wrongList/{uid}/{problemId}");
                if (res.Body != "null")
                {
                    await client.UpdateAsync($"wrongList/{uid}/{problemId}", new { solvedAfter = true });
                }
            }
        }

        public async Task<Dictionary<string, dynamic>> GetWrongListAsync()
        {
            FirebaseResponse res = await client.GetAsync($"wrongList/{uid}");
            if (res.Body == "null") return null;
            return res.ResultAs<Dictionary<string, dynamic>>();
        }

        public async Task<Dictionary<string, dynamic>> GetAllSubmissionsAsync()
        {
            FirebaseResponse res = await client.GetAsync($"submissions/{uid}");
            if (res.Body == "null") return null;
            return res.ResultAs<Dictionary<string, dynamic>>();
        }
    }
}