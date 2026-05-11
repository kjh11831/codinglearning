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

        public async Task<(bool isCorrect, string message)> RunJudge0Async(string code, string language)
        {
            int langId = 51;
            if (language == "C++") langId = 54;
            else if (language == "Python") langId = 71;
            else if (language == "Java") langId = 62;

            string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            string dummyInput = "1\n5\n1 2 3 4 5\n";
            string base64Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(dummyInput));

            var requestData = new
            {
                source_code = base64Code,
                language_id = langId,
                stdin = base64Stdin
            };
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);

            if (result["status"] == null)
            {
                string apiError = result["error"]?.ToString() ?? result["message"]?.ToString() ?? "알 수 없는 API 오류";
                return (false, $"❌ 서버 오류: {apiError}\n서버 응답: {responseBody}");
            }

            bool isCorrect = result["status"]["id"]?.ToString() == "3";
            string statusDesc = result["status"]["description"]?.ToString() ?? "상태 알 수 없음";

            string GetDecodedString(JToken token)
            {
                string str = token?.ToString();
                if (string.IsNullOrEmpty(str)) return "";
                try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                catch { return str; }
            }

            string errorOutput = GetDecodedString(result["compile_output"]) + "\n" + GetDecodedString(result["stderr"]);
            string msg = isCorrect ? $"✅ 성공 ({statusDesc})\n이제 CF 제출을 진행해보세요." : $"❌ 실패: {statusDesc}\n{errorOutput.Trim()}";
            return (isCorrect, msg);
        }
    }
}