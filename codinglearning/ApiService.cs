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

        // Codeforces 문제 목록 조회[cite: 1]
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

        // Judge0 예제 코드 채점 실행
        public async Task<(bool isCorrect, string message)> RunJudge0Async(string code, string language)
        {
            int langId = 51; // 기본값 C#
            if (language == "C++") langId = 54;
            else if (language == "Python") langId = 71;
            else if (language == "Java") langId = 62;

            var requestData = new { source_code = code, language_id = langId };
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url = "https://ce.judge0.com/submissions/?base64_encoded=false&wait=true";
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);

            // 방어 코드: 응답에 "status"가 아예 없는 경우 안전하게 예외 처리
            if (result["status"] == null)
            {
                // 서버가 보낸 에러 메시지(error나 message)를 찾아서 보여줌
                string apiError = result["error"]?.ToString() ?? result["message"]?.ToString() ?? "알 수 없는 API 오류";
                return (false, $"❌ 서버 오류: {apiError}\n서버 응답: {responseBody}");
            }

            // ?. 연산자를 사용해 값이 null이어도 프로그램이 터지지 않도록 보호
            bool isCorrect = result["status"]["id"]?.ToString() == "3"; // 3 = Accepted
            string statusDesc = result["status"]["description"]?.ToString() ?? "상태 알 수 없음";
            string errorOutput = result["compile_output"]?.ToString() ?? result["stderr"]?.ToString() ?? "";

            string msg = isCorrect ? $"✅ 성공 ({statusDesc})\n이제 CF 제출을 진행해보세요." : $"❌ 실패: {statusDesc}\n{errorOutput}";
            return (isCorrect, msg);
        }
    }
}