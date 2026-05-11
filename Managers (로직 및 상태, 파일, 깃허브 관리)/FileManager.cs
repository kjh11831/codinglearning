using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace codinglearning.Managers
{
    public class FileManager
    {
        // 파일 자동 저장 로직
        public void SaveCodeToLocalFile(string problemId, string title, string code, string language, bool isCorrect)
        {
            try
            {
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string baseFolder = Path.Combine(docsPath, "CodingLearning_Submissions");
                string subFolder = isCorrect ? "Correct" : "Wrong";
                string targetFolder = Path.Combine(baseFolder, subFolder);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                string extension = ".txt";
                if (language == "C#") extension = ".cs";
                else if (language == "C++") extension = ".cpp";
                else if (language == "Python") extension = ".py";
                else if (language == "Java") extension = ".java";

                string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
                string fileName = isCorrect
                    ? $"[{problemId}] {safeTitle}{extension}"
                    : $"[{problemId}] {safeTitle}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}{extension}";

                string filePath = Path.Combine(targetFolder, fileName);
                File.WriteAllText(filePath, code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 자동 저장 실패: {ex.Message}");
            }
        }

        // 오답 노트 마크다운 추출 로직
        public (bool isSuccess, string message) ExportWrongListToMarkdown(DataGridView dgvWrongList)
        {
            if (dgvWrongList.Rows.Count == 0 || (dgvWrongList.Rows.Count == 1 && dgvWrongList.Rows[0].IsNewRow))
            {
                return (false, "추출할 오답 기록이 없습니다. 먼저 문제를 풀어보세요!");
            }

            try
            {
                StringBuilder md = new StringBuilder();
                md.AppendLine("# 🚨 나의 코딩 테스트 오답 노트");
                md.AppendLine($"> **추출 일자:** {DateTime.Now.ToString("yyyy년 MM월 dd일 HH:mm")}");
                md.AppendLine();

                List<string> headers = new List<string>();
                foreach (DataGridViewColumn col in dgvWrongList.Columns) headers.Add(col.HeaderText);

                md.AppendLine("| " + string.Join(" | ", headers) + " |");

                List<string> separators = new List<string>();
                for (int i = 0; i < headers.Count; i++) separators.Add("---");
                md.AppendLine("| " + string.Join(" | ", separators) + " |");

                foreach (DataGridViewRow row in dgvWrongList.Rows)
                {
                    if (row.IsNewRow) continue;
                    List<string> cells = new List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cells.Add(cell.Value?.ToString() ?? "");
                    }
                    md.AppendLine("| " + string.Join(" | ", cells) + " |");
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"오답노트_{DateTime.Now.ToString("yyyyMMdd")}.md";
                string filePath = Path.Combine(desktopPath, fileName);

                File.WriteAllText(filePath, md.ToString(), Encoding.UTF8);
                return (true, $"바탕화면에 [{fileName}] 파일이 생성되었습니다!\n노션이나 블로그에 그대로 복붙해 보세요. 🚀");
            }
            catch (Exception ex)
            {
                return (false, $"파일 생성 중 오류가 발생했습니다: {ex.Message}");
            }
        }
    }
}