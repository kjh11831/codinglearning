using System;
using System.Diagnostics;
using System.IO;

namespace codinglearning.Managers
{
    public class GitHubManager
    {
        public (bool isSuccess, string message, bool isInfo) PushToGitHub()
        {
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string targetFolder = Path.Combine(docsPath, "CodingLearning_Submissions");

            if (!Directory.Exists(targetFolder))
            {
                return (false, "아직 저장된 코드가 없어요.\n먼저 문제를 풀어보세요!", true);
            }

            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
                processInfo.WorkingDirectory = targetFolder;

                string commitMessage = $"Auto commit: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")} 학습 기록";
                processInfo.Arguments = $"/c git add . && git commit -m \"{commitMessage}\" && git pull --rebase origin main && git push -u origin main";

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                processInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                processInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;

                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    // 에러 스트림과 일반 출력 스트림을 모두 읽어옵니다.
                    string error = process.StandardError.ReadToEnd();
                    string output = process.StandardOutput.ReadToEnd();
                    string fullMessage = (output + "\n" + error).Trim();

                    if (process.ExitCode == 0)
                    {
                        return (true, "🌱 깃허브에 성공적으로 코드가 업로드되었습니다!", false);
                    }
                    else
                    {
                        // 영어 및 한국어 Git 메시지 모두 대응
                        if (fullMessage.Contains("nothing to commit") ||
                            fullMessage.Contains("working tree clean") ||
                            fullMessage.Contains("커밋할 사항 없음") ||
                            fullMessage.Contains("변경 사항 없음") ||
                            fullMessage.Contains("작업 폴더 깨끗함"))
                        {
                            return (false, "새로 추가되거나 변경된 코드가 없습니다.", true);
                        }
                        else
                        {
                            // 이제 진짜 에러 원인이 팝업에 표시됩니다.
                            return (false, $"❌ 업로드 실패.\n(원인: {fullMessage})", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"명령어 실행 중 오류가 났어: {ex.Message}", false);
            }
        }
    }
}