using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace codinglearning.Services
{
    public class GeminiService
    {
        private readonly string apiKey = "AIzaSyBHLbgljhk8AJ8dbnMvwbdFyLfVpwFGujI".Trim();

        // UI에 상태를 전달하기 위해 Action<string> onProgress를 매개변수로 받음
        public async Task<(bool isSuccess, string resultText)> TranslateCodeAsync(string code, string sourceLang, string targetLang, Action<string> onProgress)
        {
            onProgress?.Invoke("/* 🤖 구글 서버에서 이 API 키로 쓸 수 있는 AI 모델을 찾고 있습니다...\n   잠시만 기다려주세요! 🚀 */");

            try
            {
                using (var client = new HttpClient())
                {
                    string getModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    var getResponse = await client.GetAsync(getModelsUrl);

                    if (!getResponse.IsSuccessStatusCode)
                    {
                        return (false, "API 키 권한이 없거나 잘못되었습니다.");
                    }

                    string getResponseBody = await getResponse.Content.ReadAsStringAsync();
                    var modelsData = JObject.Parse(getResponseBody);
                    string targetModel = "";

                    foreach (var model in modelsData["models"])
                    {
                        string modelName = model["name"]?.ToString();
                        var methods = model["supportedGenerationMethods"] as JArray;

                        if (modelName != null && modelName.Contains("gemini") && methods != null)
                        {
                            foreach (var method in methods)
                            {
                                if (method.ToString() == "generateContent")
                                {
                                    targetModel = modelName;
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(targetModel)) break;
                    }

                    if (string.IsNullOrEmpty(targetModel))
                    {
                        return (false, "이 API 키로 사용할 수 있는 Gemini 모델이 없습니다.");
                    }

                    onProgress?.Invoke($"/* 🤖 자동 탐색 성공! [{targetModel}] 모델로 코드를 번역합니다...\n   잠시만 기다려주세요! ✨ */");

                    string url = $"https://generativelanguage.googleapis.com/v1beta/{targetModel}:generateContent?key={apiKey}";
                    string prompt = $@"You are an expert competitive programming translator. Translate the following {sourceLang} code to {targetLang}. 
Strict Rules:
1. EXACT MATCH: Maintain the exact same logic and algorithm. DO NOT optimize or fix bugs.
2. DATA TYPES: Preserve 64-bit integers (e.g. long) strictly. Do not downgrade to int.
3. FORMAT: Output ONLY the raw translated code. Do NOT wrap the code in markdown blocks like ```csharp or ```java. Do NOT add any explanations or text, just the code itself.

Code to translate:
{code}";

                    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                    string jsonContent = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        return (false, $"번역 통신 실패 (상태 코드: {(int)response.StatusCode})\n\n[구글 서버 응답]\n{errorDetail}");
                    }

                    string responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseString);

                    // 기존 오류 원인: responseObject["candidates"][0]... 처럼 직접 접근하면 중간에 키가 없을 때 프로그램이 뻗어버림.
                    // 해결: '?.' (Null 조건부 연산자)를 사용하여 안전하게 텍스트 추출.
                    // 만약 candidates 배열이 비어있거나, parts 키가 없다면 에러를 내뿜는 대신 textToken에 null을 반환함.
                    var textToken = responseObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];

                    if (textToken != null) // 안전하게 텍스트를 찾았을 때만 실행
                    {
                        string translatedCode = textToken.ToString().Trim();

                        // 마크다운 코드 블록 제거 로직
                        if (translatedCode.StartsWith("```"))
                        {
                            var lines = translatedCode.Split('\n');
                            translatedCode = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
                        }

                        return (true, translatedCode.TrimEnd());
                    }
                    else // API가 예상과 다른 구조를 반환했을 때 (예: 안전 필터링 차단, 빈 응답 등)
                    {
                        // 에러 원인을 추적하기 위해 finishReason(종료 사유)를 추출해봄.
                        string finishReason = responseObject["candidates"]?[0]?["finishReason"]?.ToString() ?? "Unknown";

                        // 프로그램이 뻗는 대신 false를 반환하고, Form1.cs에서 에러 메시지를 띄워주도록 처리.
                        return (false, $"번역 결과가 비어있습니다. (사유: {finishReason})\n서버 응답: {responseString}");
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"인터넷 연결이나 시스템 오류가 발생했습니다:\n{ex.Message}");
            }
        }
    }
}