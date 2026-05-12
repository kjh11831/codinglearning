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

        public async Task<(bool isCorrect, string message)> RunJudge0Async(string code, string language, string stdin, string expectedOutput)
        {
            int langId = 51;
            if (language == "C++") langId = 54;
            else if (language == "Python") langId = 71;
            else if (language == "Java") langId = 62;

            string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            string base64Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(stdin));
            string base64Expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedOutput));

            var requestData = new
            {
                source_code = base64Code,
                language_id = langId,
                stdin = base64Stdin,
                expected_output = base64Expected // 🌟 채점 서버에 정답지 전송
            };

            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);

            if (result["status"] == null)
            {
                return (false, "❌ API 응답 오류");
            }

            // status id 3 = Accepted (정답과 일치함)
            bool isCorrect = result["status"]["id"]?.ToString() == "3";
            string statusDesc = result["status"]["description"]?.ToString() ?? "상태 알 수 없음";

            string GetDecodedString(JToken token)
            {
                string str = token?.ToString();
                if (string.IsNullOrEmpty(str)) return "";
                try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                catch { return str; }
            }

            string actualOutput = GetDecodedString(result["stdout"]);
            string errorOutput = GetDecodedString(result["compile_output"]) + "\n" + GetDecodedString(result["stderr"]);

            string msg;
            if (isCorrect)
            {
                msg = $"✅ 성공 (Accepted)\n출력값: {actualOutput.Trim()}";
            }
            else
            {
                // 🌟 틀렸을 경우 기대값과 실제값을 비교해서 보여줌
                msg = $"❌ 실패 ({statusDesc})\n[기대값]:\n{expectedOutput.Trim()}\n\n[실제 출력]:\n{actualOutput.Trim()}\n\n{errorOutput.Trim()}";
            }

            return (isCorrect, msg);
        }
    }
}