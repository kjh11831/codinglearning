// Newtonsoft.Json 등 JSON 파싱 및 HTTP 통신, 정규식 처리를 위한 시스템 네임스페이스 선언
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
    // 외부 API(Judge0, Codeforces)와의 통신을 전담하는 서비스 클래스
    public class ApiService
    {
        // 애플리케이션 전체에서 재사용할 수 있도록 정적(static) HttpClient 인스턴스를 생성하여 초기화함
        private static readonly HttpClient httpClient = CreateConfiguredHttpClient();

        // HTTP 통신 시 압축 해제 및 기본 헤더가 설정된 HttpClient 인스턴스를 생성하는 메서드
        private static HttpClient CreateConfiguredHttpClient()
        {
            // GZip 및 Deflate 압축을 자동으로 해제하도록 핸들러 설정
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            // 설정한 핸들러를 바탕으로 HttpClient 객체 생성
            var client = new HttpClient(handler);

            // 서버에서 봇으로 차단하지 않도록 브라우저와 유사한 User-Agent 및 Accept 헤더 기본으로 추가
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            // 연결 유지(keep-alive) 및 보안 요청 업그레이드 헤더 추가
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            // 설정이 완료된 HttpClient 객체 반환
            return client;
        }

        // Judge0 API를 사용하여 작성된 코드를 컴파일/실행하고, 예상 출력값과 비교해 정답 여부를 판단하는 메서드
        public async Task<(bool isCorrect, string message)> RunJudge0Async(string code, string language, string stdin, string expectedOutput)
        {
            // 선택한 언어가 앱에 정의된 Judge0 지원 언어 목록에 있는지 확인하고, 해당 언어의 ID를 가져옴
            if (!codinglearning.Models.AppConstants.Judge0LangIds.TryGetValue(language, out int langId))
            {
                // 지원하지 않는 언어일 경우 실패 처리 및 에러 메시지 반환
                return (false, $"❌ 지원하지 않는 언어입니다: {language}");
            }

            // API 전송을 위해 소스코드, 입력값, 예상 출력값을 Base64 문자열로 인코딩
            string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            string base64Stdin = Convert.ToBase64String(Encoding.UTF8.GetBytes(stdin));
            string base64Expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedOutput));

            // Judge0 API 규격에 맞게 요청할 JSON 데이터 객체 생성
            var requestData = new
            {
                source_code = base64Code,
                language_id = langId,
                stdin = base64Stdin,
                expected_output = base64Expected
            };

            // 데이터 객체를 JSON 형태의 문자열로 직렬화하고, HTTP POST 요청 본문(StringContent)으로 만듦
            string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Judge0 제출 API URL (Base64 인코딩 사용, 실행 완료까지 동기적으로 대기 설정)
            string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";

            // 설정된 URL로 POST 요청을 보내 코드 제출 및 실행
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // API로부터 돌아온 응답 본문을 문자열로 읽고 JObject 형식으로 파싱
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);

            // 파싱된 JSON에 'status' 필드가 없다면 비정상적인 응답으로 간주
            if (result["status"] == null)
            {
                // API 응답 오류 메시지 반환
                return (false, "❌ API 응답 오류");
            }

            // 상태 코드(id)가 3(Accepted, 정답)인지 확인하여 성공 여부를 불리언 값으로 저장
            bool isCorrect = result["status"]["id"]?.ToString() == "3";
            // API에서 반환한 상태 설명 문자열(예: Wrong Answer, Time Limit Exceeded)을 가져옴 없을 시 기본값 지정
            string statusDesc = result["status"]["description"]?.ToString() ?? "상태 알 수 없음";

            // 반환된 Base64 인코딩 데이터를 안전하게 디코딩하여 원본 문자열로 변환하는 지역(로컬) 함수
            string GetDecodedString(JToken token)
            {
                string str = token?.ToString();
                // 데이터가 비어있으면 빈 문자열 반환
                if (string.IsNullOrEmpty(str)) return "";
                // 정상적인 Base64 문자열이면 UTF-8로 변환하고, 파싱 오류 시 원본 문자열 그대로 반환
                try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                catch { return str; }
            }

            // API 응답 객체에서 실제 실행된 표준 출력(stdout) 결과값을 디코딩하여 가져옴
            string actualOutput = GetDecodedString(result["stdout"]);
            // 컴파일 에러(compile_output)와 표준 에러(stderr) 값을 디코딩한 뒤 합쳐서 에러 출력용 문자열로 만듦
            string errorOutput = GetDecodedString(result["compile_output"]) + "\n" + GetDecodedString(result["stderr"]);

            // 최종적으로 화면에 보여줄 메시지 변수 선언
            string msg;
            // 제출 결과가 정답인 경우
            if (isCorrect)
            {
                // 성공 메시지와 함께 디코딩된 출력값을 양옆 공백 제거(Trim)하여 지정
                msg = $"✅ 성공 (Accepted)\n출력값: {actualOutput.Trim()}";
            }
            // 제출 결과가 오답이거나 에러가 발생한 경우
            else
            {
                // 실패 사유(상태 설명), 기대했던 출력값, 실제 출력값, 에러 메시지를 합쳐서 사용자에게 원인을 상세히 보여주도록 포맷팅
                msg = $"❌ 실패 ({statusDesc})\n[기대값]:\n{expectedOutput.Trim()}\n\n[실제 출력]:\n{actualOutput.Trim()}\n\n{errorOutput.Trim()}";
            }

            // 최종 정답 여부(bool)와 포맷팅된 메시지를 튜플로 반환
            return (isCorrect, msg);
        }

        // 문제 번호를 기반으로 Codeforces 미러 사이트를 크롤링하여 예제 입력/출력 데이터를 리스트로 가져오는 메서드
        public async Task<(List<string> inputs, List<string> outputs, string errorMessage)> FetchSampleDataFromWebAsync(string problemId)
        {
            // 크롤링한 예제 입력 데이터들을 저장할 리스트 초기화
            List<string> inputs = new List<string>();
            // 크롤링한 예제 출력 데이터들을 저장할 리스트 초기화
            List<string> outputs = new List<string>();

            // HTTP 통신 및 파싱 중 발생할 수 있는 에러 처리를 위한 try-catch 블록
            try
            {
                // 문제 번호 중 숫자 부분(Contest ID)만 추출
                string contestId = new String(problemId.Where(Char.IsDigit).ToArray());
                // 문제 번호 중 알파벳 부분(Problem Index, A, B 등)만 추출
                string index = new String(problemId.Where(Char.IsLetter).ToArray());
                // 안정적인 접근을 위해 Codeforces의 Mirror(우회) 사이트 문제 페이지 URL 조립
                string url = $"https://mirror.codeforces.com/problemset/problem/{contestId}/{index}";

                // 조립된 URL에 GET 방식으로 HTTP 요청 객체 생성
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    // 봇 차단을 피하기 위해 일반 브라우저 형태의 User-Agent 헤더 추가
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    // 서버로 GET 요청을 전송하고 응답 받음
                    var response = await httpClient.SendAsync(request);

                    // 응답 상태 코드가 200번대(성공)가 아니라면
                    if (!response.IsSuccessStatusCode)
                    {
                        // 실패 메시지와 상태 코드를 반환하며 처리 중단
                        return (null, null, $"서버 접근 거부 (상태 코드: {(int)response.StatusCode})");
                    }

                    // 성공적으로 받은 HTML 본문을 문자열 형태로 읽어옴
                    string html = await response.Content.ReadAsStringAsync();
                    // 정규표현식을 사용하여 HTML 내에서 'input' 클래스를 가진 div 안의 <pre> 태그 내용 모두 매칭
                    MatchCollection inputMatches = Regex.Matches(html, @"<div class=""input"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    // 정규표현식을 사용하여 HTML 내에서 'output' 클래스를 가진 div 안의 <pre> 태그 내용 모두 매칭
                    MatchCollection outputMatches = Regex.Matches(html, @"<div class=""output"">.*?<pre>(.*?)</pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    // 입력 예제와 출력 예제가 둘 다 존재하고, 그 개수가 짝이 맞을 경우
                    if (inputMatches.Count > 0 && outputMatches.Count > 0 && inputMatches.Count == outputMatches.Count)
                    {
                        // 추출된 예제 쌍의 개수만큼 반복문 수행
                        for (int i = 0; i < inputMatches.Count; i++)
                        {
                            // 정규식 첫 번째 그룹에 잡힌 텍스트(예제 데이터)의 HTML 태그들을 정리하는 헬퍼 함수를 호출한 뒤 리스트에 추가
                            inputs.Add(CleanHtmlText(inputMatches[i].Groups[1].Value));
                            // 출력 예제 텍스트도 동일하게 정리 후 리스트에 추가
                            outputs.Add(CleanHtmlText(outputMatches[i].Groups[1].Value));
                        }
                        // 추출 및 정리가 완료된 입력/출력 예제 리스트 쌍과 함께 에러 없음을 나타내는 빈 문자열 반환
                        return (inputs, outputs, "");
                    }
                    // 예제 데이터를 찾지 못했거나 개수가 맞지 않는 경우
                    else
                    {
                        // 실패 원인 메시지 반환
                        return (null, null, "Codeforces 페이지에서 예제 데이터를 찾지 못했거나 입출력 쌍이 맞지 않습니다.");
                    }
                }
            }
            // 네트워크 문제, 정규식 예외 등이 발생했을 때의 처리
            catch (Exception ex)
            {
                // 예외 세부 정보가 담긴 오류 메시지 반환
                return (null, null, $"네트워크 통신 오류: {ex.Message}");
            }
        }

        // HTML 문자열에 포함된 줄바꿈용 태그나 엔티티(Entity) 문자를 실제 텍스트로 깔끔하게 정리하는 보조 메서드
        private string CleanHtmlText(string html)
        {
            // HTML 안의 모든 <div> 태그 시작/종료 부분을 일반 줄바꿈 기호(\n)로 치환
            string text = Regex.Replace(html, @"<div[^>]*>", "\n", RegexOptions.IgnoreCase);
            // HTML의 <br>, <br/> 등의 줄바꿈 태그도 줄바꿈 기호(\n)로 치환
            text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

            // 남은 HTML 태그들(<...>)을 모조리 찾아 삭제(빈 문자열로 치환)
            text = Regex.Replace(text, @"<.*?>", "");

            // &lt; , &gt; 같은 HTML 특수 엔티티 문자를 원래 기호(<, >)로 디코딩
            text = System.Net.WebUtility.HtmlDecode(text);

            // 문자열 좌우측 공백을 자르고(Trim), 각 줄의 앞뒤 쓸데없는 공백이나 여러 번 반복된 빈 줄을 정리하여 최종 텍스트 반환
            return Regex.Replace(text, @"^\s+$[\r\n]*", "", RegexOptions.Multiline).Trim();
        }

        // Codeforces API를 호출하여 전체 문제 목록 데이터(Problemset)를 JArray 배열 형식으로 받아오는 내부 접근(internal) 비동기 메서드
        internal async Task<JArray> FetchCodeforcesProblemsAsync()
        {
            // 외부 통신 시 예외를 잡기 위한 try-catch 블록 시작
            try
            {
                // Codeforces 문제 목록 데이터를 제공하는 공식 API URL
                string url = "https://codeforces.com/api/problemset.problems";
                // HTTP GET 방식으로 해당 API 호출
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // API 응답 코드가 200(성공)이 아니면 예외(Exception)를 강제로 발생시켜 아래 로직을 멈춤
                response.EnsureSuccessStatusCode();

                // 응답받은 JSON 데이터를 문자열 형태로 변환
                string responseBody = await response.Content.ReadAsStringAsync();
                // 문자열을 JObject 트리 형태로 파싱하여 내부 데이터에 접근할 수 있게 만듦
                JObject data = JObject.Parse(responseBody);

                // 반환된 JSON에서 상태값(status)이 "OK"라면 정상적으로 데이터를 가져온 것으로 판단
                if (data["status"]?.ToString() == "OK")
                {
                    // 최상위 객체의 "result" 안의 "problems" 배열만 추출하여 JArray로 캐스팅해 반환
                    return (JArray)data["result"]["problems"];
                }
            }
            // 네트워크 단절, DNS 해석 오류 등 HTTP 요청 과정 중 문제가 발생했을 때 잡히는 예외 처리
            catch (HttpRequestException httpEx)
            {
                // 사용자에게 인터넷 상태 및 서버 연결 문제를 알리는 윈도우 팝업(MessageBox) 표시
                System.Windows.Forms.MessageBox.Show($"🌐 Codeforces 서버에 연결할 수 없습니다. 인터넷 상태를 확인해주세요.\n\n(상세 원인: {httpEx.Message})", "네트워크 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            // 응답은 정상적으로 왔으나 JSON 형태가 망가졌거나 규격이 달라 파싱이 실패했을 때의 예외 처리
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                // 사용자에게 API 응답 구조나 파싱 관련 문제가 발생했음을 알리는 에러 팝업 표시
                System.Windows.Forms.MessageBox.Show($"🧩 데이터를 분석하는 중 문제가 발생했습니다. API 응답 구조가 변경되었을 수 있습니다.\n\n(상세 원인: {jsonEx.Message})", "데이터 처리 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            // 위에서 처리되지 않은 그 외의 모든 일반적인 에러를 잡아내는 예외 처리 블록
            catch (Exception ex)
            {
                // 예상치 못한 범용 에러가 발생했다는 메시지를 담은 시스템 오류 팝업 표시
                System.Windows.Forms.MessageBox.Show($"💻 알 수 없는 오류가 발생했습니다.\n\n(상세 원인: {ex.Message})", "시스템 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }

            // 예외가 발생했거나, status가 OK가 아닌 경우에는 정상 데이터 반환이 불가능하므로 null 반환
            return null;
        }

        // 문제 번호나 정답 예상 출력값 없이, 사용자가 작성한 코드를 단순히 실행만 시켜볼 때 호출되는 메서드 (예제 테스트용)
        public async Task<string> RunCodeOnlyAsync(string code, string language)
        {
            // 통신 예외 방지용 try-catch 블록 시작
            try
            {
                // 실행할 언어가 Judge0 설정에 등록되어 있는지 검사하고 ID를 가져옴
                if (!codinglearning.Models.AppConstants.Judge0LangIds.TryGetValue(language, out int langId))
                {
                    // 등록되지 않은 언어라면 에러 문자열을 곧바로 반환
                    return "❌ 지원하지 않는 언어입니다.";
                }

                // 전송할 코드를 깨짐을 방지하기 위해 UTF-8 형태의 바이트로 바꾼 뒤 Base64 텍스트로 인코딩
                string base64Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
                // 정답/예제입력 데이터 없이 오직 소스코드와 언어 ID만 담은 요청 객체 만듦
                var requestData = new { source_code = base64Code, language_id = langId };

                // 요청 객체를 JSON 규격에 맞게 직렬화함
                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                // 직렬화된 JSON 데이터를 HTTP Body에 담을 수 있게 StringContent 객체로 변환
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                // Judge0 동기적 코드 제출 API 주소
                string url = "https://ce.judge0.com/submissions/?base64_encoded=true&wait=true";
                // HTTP 통신을 시작하여 POST 방식으로 실행 요청을 날리고 결과 기다림
                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                // 실행이 완료되어 돌아온 JSON 응답 본문 추출
                string responseBody = await response.Content.ReadAsStringAsync();
                // JObject로 파싱하여 내부 노드에 접근
                JObject result = JObject.Parse(responseBody);

                // 상태값이 빈 응답이면 API 오류로 간주하고 중단
                if (result["status"] == null) return "❌ API 응답 오류";
                // Base64로 인코딩된 출력/에러 문자열들을 원본으로 디코딩하는 로컬 함수
                string GetDecodedString(JToken token)
                {
                    // JSON 토큰을 문자열 형태로 변환
                    string str = token?.ToString();
                    // 빈 값이면 그대로 빈 값 반환
                    if (string.IsNullOrEmpty(str)) return "";
                    // Base64 디코딩 시도 후 성공 시 원본 UTF-8 문자열을, 실패 시 그대로 반환
                    try { return Encoding.UTF8.GetString(Convert.FromBase64String(str)); }
                    catch { return str; }
                }

                // API 응답에서 표준 출력(stdout) 값 디코딩
                string stdout = GetDecodedString(result["stdout"]);
                // API 응답에서 실행 중 발생한 런타임 에러(stderr) 값 디코딩
                string stderr = GetDecodedString(result["stderr"]);
                // API 응답에서 컴파일 과정 중 발생한 에러(compile_output) 값 디코딩
                string compileOutput = GetDecodedString(result["compile_output"]);

                // 컴파일 에러 메시지가 비어있지 않다면 최우선적으로 컴파일 실패 문구 반환
                if (!string.IsNullOrEmpty(compileOutput)) return $"[컴파일 에러]\r\n{compileOutput.Trim()}";
                // 컴파일은 성공했으나 실행 중 에러가 났다면 런타임 에러 문구 반환
                if (!string.IsNullOrEmpty(stderr)) return $"[런타임 에러]\r\n{stderr.Trim()}";
                // 에러도 없고 표준 출력(stdout)도 비어있다면 에러 없이 프로그램이 곧바로 종료된 것으로 판단
                if (string.IsNullOrEmpty(stdout)) return "[출력 없음 (정상 종료)]";

                // 앞선 에러가 모두 없다면 코드가 정상적으로 동작해 뱉어낸 출력값을 공백 제거 후 최종 반환
                return stdout.Trim();
            }
            // 코드가 동작하는 과정이나 통신/파싱 중 발생한 예상 못한 문제들 처리
            catch (Exception ex)
            {
                // 예외 세부 내용을 담아 에러 문자열 반환
                return $"실행 중 오류 발생: {ex.Message}";
            }
        }
    }
}