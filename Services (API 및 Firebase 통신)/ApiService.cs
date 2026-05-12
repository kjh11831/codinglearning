using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace codinglearning.Services
{
    public class ApiService
    {
        // 1. HttpClient를 정적 생성자나 초기화 메서드에서 상세히 세팅합니다.
        private static readonly HttpClient httpClient = CreateConfiguredHttpClient();

        private static HttpClient CreateConfiguredHttpClient()
        {
            // GZip, Deflate 압축을 자동으로 풀도록 핸들러 설정 (브라우저의 기본 동작)
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler);

            // 2. 브라우저와 완벽하게 동일한 헤더 세팅
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            return client;
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


        public async Task<(string input, string output, string errorMessage)> FetchSampleDataFromWebAsync(string problemId)
        {
            try
            {
                string contestId = new String(problemId.Where(Char.IsDigit).ToArray());
                string index = new String(problemId.Where(Char.IsLetter).ToArray());
                string url = $"https://mirror.codeforces.com/problemset/problem/{contestId}/{index}";

                // 🌟 핵심: 크롬 브라우저인 척하는 User-Agent(신분증) 헤더 추가
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                    var response = await httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return ("", "", $"서버 접근 거부 (상태 코드: {(int)response.StatusCode})");
                    }

                    string html = await response.Content.ReadAsStringAsync();

                    // 정규식으로 <div class="input"> 안의 <pre> 내용 추출
                    Match inputMatch = Regex.Match(html, @"<div class=""input"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    Match outputMatch = Regex.Match(html, @"<div class=""output"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    if (inputMatch.Success && outputMatch.Success)
                    {
                        string input = CleanHtmlText(inputMatch.Groups[1].Value);
                        string output = CleanHtmlText(outputMatch.Groups[1].Value);
                        return (input, output, ""); // 에러 없음!
                    }
                    else
                    {
                        return ("", "", "Codeforces 페이지의 HTML 구조가 변경되어 예제 데이터를 찾지 못했습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                return ("", "", $"네트워크 통신 오류: {ex.Message}");
            }
        }

        // HTML 태그 청소기 (최신 Codeforces <div> 줄바꿈 구조 대응)
        private string CleanHtmlText(string html)
        {
            // <div class="test-example-line"> 등으로 감싸진 텍스트를 줄바꿈으로 변환
            string text = Regex.Replace(html, @"<div[^>]*>", "\n", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

            // 나머지 찌꺼기 HTML 태그 제거
            text = Regex.Replace(text, @"<.*?>", "");

            // HTML 특수문자(&gt;, &amp; 등) 원상복구
            text = System.Net.WebUtility.HtmlDecode(text);

            // 불필요한 연속 공백 및 위아래 여백 제거
            return Regex.Replace(text, @"^\s+$[\r\n]*", "", RegexOptions.Multiline).Trim();
        }

        internal async Task<JArray> FetchCodeforcesProblemsAsync()
        {
            try
            {
                string url = "https://codeforces.com/api/problemset.problems";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(responseBody);

                if (data["status"]?.ToString() == "OK")
                {
                    return (JArray)data["result"]["problems"];
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"문제 목록을 가져오는 중 오류가 발생했습니다:\n{ex.Message}");
            }
            return null;
        }
    }
}