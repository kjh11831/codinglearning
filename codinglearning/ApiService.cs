using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace codinglearning.Services
{
    public class ApiService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        // Codeforces 문제 목록 조회
        public async Task<JArray> FetchCodeforcesProblemsAsync()
        {
            string url = "https://codeforces.com/api/problemset.problems";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(responseBody);

            if (data["status"].ToString() == "OK")
            {
                return (JArray)data["result"]["problems"];
            }
            return null;
        }

        // ⭐ Judge0 예제 코드 채점 실행 (한글 깨짐 완벽 방지 Base64 적용판!)
        public async Task<(bool isCorrect, string message)> RunJudge0Async(string code, string language)
        {
            int langId = 51; // 기본값 C#
            if (language == "C++") langId = 54;
            else if (language == "Python") langId = 71;
            else if (language == "Java") langId = 62;

            // 1. 코드를 Base64로 안전하게 포장
            string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));

            // ⭐ [추가] 임시 테스트용 예제 입력값 (나중엔 Form 화면에 텍스트 박스를 추가해서 받아와야 해!)
            string dummyInput = "1\n5\n1 2 3 4 5\n"; // 예시 테스트 케이스
            string base64Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(dummyInput));

            // ⭐ 2. JSON에 stdin(입력값) 데이터 추가!
            var requestData = new
            {
                source_code = base64Code,
                language_id = langId,
                stdin = base64Stdin // 서버에 입력값도 안전하게 포장해서 같이 전달
            };

            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // url은 아까 바꾼 그대로 유지! (base64_encoded=true)
            string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);

            // 방어 코드: 응답에 "status"가 아예 없는 경우 안전하게 예외 처리
            if (result["status"] == null)
            {
                string apiError = result["error"]?.ToString() ?? result["message"]?.ToString() ?? "알 수 없는 API 오류";
                return (false, $"❌ 서버 오류: {apiError}\n서버 응답: {responseBody}");
            }
            
            bool isCorrect = result["status"]["id"]?.ToString() == "3"; // 3 = Accepted
            string statusDesc = result["status"]["description"]?.ToString() ?? "상태 알 수 없음";

            // 3. 서버가 결과도 Base64로 포장해서 돌려주기 때문에, 우리가 읽을 수 있게 다시 번역해 주는 도우미 함수
            string GetDecodedString(JToken token)
            {
                string str = token?.ToString();
                if (string.IsNullOrEmpty(str)) return "";
                try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                catch { return str; } // 혹시 일반 텍스트면 그냥 반환
            }

            string errorOutput = GetDecodedString(result["compile_output"]) + "\n" + GetDecodedString(result["stderr"]);

            string msg = isCorrect ? $"✅ 성공 ({statusDesc})\n이제 CF 제출을 진행해보세요." : $"❌ 실패: {statusDesc}\n{errorOutput.Trim()}";
            return (isCorrect, msg);
        }
    }
}