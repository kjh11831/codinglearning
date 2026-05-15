// 파일 입출력, 시스템 환경 변수, 폼 컨트롤 등 필요한 네임스페이스 임포트
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace codinglearning.Managers
{
    // 로컬 파일 시스템(저장 및 내보내기)과 관련된 작업을 담당하는 매니저 클래스
    public class FileManager
    {
        // 작성한 코드를 사용자의 로컬 디스크에 자동 저장하는 메서드 (문제번호, 제목, 코드 내용, 언어, 정답 여부를 파라미터로 받음)
        public void SaveCodeToLocalFile(string problemId, string title, string code, string language, bool isCorrect)
        {
            try
            {
                // 사용자의 '내 문서(My Documents)' 폴더 경로를 시스템 환경 변수로부터 가져옴
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                // 내 문서 경로 아래에 'CodingLearning_Submissions'라는 최상위 백업 폴더 경로 생성
                string baseFolder = Path.Combine(docsPath, "CodingLearning_Submissions");
                // 정답 여부에 따라 하위 폴더 이름을 'Correct'(정답) 또는 'Wrong'(오답)으로 결정
                string subFolder = isCorrect ? "Correct" : "Wrong";
                // 최상위 폴더와 하위 폴더 경로를 병합하여 최종 저장 위치 결정
                string targetFolder = Path.Combine(baseFolder, subFolder);

                // 만약 해당 폴더(경로)가 로컬 디스크에 존재하지 않는다면
                if (!Directory.Exists(targetFolder))
                {
                    // 새로운 폴더를 생성
                    Directory.CreateDirectory(targetFolder);
                // 폴더 생성 분기 끝
                }

                // 기본 파일 확장자를 텍스트(.txt)로 설정
                string extension = ".txt";
                // 파라미터로 넘어온 언어 이름에 맞춰 적절한 소스코드 확장자로 변경
                if (language == "C#") extension = ".cs";
                else if (language == "C++") extension = ".cpp";
                else if (language == "Python") extension = ".py";
                else if (language == "Java") extension = ".java";

                // 문제 제목에 파일 이름으로 사용할 수 없는 특수 문자(\, /, :, *, ?, ", <, >, | 등)가 있다면 언더바(_)로 치환하여 안전한 문자열로 만듦
                string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));

                // 정답 여부에 따라 저장될 파일명 포맷 결정
                string fileName = isCorrect
                    ?
                    // 정답인 경우: [문제번호] 문제제목.확장자 (단일 덮어쓰기 용이)
                    $"[{problemId}] {safeTitle}{extension}"
                    // 오답인 경우: [문제번호] 문제제목_저장시간(년월일_시분초).확장자 (여러 번 시도한 오답 내역을 각각 분리 백업하기 위함)
                    : $"[{problemId}] {safeTitle}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}{extension}";

                // 저장할 폴더 경로와 만들어진 파일 이름을 합쳐 최종 파일 경로(FullPath) 생성
                string filePath = Path.Combine(targetFolder, fileName);
                // 지정된 경로에 코드(문자열)를 파일로 작성하여 저장
                File.WriteAllText(filePath, code);
            }
            // 폴더 권한 문제나 디스크 용량 부족 등 파일 입출력 중 에러가 발생한 경우
            catch (Exception ex)
            {
                // 프로그램이 종료되지 않도록 예외를 잡고 에러 메시지를 팝업으로 안내
                MessageBox.Show($"파일 자동 저장 실패: {ex.Message}");
            // 예외 처리 끝
            }
        }

        // 오답 목록(DataGrid)을 마크다운 표 형식 문자열로 변환하여 바탕화면에 파일로 추출(Export)하는 메서드
        public (bool isSuccess, string message) ExportWrongListToMarkdown(DataGridView dgvWrongList)
        {
            // 데이터그리드뷰에 행이 아예 없거나, 빈 새 행(입력 대기용) 딱 하나만 있는 경우 데이터가 없는 것으로 간주
            if (dgvWrongList.Rows.Count == 0 || (dgvWrongList.Rows.Count == 1 && dgvWrongList.Rows[0].IsNewRow))
            {
                // 추출 실패 플래그(false)와 안내 메시지를 튜플로 반환
                return (false, "추출할 오답 기록이 없습니다. 먼저 문제를 풀어보세요!");
            // 조건문 끝
            }

            try
            {
                // 많은 양의 문자열을 메모리 효율적으로 덧붙이고 조합하기 위해 StringBuilder 사용
                StringBuilder md = new StringBuilder();
                // 마크다운 최상위 제목(H1) 작성
                md.AppendLine("# 🚨 나의 코딩 테스트 오답 노트");
                // 노트 추출 일자(현재 시간)를 마크다운 인용구(>) 형식으로 작성
                md.AppendLine($"> **추출 일자:** {DateTime.Now.ToString("yyyy년 MM월 dd일 HH:mm")}");
                // 마크다운 문법을 위해 빈 줄 추가
                md.AppendLine();

                // 표의 헤더(열 이름)들을 담을 리스트 생성
                List<string> headers = new List<string>();
                // DataGridView의 컬럼들을 순회하며 컬럼 헤더 텍스트를 리스트에 추가
                foreach (DataGridViewColumn col in dgvWrongList.Columns) headers.Add(col.HeaderText);
                // 추출한 헤더 텍스트들을 ' | '로 연결하고 양 끝에 '|'를 붙여 마크다운 표 헤더 문법 생성
                md.AppendLine("| " + string.Join(" | ", headers) + " |");

                // 마크다운 표의 헤더와 본문을 나누는 구분선(---)을 담을 리스트 생성
                List<string> separators = new List<string>();
                // 열의 개수만큼 "---" 문자열을 리스트에 추가
                for (int i = 0; i < headers.Count; i++) separators.Add("---");
                // 구분선들을 결합하여 마크다운 표 구분 라인 작성
                md.AppendLine("| " + string.Join(" | ", separators) + " |");

                // 실제 데이터가 들어있는 표의 모든 행을 순회
                foreach (DataGridViewRow row in dgvWrongList.Rows)
                {
                    // 입력 대기중인 빈 깡통 행(새 행)은 무시하고 다음 행으로 넘어감
                    if (row.IsNewRow) continue;

                    // 현재 행의 각 셀(칸)의 문자열 값을 담을 리스트 생성
                    List<string> cells = new List<string>();
                    // 현재 행에 포함된 모든 셀을 순회
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        // 셀 값이 null이면 빈 문자열로, 아니면 문자열로 변환하여 리스트에 추가
                        cells.Add(cell.Value?.ToString() ?? "");
                    // 셀 순회 끝
                    }
                    // 추출한 한 줄의 셀 값들을 ' | '로 연결하여 마크다운 표의 한 행(Row)으로 작성
                    md.AppendLine("| " + string.Join(" | ", cells) + " |");
                // 표 행 순회 끝
                }

                // 추출한 마크다운 파일을 저장할 바탕화면(Desktop) 경로를 환경 변수에서 가져옴
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                // 파일명을 '오답노트_년월일.md' 형태로 생성
                string fileName = $"오답노트_{DateTime.Now.ToString("yyyyMMdd")}.md";
                // 바탕화면 경로와 파일명을 병합하여 최종 저장 경로 생성
                string filePath = Path.Combine(desktopPath, fileName);

                // 완성된 마크다운 문자열을 UTF-8 인코딩을 사용하여 지정된 경로의 파일로 저장
                File.WriteAllText(filePath, md.ToString(), Encoding.UTF8);

                // 저장이 완료되면 성공 플래그(true)와 파일 생성 완료 안내 메시지 반환
                return (true, $"바탕화면에 [{fileName}] 파일이 생성되었습니다!\n노션이나 블로그에 그대로 복붙해 보세요. 🚀");
            // 로직 끝
            }
            // 파일 생성 및 쓰기 도중 권한 오류나 디스크 에러 등이 발생한 경우
            catch (Exception ex)
            {
                // 실패 플래그(false)와 오류 메시지 반환
                return (false, $"파일 생성 중 오류가 발생했습니다: {ex.Message}");
            // 예외 처리 끝
            }
        }
    }
}