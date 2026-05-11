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
                processInfo.Arguments = $"/c git add . && git commit -m \"{commitMessage}\" && git push";

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode == 0)
                    {
                        return (true, "🌱 깃허브에 성공적으로 코드가 업로드되었습니다!", false);
                    }
                    else
                    {
                        if (error.Contains("nothing to commit") || error.Contains("working tree clean"))
                            return (false, "새로 추가되거나 변경된 코드가 없습니다.", true);
                        else
                            return (false, $"❌ 업로드 실패.\n(원인: {error})", false);
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