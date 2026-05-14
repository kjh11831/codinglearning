using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            if (!codinglearning.Models.AppConstants.Judge0LangIds.TryGetValue(language, out int langId))
            {
                return (false, $"❌ 지원하지 않는 언어입니다: {language}");
            }

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

        public async Task<(List<string> inputs, List<string> outputs, string errorMessage)> FetchSampleDataFromWebAsync(string problemId)
        {
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();

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
                        return (null, null, $"서버 접근 거부 (상태 코드: {(int)response.StatusCode})");
                    }

                    string html = await response.Content.ReadAsStringAsync();

                    // 🌟 Match가 아닌 Matches를 사용하여 페이지 내의 모든 예제를 찾습니다.
                    MatchCollection inputMatches = Regex.Matches(html, @"<div class=""input"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    MatchCollection outputMatches = Regex.Matches(html, @"<div class=""output"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    // 입력과 출력 예제의 개수가 동일하고 1개 이상일 때만 성공으로 간주
                    if (inputMatches.Count > 0 && outputMatches.Count > 0 && inputMatches.Count == outputMatches.Count)
                    {
                        for (int i = 0; i < inputMatches.Count; i++)
                        {
                            inputs.Add(CleanHtmlText(inputMatches[i].Groups[1].Value));
                            outputs.Add(CleanHtmlText(outputMatches[i].Groups[1].Value));
                        }
                        return (inputs, outputs, ""); // 에러 없음!
                    }
                    else
                    {
                        return (null, null, "Codeforces 페이지에서 예제 데이터를 찾지 못했거나 입출력 쌍이 맞지 않습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, null, $"네트워크 통신 오류: {ex.Message}");
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
            // 🌟 1. 인터넷 연결 문제 또는 서버 다운 시
            catch (HttpRequestException httpEx)
            {
                System.Windows.Forms.MessageBox.Show($"🌐 Codeforces 서버에 연결할 수 없습니다. 인터넷 상태를 확인해주세요.\n\n(상세 원인: {httpEx.Message})", "네트워크 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            // 🌟 2. 응답은 받았지만 JSON 데이터 형태가 깨졌거나 변경되었을 시
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                System.Windows.Forms.MessageBox.Show($"🧩 데이터를 분석하는 중 문제가 발생했습니다. API 응답 구조가 변경되었을 수 있습니다.\n\n(상세 원인: {jsonEx.Message})", "데이터 처리 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            // 🌟 3. 그 외의 예상치 못한 시스템 오류
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"💻 알 수 없는 오류가 발생했습니다.\n\n(상세 원인: {ex.Message})", "시스템 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            return null;
        }

        // 단순 코드 실행 전용 메서드
        public async Task<string> RunCodeOnlyAsync(string code, string language)
        {
            try
            {
                if (!codinglearning.Models.AppConstants.Judge0LangIds.TryGetValue(language, out int langId))
                {
                    return "❌ 지원하지 않는 언어입니다.";
                }

                string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));

                // 입력값(stdin)이나 정답(expected_output) 없이 순수 코드만 전송
                var requestData = new { source_code = base64Code, language_id = langId };

                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(responseBody);

                if (result["status"] == null) return "❌ API 응답 오류";

                string GetDecodedString(JToken token)
                {
                    string str = token?.ToString();
                    if (string.IsNullOrEmpty(str)) return "";
                    try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                    catch { return str; }
                }

                string stdout = GetDecodedString(result["stdout"]);
                string stderr = GetDecodedString(result["stderr"]);
                string compileOutput = GetDecodedString(result["compile_output"]);

                if (!string.IsNullOrEmpty(compileOutput)) return $"[컴파일 에러]\r\n{compileOutput.Trim()}";
                if (!string.IsNullOrEmpty(stderr)) return $"[런타임 에러]\r\n{stderr.Trim()}";
                if (string.IsNullOrEmpty(stdout)) return "[출력 없음 (정상 종료)]";

                return stdout.Trim();
            }
            catch (Exception ex)
            {
                return $"실행 중 오류 발생: {ex.Message}";
            }
        }
    }
}