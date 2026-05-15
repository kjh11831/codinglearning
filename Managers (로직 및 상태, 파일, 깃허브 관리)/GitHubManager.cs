// 시스템 환경, 외부 프로세스(cmd) 실행, 파일 입출력을 위한 기본 네임스페이스 선언
using System;
using System.Diagnostics;
using System.IO;

namespace codinglearning.Managers
{
    // GitHub 자동 커밋 및 푸시 기능을 담당하는 매니저 클래스
    public class GitHubManager
    {
        // 깃허브 업로드를 실행하고 결과(성공 여부, 메시지, 정보성 알림 여부)를 튜플 형태로 반환하는 메서드
        public (bool isSuccess, string message, bool isInfo) PushToGitHub()
        {
            // 사용자의 '내 문서(My Documents)' 경로를 환경 변수에서 가져옴
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // 내 문서 경로에 코드가 저장되는 폴더명(CodingLearning_Submissions)을 합쳐 타겟 폴더 경로 생성
            string targetFolder = Path.Combine(docsPath, "CodingLearning_Submissions");

            // 만약 해당 타겟 폴더가 로컬에 아예 존재하지 않는다면
            if (!Directory.Exists(targetFolder))
            {
                // 아직 저장된 코드가 없다는 안내 메시지를 '정보성(isInfo = true)'으로 반환
                return (false, "아직 저장된 코드가 없어요.\n먼저 문제를 풀어보세요!", true);
            // 조건문 종료
            }

            // 외부 프로세스 실행 중 예외 처리를 위한 try 블록 시작
            try
            {
                // 명령 프롬프트(cmd.exe)를 실행하기 위한 프로세스 시작 정보 객체 생성
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
                // cmd가 실행될 때 기준이 되는 작업 디렉토리를 앞서 구한 타겟 폴더(저장소 위치)로 설정
                processInfo.WorkingDirectory = targetFolder;

                // 커밋 로그에 남길 메시지. 현재 시간(yyyy-MM-dd HH:mm 포맷)을 조합하여 자동 생성
                string commitMessage = $"Auto commit: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")} 학습 기록";
                // cmd.exe에 전달할 명령어 인자 세팅
                // '/c'는 명령어 실행 후 cmd 창을 닫으라는 의미이며, 그 뒤에 git add, commit, pull(rebase 병합), push를 순차적(&&)으로 실행
                processInfo.Arguments = $"/c git add . && git commit -m \"{commitMessage}\" && git pull --rebase origin main && git push -u origin main";
                // cmd 창이 사용자 화면에 보이지 않도록(백그라운드 실행) 설정
                processInfo.CreateNoWindow = true;
                // 운영체제 셸을 사용하지 않고 직접 실행해야 표준 입출력 리다이렉션이 가능함
                processInfo.UseShellExecute = false;
                // 프로세스의 일반 출력 결과(StandardOutput)를 프로그램 내부로 가져오기 위한 설정
                processInfo.RedirectStandardOutput = true;
                // 프로세스의 에러 출력 결과(StandardError)를 프로그램 내부로 가져오기 위한 설정
                processInfo.RedirectStandardError = true;

                // 한글 및 다국어 텍스트가 깨지지 않도록 출력 인코딩을 UTF-8로 설정
                processInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                // 에러 메시지 인코딩도 UTF-8로 설정
                processInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                // 설정한 프로세스 정보(processInfo)를 바탕으로 실제 cmd 프로세스를 실행하고 using문으로 리소스 자동 해제 준비
                using (Process process = Process.Start(processInfo))
                {
                    // git 명령어 실행이 모두 끝날 때까지 프로그램 흐름을 대기시킴
                    process.WaitForExit();
                    // 실행 완료 후, 프로세스의 에러 스트림 내용을 끝까지 읽어옴
                    string error = process.StandardError.ReadToEnd();
                    // 프로세스의 일반 출력 스트림 내용을 끝까지 읽어옴
                    string output = process.StandardOutput.ReadToEnd();
                    // 일반 출력과 에러 메시지를 줄바꿈으로 합치고, 앞뒤 공백을 제거하여 최종 문자열 생성
                    string fullMessage = (output + "\n" + error).Trim();
                    // 프로세스 종료 코드가 0이면(오류 없이 정상적으로 완료됨을 의미)
                    if (process.ExitCode == 0)
                    {
                        // 성공 메시지를 튜플로 반환
                        return (true, "🌱 깃허브에 성공적으로 코드가 업로드되었습니다!", false);
                    // 조건 종료
                    }
                    // 종료 코드가 0이 아닌 경우 (어떤 형태로든 문제가 있거나 실패한 경우)
                    else
                    {
                        // 영문판 또는 한글판 git 환경에 따라 "변경 사항이 없다"는 메시지가 출력되었는지 문자열 포함 여부로 확인
                        if (fullMessage.Contains("nothing to commit") ||
                            fullMessage.Contains("working tree clean") ||
                            // 한글판 git 메시지 대응 1
                            fullMessage.Contains("커밋할 사항 없음") ||
                            // 한글판 git 메시지 대응 2
                            fullMessage.Contains("변경 사항 없음") ||
                            // 한글판 git 메시지 대응 3
                            fullMessage.Contains("작업 폴더 깨끗함"))

                        // 위의 키워드 중 하나라도 포함되어 있다면 (에러가 아니라 단지 추가된 코드가 없는 상태)
                        {
                            // 실패가 아닌 정보성 알림 메시지로 반환
                            return (false, "새로 추가되거나 변경된 코드가 없습니다.", true);
                        // 내부 조건 종료
                        }
                        // 변경 사항이 없는 경우가 아니라 진짜 권한 오류, 충돌 등의 실제 오류가 발생한 경우
                        else
                        {
                            // 실제 실패 메시지와 함께 git에서 내뱉은 에러 원인 문자열(fullMessage)을 반환
                            return (false, $"❌ 업로드 실패.\n(원인: {fullMessage})", false);
                        // 진짜 실패 분기 종료
                        }
                    }
                }
            }
            // cmd 실행 권한 문제, 메모리 부족 등 프로세스 실행 단계에서 닷넷 런타임 예외가 발생한 경우
            catch (Exception ex)
            {
                // 예외 메시지를 담아 실패 결과 반환
                return (false, $"명령어 실행 중 오류가 났어: {ex.Message}", false);
            // 예외 처리 블록 종료
            }
        }
    }
}