// 기본 시스템 라이브러리 및 비동기 처리, HTTP 통신, 데이터 변환(LINQ) 네임스페이스 선언
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
// JSON 데이터 직렬화 및 JObject 파싱을 위한 Newtonsoft.Json 네임스페이스 선언
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace codinglearning.Services
{
    // Google의 Gemini AI를 활용하여 작성된 코드를 다른 프로그래밍 언어로 자동 번역해주는 서비스 클래스
    public class GeminiService
    {
        // Gemini API에 접근하기 위한 인증 키 (Trim()을 통해 불필요한 공백 제거)
        private readonly string apiKey = "AIzaSyBHLbgljhk8AJ8dbnMvwbdFyLfVpwFGujI".Trim();

        // 원본 코드, 원본 언어, 타겟(번역할) 언어, 그리고 진행 상태를 UI 텍스트 박스에 실시간으로 전달할 콜백 함수(onProgress)를 매개변수로 받는 비동기 메서드
        public async Task<(bool isSuccess, string resultText)> TranslateCodeAsync(string code, string sourceLang, string targetLang, Action<string> onProgress)
        {
            // 통신 시작 전, UI에 모델 탐색을 시작한다는 안내 메시지를 콜백으로 전달하여 출력
            onProgress?.Invoke("/* 🤖 구글 서버에서 이 API 키로 쓸 수 있는 AI 모델을 찾고 있습니다...\n   잠시만 기다려주세요! 🚀 */");

            // 네트워크 통신 및 파싱 중 발생할 수 있는 예외 처리를 위한 try 블록 시작
            try
            {
                // HTTP 통신을 위한 HttpClient 인스턴스를 생성하고, 작업 완료 후 리소스 자동 해제를 위해 using문 사용
                using (var client = new HttpClient())
                {
                    // 1단계: 제공된 API 키를 사용하여 현재 사용 가능한 Google Generative AI 모델 목록을 조회하는 API 주소
                    string getModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    // GET 방식으로 모델 목록을 서버에 요청
                    var getResponse = await client.GetAsync(getModelsUrl);

                    // HTTP 응답 코드가 200(성공) 대역이 아니라면 (키 오류, 한도 초과 등)
                    if (!getResponse.IsSuccessStatusCode)
                    {
                        // 실패 플래그(false)와 권한 오류 메시지 반환
                        return (false, "API 키 권한이 없거나 잘못되었습니다.");
                    }

                    // 성공적으로 받은 모델 목록 JSON 본문을 문자열 형태로 읽어옴
                    string getResponseBody = await getResponse.Content.ReadAsStringAsync();
                    // 문자열을 JSON 노드 트리(JObject) 형태로 파싱
                    var modelsData = JObject.Parse(getResponseBody);
                    // 코드를 번역할 때 최종적으로 사용할 타겟 모델의 이름을 저장할 변수
                    string targetModel = "";

                    // 파싱된 JSON 데이터의 "models" 배열을 하나씩 순회
                    foreach (var model in modelsData["models"])
                    {
                        // 각 모델의 이름("name" 필드) 가져옴
                        string modelName = model["name"]?.ToString();
                        // 해당 모델이 지원하는 생성 방식 목록("supportedGenerationMethods" 필드)을 배열(JArray) 형태로 캐스팅해 가져옴
                        var methods = model["supportedGenerationMethods"] as JArray;

                        // 모델 이름이 정상적으로 존재하고, 그 이름에 "gemini"가 포함되어 있으며, 지원 기능 목록이 비어있지 않다면
                        if (modelName != null && modelName.Contains("gemini") && methods != null)
                        {
                            // 지원하는 기능 목록을 다시 반복문으로 순회하여 검사
                            foreach (var method in methods)
                            {
                                // 만약 해당 모델이 'generateContent'(텍스트 생성 기능)를 지원한다면
                                if (method.ToString() == "generateContent")
                                {
                                    // 텍스트 생성이 가능한 이 gemini 모델을 우리의 타겟 모델로 지정
                                    targetModel = modelName;
                                    // 조건에 맞는 모델을 하나 찾았으므로 내부 반복문 탈출
                                    break;
                                }
                            }
                        }
                        // 타겟 모델을 성공적으로 찾았다면, 불필요한 전체 외부 반복문 탐색도 탈출하여 시간 단축
                        if (!string.IsNullOrEmpty(targetModel)) break;
                    }

                    // 전체 모델을 다 뒤졌는데도 적합한 모델을 찾지 못했다면
                    if (string.IsNullOrEmpty(targetModel))
                    {
                        // 실패 플래그와 오류 안내 메시지 반환
                        return (false, "이 API 키로 사용할 수 있는 Gemini 모델이 없습니다.");
                    }

                    // 성공적으로 찾은 모델 이름을 UI 콜백으로 전달하여 사용자에게 번역 시작 안내
                    onProgress?.Invoke($"/* 🤖 자동 탐색 성공! [{targetModel}] 모델로 코드를 번역합니다...\n   잠시만 기다려주세요! ✨ */");

                    // 2단계: 실제로 코드를 번역하기 위해, 위에서 알아낸 타겟 모델의 텍스트 생성(generateContent) 엔드포인트 URL 구성
                    string url = $"https://generativelanguage.googleapis.com/v1beta/{targetModel}:generateContent?key={apiKey}";

                    // Gemini에게 번역을 지시할 프롬프트(명령어) 텍스트 작성
                    // 경쟁 프로그래밍(코테) 전문가 역할 부여, 동일 로직 유지, 자료형 유지(64비트 등), 불필요한 설명이나 마크다운 백틱(```) 없이 순수하게 코드만 출력하라는 엄격한 규칙(Strict Rules) 포함
                    string prompt = $@"You are an expert competitive programming translator. Translate the following {sourceLang} code to {targetLang}. 
Strict Rules:
1. EXACT MATCH: Maintain the exact same logic and algorithm. DO NOT optimize or fix bugs.
2. DATA TYPES: Preserve 64-bit integers (e.g. long) strictly. Do not downgrade to int.
3. FORMAT: Output ONLY the raw translated code. Do NOT wrap the code in markdown blocks like ```csharp or ```java. Do NOT add any explanations or text, just the code itself.

Code to translate:
{code}";

                    // Gemini API 규격에 맞게 JSON 요청 본문(Payload)의 골격을 익명 객체로 생성
                    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                    // 생성한 익명 객체를 실제 전송 가능한 JSON 형식의 문자열로 직렬화(Serialize)
                    string jsonContent = JsonConvert.SerializeObject(requestBody);
                    // HTTP POST 요청의 Body에 담기 위해 StringContent 타입으로 변환하고 UTF-8 인코딩 처리
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // 구성된 URL과 페이로드(본문)를 사용하여 POST 방식으로 구글 서버에 번역을 요청하고 응답 기다림
                    var response = await client.PostAsync(url, content);

                    // 번역 요청의 응답이 성공 코드가 아니라면 (할당량 제한, 내부 에러 등)
                    if (!response.IsSuccessStatusCode)
                    {
                        // 실패한 상세 원인 문자열 읽어옴
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        // 상태 코드 및 구글 서버의 원시 에러 메시지를 합쳐 실패 원인으로 반환
                        return (false, $"번역 통신 실패 (상태 코드: {(int)response.StatusCode})\n\n[구글 서버 응답]\n{errorDetail}");
                    }

                    // 번역 성공 시, 구글 서버가 반환한 응답 본문 전체를 문자열로 읽어옴
                    string responseString = await response.Content.ReadAsStringAsync();
                    // 결과 문자열을 JObject 트리 구조로 파싱
                    var responseObject = JObject.Parse(responseString);

                    // 파싱된 JSON 트리 깊숙한 곳에 위치한 실제 번역 결과 'text' 토큰을 안전하게 추출 (? 연산자를 통해 NullReference 예외 방지)
                    var textToken = responseObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];

                    // 텍스트 토큰이 비어있지 않고 정상적으로 존재한다면
                    if (textToken != null)
                    {
                        // 추출한 토큰을 일반 문자열로 변환하고 좌우의 쓸데없는 공백이나 줄바꿈 제거(Trim)
                        string translatedCode = textToken.ToString().Trim();

                        // 프롬프트 지시에도 불구하고 Gemini가 응답에 마크다운 코드 블록 표시(예: ```python)를 붙여서 보냈을 경우를 대비한 예외 처리
                        if (translatedCode.StartsWith("```"))
                        {
                            // 코드를 엔터(\n) 기준으로 여러 줄의 문자열 배열로 쪼갬
                            var lines = translatedCode.Split('\n');
                            // 첫 번째 줄(```언어명)과 마지막 줄(```)을 건너뛰고, 가운데 있는 실제 소스 코드 부분만 다시 엔터(\n)로 이어 붙임
                            translatedCode = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
                        }

                        // 가공된 최종 번역 코드를 우측 공백 한 번 더 정리하여 성공 플래그(true)와 함께 반환
                        return (true, translatedCode.TrimEnd());
                    }
                    // 텍스트 토큰을 찾지 못했다면 (예: 정책 위반이나 안전 필터링 등으로 텍스트 생성이 차단되었을 때)
                    else
                    {
                        // 생성 중단 이유를 담고 있는 finishReason 속성 추출. 못 찾으면 "Unknown" 할당
                        string finishReason = responseObject["candidates"]?[0]?["finishReason"]?.ToString() ?? "Unknown";

                        // 중단 사유 및 서버 응답 원본 전체를 보여주며 실패 처리로 반환
                        return (false, $"번역 결과가 비어있습니다. (사유: {finishReason})\n서버 응답: {responseString}");
                    }
                }
            }
            // 코드 실행 중 인터넷 단절이나 파싱 오류 등 기타 시스템 관련 예외가 발생했을 때
            catch (Exception ex)
            {
                // 앱 강제 종료를 막고 예외의 핵심 메시지를 묶어 반환
                return (false, $"인터넷 연결이나 시스템 오류가 발생했습니다:\n{ex.Message}");
            }
        }
    }
}