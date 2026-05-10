using codinglearning.Managers;
using codinglearning.Models;
using codinglearning.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace codinglearning
{
    public partial class Form1 : Form
    {
        // 1. 객체 생성
        private LearningSessionManager sessionManager = new LearningSessionManager();
        private FirebaseManager firebaseManager = new FirebaseManager();
        private ApiService apiService = new ApiService();

        // 2. 현재 선택된 문제 정보
        private string selId = "", selTitle = "", selDiff = "", selTags = "";

        // ⭐ 추가할 스위치 변수들 (중복 클릭 방지용)
        private bool isSearching = false;
        private bool isRunningSample = false;

        // ⭐ 1. 이전 언어를 기억하는 변수 (콤보박스 복구용, 여기에만 딱 한 번 있어야 함!)
        private string previousLang = "C#";

        // ⭐ [추가] 재번역(Retry)을 위해 원본 코드와 언어를 기억하는 변수
        private string lastSourceCode = "";
        private string lastSourceLang = "";
        private string lastTargetLang = "";

        public Form1()
        {
            InitializeComponent();
        }

        #region [ 1. 공통 및 하단 상태 표시줄 ]
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!firebaseManager.Initialize())
            {
                MessageBox.Show("Firebase 연결 실패. 인터넷 상태를 확인하세요.");
                return;
            }

            cbLanguage.Items.AddRange(new string[] { "C#", "C++", "Python", "Java" });
            cbLanguage.SelectedIndex = 0;

            if (chartAccuracy != null && chartAccuracy.Series.Count > 0)
            {
                chartAccuracy.Series[0].Name = "풀이 통계";
            }

            ApplyMinimalStyle();
            StartLearningSession();

            lblDarkMode.Parent = this;
            lblDarkMode.Location = new Point(tabControl1.Width - lblDarkMode.Width - 15, 3);
            lblDarkMode.BringToFront();
            lblDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDarkMode.BackColor = Color.Transparent;

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            // 프로그램 시작 시 라이트 모드 테마 적용
            ApplyTheme();

            // ⭐ [추가] 폼이 꺼질 때 방금 만든 안전장치(Form1_FormClosing)가 작동하도록 연결!
            this.FormClosing += Form1_FormClosing;
        }

        private void learningTimer_Tick(object sender, EventArgs e)
        {
            if (sessionManager.CurrentStatus == "Active")
            {
                lblTimer.Text = sessionManager.GetCurrentDuration().ToString(@"hh\:mm\:ss");
                sessionManager.UpdateIdleState();
            }
            UpdateStatusUI();
        }

        private void UpdateStatusUI()
        {
            Color activeColor = isDarkMode ? Color.LimeGreen : Color.Green;
            Color idleColor = isDarkMode ? Color.Gold : Color.Orange;
            Color breakColor = isDarkMode ? Color.LightCoral : Color.Crimson;

            if (sessionManager.CurrentStatus == "Active")
            {
                lblStatus.Text = "▶ 학습 중";
                lblStatus.ForeColor = activeColor;
                lblTimer.ForeColor = activeColor;
                btnStopSession.Text = "⏹ 세션 종료";
            }
            else if (sessionManager.CurrentStatus == "Idle")
            {
                lblStatus.Text = "Ⅱ 잠깐 쉬는 중";
                lblStatus.ForeColor = idleColor;
                lblTimer.ForeColor = idleColor;
                btnStopSession.Text = "⏹ 세션 종료";
            }
            else
            {
                lblStatus.Text = "■ 휴식 중";
                lblStatus.ForeColor = breakColor;
                lblTimer.ForeColor = breakColor;
                btnStopSession.Text = "▶ 세션 시작";
            }
        }

        private void StartLearningSession()
        {
            sessionManager.StartSession();
            learningTimer.Start();
            UpdateStatusUI();
        }

        private async void btnStopSession_Click(object sender, EventArgs e)
        {
            if (sessionManager.CurrentStatus == "Break")
            {
                StartLearningSession();
                return;
            }

            learningTimer.Stop();
            sessionManager.StopSession();
            UpdateStatusUI();

            int duration = sessionManager.GetTotalSeconds();
            var sessionData = new SessionData
            {
                status = "Break",
                sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sessionDuration = duration,
                lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await firebaseManager.SaveSessionAsync(sessionData);
            await firebaseManager.PushSessionLogAsync(sessionData);

            duration = sessionManager.GetTotalSeconds();
            TimeSpan ts = TimeSpan.FromSeconds(duration);
            string formattedTime = "";

            if (ts.Hours > 0) formattedTime += $"{ts.Hours}시간 ";
            if (ts.Minutes > 0) formattedTime += $"{ts.Minutes}분 ";
            formattedTime += $"{ts.Seconds}초";

            MessageBox.Show($"학습 세션이 종료되었습니다!\n🔥 이번 세션 학습 시간: {formattedTime}", "세션 종료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region [ 2. 탭 1: 문제 탐색 ]
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await PerformSearch(txtKeyword.Text, txtMinDifficulty.Text, txtMaxDifficulty.Text);
        }

        private async void btnSearchAll_Click(object sender, EventArgs e)
        {
            await PerformSearch("", "", "");
        }

        private void btnResetSearch_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            txtKeyword.Text = "";
            txtMinDifficulty.Text = "";
            txtMaxDifficulty.Text = "";
            dgvProblems.Rows.Clear();
        }

        private async Task PerformSearch(string keyword, string minDiffStr, string maxDiffStr)
        {
            if (isSearching) return;
            isSearching = true;
            sessionManager.RecordUserAction();

            btnSearch.Text = "검색 중...";
            btnSearch.ForeColor = isDarkMode ? Color.White : SystemColors.ControlText;
            btnSearch.Refresh();

            try
            {
                JArray problems = await apiService.FetchCodeforcesProblemsAsync();
                if (problems != null)
                {
                    dgvProblems.Rows.Clear();
                    dgvProblems.ColumnCount = 5;
                    dgvProblems.Columns[0].Name = "번호";
                    dgvProblems.Columns[1].Name = "제목";
                    dgvProblems.Columns[2].Name = "난이도";
                    dgvProblems.Columns[3].Name = "태그";
                    dgvProblems.Columns[4].Name = "결과";

                    keyword = keyword.ToLower();
                    int minDiff = string.IsNullOrEmpty(minDiffStr) ? 0 : int.Parse(minDiffStr);
                    int maxDiff = string.IsNullOrEmpty(maxDiffStr) ? 3500 : int.Parse(maxDiffStr);

                    int count = 0;
                    foreach (var p in problems)
                    {
                        if (count >= 50) break;

                        string pTitle = p["name"]?.ToString();
                        string pTags = string.Join(", ", p["tags"]);
                        int pRating = p["rating"] != null ? (int)p["rating"] : 0;

                        if (pRating >= minDiff && (pRating <= maxDiff || maxDiff == 0))
                        {
                            if (string.IsNullOrEmpty(keyword) || pTitle.ToLower().Contains(keyword) || pTags.ToLower().Contains(keyword))
                            {
                                dgvProblems.Rows.Add($"{p["contestId"]}{p["index"]}", pTitle, pRating, pTags, "-");
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류: " + ex.Message);
            }
            finally
            {
                isSearching = false;
                btnSearch.Text = "검색";
            }
        }

        private void dgvProblems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            sessionManager.RecordUserAction();
            if (e.RowIndex >= 0)
            {
                var row = dgvProblems.Rows[e.RowIndex];
                selId = row.Cells[0].Value?.ToString();
                selTitle = row.Cells[1].Value?.ToString();
                selDiff = row.Cells[2].Value?.ToString();
                selTags = row.Cells[3].Value?.ToString();

                lblSelProbNum.Text = lblCodeProbNum.Text = selId;
                lblSelProbTitle.Text = lblCodeProbTitle.Text = selTitle;
                lblSelProbDiff.Text = lblCodeProbDiff.Text = selDiff;
                lblSelProbTags.Text = lblCodeProbTags.Text = selTags;
            }
        }

        private void btnViewProblem_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (!string.IsNullOrEmpty(selId))
            {
                string contestId = new String(selId.Where(Char.IsDigit).ToArray());
                string index = new String(selId.Where(Char.IsLetter).ToArray());
                System.Diagnostics.Process.Start($"https://codeforces.com/problemset/problem/{contestId}/{index}");
            }
            else
            {
                MessageBox.Show("목록에서 문제를 먼저 선택해주세요.");
            }
        }
        #endregion

        #region [ 3. 탭 2: 코드 작성 및 채점 ]
        private void txtCode_TextChanged(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
        }

        private void btnResetCode_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            txtCode.Text = "";
            txtResult.Text = "";
        }

        private async void btnRunSample_Click(object sender, EventArgs e)
        {
            if (isRunningSample) return;
            sessionManager.RecordUserAction();

            if (string.IsNullOrEmpty(selId))
            {
                MessageBox.Show("먼저 문제를 선택해주세요!");
                return;
            }

            isRunningSample = true;
            btnRunSample.Text = "실행 중...";
            btnRunSample.ForeColor = isDarkMode ? Color.White : SystemColors.ControlText;
            btnRunSample.Refresh();

            txtResult.Text = "Judge0 서버에 채점 요청 중...";

            try
            {
                string currentLang = cbLanguage.SelectedItem.ToString();
                var (isCorrect, message) = await apiService.RunJudge0Async(txtCode.Text, currentLang);
                txtResult.Text = message;

                SaveCodeToLocalFile(selId, selTitle, txtCode.Text, currentLang, isCorrect);

                var record = new SubmissionRecord
                {
                    code = txtCode.Text,
                    status = isCorrect ? "correct" : "wrong",
                    language = currentLang,
                    date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    title = selTitle
                };

                // ⭐ [핵심] C#, C++ 특수기호가 URL을 망가뜨리지 않게 안전한 글자로 변환!
                string safeLang = currentLang.Replace("#", "Sharp").Replace("+", "p");
                string uniqueKey = $"{selId}_{safeLang}";

                await firebaseManager.SaveSubmissionAsync(uniqueKey, record, selDiff, selTags);
            }
            catch (Exception ex)
            {
                txtResult.Text = $"채점 서버 통신 오류: {ex.Message}";
            }
            finally
            {
                isRunningSample = false;
                btnRunSample.Text = "예제 테스트 실행";
            }
        }

        private void btnSubmitCF_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (!string.IsNullOrEmpty(selId))
            {
                string contestId = new String(selId.Where(Char.IsDigit).ToArray());
                System.Diagnostics.Process.Start($"https://codeforces.com/contest/{contestId}/submit");
            }
            else
            {
                MessageBox.Show("문제를 먼저 선택해주세요.");
            }
        }
        #endregion

        #region [ 4. 탭 3 & 4: 오답 목록 및 통계 데이터 갱신 ]
        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (tabControl1.SelectedIndex == 2) await LoadWrongListUI();
            else if (tabControl1.SelectedIndex == 3) await LoadStatisticsUI();
            else if (tabControl1.SelectedIndex == 4) await LoadTimeStatisticsUI();
        }

        // ⭐ 표(풀이 내역)를 불러오고 필터링을 적용하는 마법의 메서드
        private async Task LoadWrongListUI()
        {
            var dict = await firebaseManager.GetWrongListAsync();
            dgvWrongList.Rows.Clear();
            if (dict == null) return;

            dgvWrongList.ColumnCount = 8; // ⭐ 7에서 8로 증가!
            dgvWrongList.Columns[0].HeaderText = "번호";
            dgvWrongList.Columns[1].HeaderText = "제목";
            dgvWrongList.Columns[2].HeaderText = "언어"; // ⭐ 언어 열 추가
            dgvWrongList.Columns[3].HeaderText = "난이도";
            dgvWrongList.Columns[4].HeaderText = "태그";
            dgvWrongList.Columns[5].HeaderText = "결과";
            dgvWrongList.Columns[6].HeaderText = "발생일(풀이일)";
            dgvWrongList.Columns[7].HeaderText = "복습 예정일";

            foreach (var item in dict)
            {
                // ⭐ "2225A_CSharp" 같은 키에서 번호와 언어를 분리하고 기호 복구!
                string rawKey = item.Key;
                string pNum = rawKey.Split('_')[0];
                string safeLang = rawKey.Contains("_") ? rawKey.Split('_')[1] : "알 수 없음";
                string lang = safeLang.Replace("Sharp", "#").Replace("p", "+"); // 화면엔 다시 C#으로!

                string title = item.Value["title"]?.ToString() ?? "-";
                string diff = item.Value["diff"]?.ToString() ?? "-";
                string tags = item.Value["tags"]?.ToString() ?? "-";
                bool isSolved = item.Value["solvedAfter"] != null && (bool)item.Value["solvedAfter"];
                string date = item.Value["addedDate"]?.ToString() ?? "-";
                string reviewDateStr = item.Value["reviewDate"]?.ToString() ?? "-";

                if (rbCorrect != null && rbCorrect.Checked && !isSolved) continue;
                if (rbWrong != null && rbWrong.Checked && isSolved) continue;

                // ⭐ 셀 추가할 때 lang(언어) 변수 삽입
                int rowIndex = dgvWrongList.Rows.Add(pNum, title, lang, diff, tags, isSolved ? "✅ 해결됨" : "❌ 미해결", date, reviewDateStr);

                if (!isSolved && DateTime.TryParse(reviewDateStr, out DateTime reviewDate))
                {
                    if (reviewDate < DateTime.Now)
                    {
                        dgvWrongList.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.IndianRed;
                        dgvWrongList.Rows[rowIndex].DefaultCellStyle.Font = new Font(dgvWrongList.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private async void dgvWrongList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            sessionManager.RecordUserAction();
            if (e.RowIndex >= 0)
            {
                string pNum = dgvWrongList.Rows[e.RowIndex].Cells[0].Value?.ToString();
                string lang = dgvWrongList.Rows[e.RowIndex].Cells[2].Value?.ToString();

                var dict = await firebaseManager.GetWrongListAsync();

                // ⭐ DB에서 찾을 땐 다시 안전한 글자로 변환해서 검색 ("C#" -> "CSharp")
                string safeLang = lang.Replace("#", "Sharp").Replace("+", "p");
                string searchKey = $"{pNum}_{safeLang}";
                if (dict != null && !dict.ContainsKey(searchKey)) searchKey = pNum;

                if (!string.IsNullOrWhiteSpace(searchKey) && dict != null && dict.ContainsKey(searchKey))
                {
                    JObject data = JObject.FromObject(dict[searchKey]);

                    lblWrongProbNum.Text = pNum; // 화면엔 번호만 깔끔하게 표시
                    lblWrongProbTitle.Text = data["title"] != null ? data["title"].ToString() : "-";
                    lblWrongProbDiff.Text = data["diff"] != null ? data["diff"].ToString() : "-";
                    lblWrongProbTags.Text = data["tags"] != null ? data["tags"].ToString() : "-";

                    bool isSolved = data["solvedAfter"] != null && (bool)data["solvedAfter"];
                    lblWrongProbResult.Text = isSolved ? "해결됨" : "미해결";
                }
                else
                {
                    ClearWrongDetailLabels();
                }
            }
        }

        private void ClearWrongDetailLabels()
        {
            lblWrongProbNum.Text = "-";
            lblWrongProbTitle.Text = "-";
            lblWrongProbDiff.Text = "-";
            lblWrongProbTags.Text = "-";
            lblWrongProbResult.Text = "-";
        }

        private void btnViewWrongProblem_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            string wrongId = lblWrongProbNum.Text;
            if (wrongId != "나야 번호" && !string.IsNullOrEmpty(wrongId))
            {
                string contestId = new String(wrongId.Where(Char.IsDigit).ToArray());
                string index = new String(wrongId.Where(Char.IsLetter).ToArray());
                System.Diagnostics.Process.Start($"https://codeforces.com/problemset/problem/{contestId}/{index}");
            }
            else
            {
                MessageBox.Show("오답 목록에서 문제를 선택해주세요.");
            }
        }

        private void btnSolveAgain_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (lblWrongProbNum.Text == "나야 번호" || string.IsNullOrEmpty(lblWrongProbNum.Text))
            {
                MessageBox.Show("다시 풀 오답을 선택해주세요.");
                return;
            }

            selId = lblCodeProbNum.Text = lblWrongProbNum.Text;
            selTitle = lblCodeProbTitle.Text = lblWrongProbTitle.Text;
            selDiff = lblCodeProbDiff.Text = lblWrongProbDiff.Text;
            selTags = lblCodeProbTags.Text = lblWrongProbTags.Text;

            tabControl1.SelectedIndex = 1;
        }

        private async Task LoadStatisticsUI()
        {
            var dict = await firebaseManager.GetAllSubmissionsAsync();
            if (dict == null) return;

            int totalProblems = 0, correctCount = 0;

            if (dgvRecentRecords != null)
            {
                dgvRecentRecords.Rows.Clear();
                dgvRecentRecords.ColumnCount = 5; // ⭐ 4에서 5로 증가
                dgvRecentRecords.Columns[0].Name = "번호";
                dgvRecentRecords.Columns[1].Name = "제목";
                dgvRecentRecords.Columns[2].Name = "언어"; // ⭐ 언어 열 추가
                dgvRecentRecords.Columns[3].Name = "결과";
                dgvRecentRecords.Columns[4].Name = "날짜";
            }

            foreach (var problem in dict)
            {
                totalProblems++;
                bool hasCorrect = false;

                // ⭐ 번호와 언어 분리 및 기호 복구
                string rawKey = problem.Key;
                string pNum = rawKey.Split('_')[0];
                string safeLang = rawKey.Contains("_") ? rawKey.Split('_')[1] : "알 수 없음";
                string lang = safeLang.Replace("Sharp", "#").Replace("p", "+");

                foreach (var attempt in problem.Value["attempts"])
                {
                    string status = attempt.Value["status"];
                    string date = attempt.Value["date"];
                    string title = attempt.Value["title"] != null ? attempt.Value["title"].ToString() : "-";

                    if (status == "correct") hasCorrect = true;

                    if (dgvRecentRecords != null)
                    {
                        // ⭐ lang 변수 삽입
                        dgvRecentRecords.Rows.Add(pNum, title, lang, status == "correct" ? "✅ 정답" : "❌ 오답", date);
                    }
                }
                if (hasCorrect) correctCount++;
            }

            int wrongCount = totalProblems - correctCount;
            double accuracy = totalProblems > 0 ? ((double)correctCount / totalProblems) * 100 : 0;

            lblTotalSolved.Text = totalProblems.ToString();
            lblCorrect.Text = correctCount.ToString();
            lblWrong.Text = wrongCount.ToString();
            lblAccuracy.Text = $"{Math.Round(accuracy, 1)}%";

            if (chartAccuracy != null && chartAccuracy.Series.Count > 0)
            {
                chartAccuracy.Series[0].Points.Clear();
                chartAccuracy.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

                chartAccuracy.Series[0].Points.AddXY("정답", correctCount);
                chartAccuracy.Series[0].Points.AddXY("오답", wrongCount);

                Color correctColor = isDarkMode ? Color.FromArgb(90, 130, 90) : Color.FromArgb(143, 188, 143);
                Color wrongColor = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.FromArgb(224, 159, 150);

                chartAccuracy.Series[0].Points[0].Color = correctColor;
                chartAccuracy.Series[0].Points[1].Color = wrongColor;

                chartAccuracy.Series[0].IsVisibleInLegend = false;

                chartAccuracy.Legends[0].CustomItems.Clear();
                chartAccuracy.Legends[0].CustomItems.Add(correctColor, "정답");
                chartAccuracy.Legends[0].CustomItems.Add(wrongColor, "오답");
            }
        }
        #endregion

        private void lblGitHubPush_Click(object sender, EventArgs e)
        {
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string targetFolder = Path.Combine(docsPath, "CodingLearning_Submissions");

            if (!Directory.Exists(targetFolder))
            {
                MessageBox.Show("아직 저장된 코드가 없어요.\n먼저 문제를 풀어보세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
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
                        MessageBox.Show("🌱 깃허브에 성공적으로 코드가 업로드되었습니다!", "잔디 심기 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (error.Contains("nothing to commit") || error.Contains("working tree clean"))
                        {
                            MessageBox.Show("새로 추가되거나 변경된 코드가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"❌ 업로드 실패.\n(원인: {error})", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"명령어 실행 중 오류가 났어: {ex.Message}", "시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportWrongList_Click(object sender, EventArgs e)
        {
            if (dgvWrongList.Rows.Count == 0 || (dgvWrongList.Rows.Count == 1 && dgvWrongList.Rows[0].IsNewRow))
            {
                MessageBox.Show("추출할 오답 기록이 없습니다. 먼저 문제를 풀어보세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                StringBuilder md = new StringBuilder();
                md.AppendLine("# 🚨 나의 코딩 테스트 오답 노트");
                md.AppendLine($"> **추출 일자:** {DateTime.Now.ToString("yyyy년 MM월 dd일 HH:mm")}");
                md.AppendLine();

                List<string> headers = new List<string>();
                foreach (DataGridViewColumn col in dgvWrongList.Columns)
                {
                    headers.Add(col.HeaderText);
                }
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

                MessageBox.Show($"바탕화면에 [{fileName}] 파일이 생성되었습니다!\n노션이나 블로그에 그대로 복붙해 보세요. 🚀", "추출 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 생성 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadTimeStatisticsUI()
        {
            var logs = await firebaseManager.GetAllSessionLogsAsync();

            if (logs == null || logs.Count == 0)
            {
                dgvTimeRecords.Rows.Clear();
                return;
            }

            DateTime now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");
            DateTime weekAgo = now.AddDays(-7);

            int todayTotalSeconds = 0;
            int weeklyTotalSeconds = 0;
            int maxFocusSeconds = 0;

            Dictionary<string, int> dailyStudyMap = new Dictionary<string, int>();
            for (int i = 6; i >= 0; i--)
            {
                dailyStudyMap[now.AddDays(-i).ToString("yyyy-MM-dd")] = 0;
            }

            dgvTimeRecords.Rows.Clear();
            dgvTimeRecords.ColumnCount = 4;
            dgvTimeRecords.Columns[0].Name = "날짜";
            dgvTimeRecords.Columns[1].Name = "학습 구간";
            dgvTimeRecords.Columns[2].Name = "순공 시간";
            dgvTimeRecords.Columns[3].Name = "성과";

            foreach (var item in logs.OrderByDescending(x => x.Value.sessionEnd))
            {
                var log = item.Value;
                if (!DateTime.TryParse(log.sessionEnd, out DateTime endTime)) continue;

                DateTime startTime = endTime.AddSeconds(-log.sessionDuration);
                string logDate = endTime.ToString("yyyy-MM-dd");

                if (logDate == todayStr) todayTotalSeconds += log.sessionDuration;
                if (endTime >= weekAgo) weeklyTotalSeconds += log.sessionDuration;
                if (log.sessionDuration > maxFocusSeconds) maxFocusSeconds = log.sessionDuration;

                if (dailyStudyMap.ContainsKey(logDate))
                {
                    dailyStudyMap[logDate] += log.sessionDuration;
                }

                TimeSpan duration = TimeSpan.FromSeconds(log.sessionDuration);
                string interval = $"{startTime:HH:mm} ~ {endTime:HH:mm}";
                string netTime = duration.Hours > 0 ? $"{duration.Hours}시간 {duration.Minutes}분" : $"{duration.Minutes}분 {duration.Seconds}초";

                dgvTimeRecords.Rows.Add(logDate, interval, netTime, "학습 완료");
            }

            if (lblTodayTotal != null)
            {
                TimeSpan tToday = TimeSpan.FromSeconds(todayTotalSeconds);
                lblTodayTotal.Text = $"{tToday.Hours}h {tToday.Minutes}m {tToday.Seconds}s";
            }

            if (lblWeeklyAvg != null)
            {
                int avgSeconds = weeklyTotalSeconds / 7;
                TimeSpan tAvg = TimeSpan.FromSeconds(avgSeconds);
                lblWeeklyAvg.Text = $"{tAvg.Hours}h {tAvg.Minutes}m {tAvg.Seconds}s";
            }

            if (lblMaxFocus != null)
            {
                TimeSpan tMax = TimeSpan.FromSeconds(maxFocusSeconds);
                lblMaxFocus.Text = $"{tMax.Hours}h {tMax.Minutes}m {tMax.Seconds}s";
            }

            if (chartTimeHistory != null)
            {
                chartTimeHistory.Series.Clear();
                var series = chartTimeHistory.Series.Add("학습 시간(분)");

                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
                series.Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                series.IsValueShownAsLabel = true;

                foreach (var kvp in dailyStudyMap)
                {
                    string shortDate = kvp.Key.Substring(5);
                    double minutes = Math.Round(kvp.Value / 60.0, 1);
                    series.Points.AddXY(shortDate, minutes);
                }
            }
        }

        private void ApplyMinimalStyle()
        {
            DataGridView[] grids = { dgvTimeRecords, dgvWrongList, dgvProblems, dgvRecentRecords };

            foreach (var grid in grids)
            {
                if (grid == null) continue;

                grid.BackgroundColor = Color.White;
                grid.BorderStyle = BorderStyle.None;
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                grid.GridColor = Color.FromArgb(240, 240, 240);
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 245, 245);
                grid.DefaultCellStyle.SelectionForeColor = Color.Black;
                grid.RowHeadersVisible = false;
            }

            if (chartTimeHistory != null)
            {
                chartTimeHistory.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                chartTimeHistory.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                chartTimeHistory.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartTimeHistory.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            }

            if (chartAccuracy != null)
            {
                chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                chartAccuracy.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                chartAccuracy.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartAccuracy.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            }
        }

        private void SaveCodeToLocalFile(string problemId, string title, string code, string language, bool isCorrect)
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
                string fileName;

                if (isCorrect)
                {
                    fileName = $"[{problemId}] {safeTitle}{extension}";
                }
                else
                {
                    string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    fileName = $"[{problemId}] {safeTitle}_{timeStamp}{extension}";
                }

                string filePath = Path.Combine(targetFolder, fileName);
                File.WriteAllText(filePath, code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 자동 저장 실패: {ex.Message}");
            }
        }

        private void lblDarkMode_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            lblDarkMode.Text = isDarkMode ? "🌞 라이트 모드" : "🌙 다크 모드";
            ApplyTheme();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color formBgColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;

            Rectangle bgRect = e.Bounds;
            bgRect.Inflate(2, 2);
            e.Graphics.FillRectangle(new SolidBrush(formBgColor), bgRect);

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color tabColor = isDarkMode
                ? (isSelected ? Color.FromArgb(60, 60, 65) : formBgColor)
                : (isSelected ? Color.White : SystemColors.Control);

            Color textColor = isDarkMode
                ? (isSelected ? Color.White : Color.Gray)
                : SystemColors.ControlText;

            Rectangle tabRect = e.Bounds;
            tabRect.Inflate(-1, -1);
            e.Graphics.FillRectangle(new SolidBrush(tabColor), tabRect);

            string tabText = tabControl1.TabPages[e.Index].Text;
            TextRenderer.DrawText(e.Graphics, tabText, e.Font, tabRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private Panel tabCoverPanel;
        private bool isDarkMode = false;

        private void ApplyTheme()
        {
            Color bgColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Color textColor = isDarkMode ? Color.FromArgb(212, 212, 212) : SystemColors.ControlText;
            Color boxColor = isDarkMode ? Color.FromArgb(37, 37, 38) : Color.White;
            Color gridLineColor = isDarkMode ? Color.FromArgb(63, 63, 70) : Color.LightGray;
            Color accentColor = isDarkMode ? Color.FromArgb(86, 156, 214) : Color.SteelBlue;

            this.BackColor = bgColor;
            this.ForeColor = textColor;

            tabControl1.Appearance = TabAppearance.Normal;
            tabControl1.BackColor = bgColor;

            if (tabCoverPanel == null)
            {
                tabCoverPanel = new Panel();
                this.Controls.Add(tabCoverPanel);
            }

            Rectangle lastTab = tabControl1.GetTabRect(tabControl1.TabCount - 1);

            tabCoverPanel.BackColor = bgColor;
            tabCoverPanel.Location = new Point(tabControl1.Left + lastTab.Right, tabControl1.Top);
            tabCoverPanel.Size = new Size(tabControl1.Width - lastTab.Right, lastTab.Bottom);
            tabCoverPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tabCoverPanel.BringToFront();
            lblDarkMode.BringToFront();

            ChangeControlColors(this, bgColor, textColor, boxColor, gridLineColor, accentColor);
        }

        private void ChangeControlColors(Control parent, Color bg, Color text, Color box, Color gridLine, Color accent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Label || c is CheckBox || c is RadioButton)
                {
                    c.BackColor = Color.Transparent;
                    c.ForeColor = text;
                }
                else
                {
                    c.BackColor = bg;
                    c.ForeColor = text;
                }

                if (c.Name == "lblStudyTime")
                {
                    c.Font = new Font(c.Font.FontFamily, 24, FontStyle.Bold);
                    c.ForeColor = accent;
                    continue;
                }

                if (c is TextBox || c is ComboBox)
                {
                    if (isDarkMode)
                    {
                        c.BackColor = Color.FromArgb(60, 60, 65);
                        c.ForeColor = Color.White;
                    }
                    else
                    {
                        c.BackColor = box;
                        c.ForeColor = text;
                    }

                    if (c is ComboBox cb)
                    {
                        cb.FlatStyle = FlatStyle.Flat;
                    }
                    if (c is TextBox tb)
                    {
                        tb.BorderStyle = isDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    }
                }

                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = box;
                    dgv.DefaultCellStyle.BackColor = box;
                    dgv.DefaultCellStyle.ForeColor = text;
                    dgv.GridColor = gridLine;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(50, 50, 50) : SystemColors.Control;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = text;
                }

                else if (c is Chart chart)
                {
                    chart.BackColor = bg;
                    if (chart.Titles.Count > 0) chart.Titles[0].ForeColor = text;
                    if (chart.Legends.Count > 0)
                    {
                        chart.Legends[0].BackColor = Color.Transparent;
                        chart.Legends[0].ForeColor = text;
                    }

                    foreach (var area in chart.ChartAreas)
                    {
                        area.BackColor = box;
                        area.AxisX.LabelStyle.ForeColor = text;
                        area.AxisY.LabelStyle.ForeColor = text;
                        area.AxisX.MajorGrid.LineColor = gridLine;
                        area.AxisY.MajorGrid.LineColor = gridLine;
                        area.AxisX.LineColor = gridLine;
                        area.AxisY.LineColor = gridLine;
                    }

                    if (chart.Series.Count > 0)
                    {
                        chart.Series[0].BackGradientStyle = GradientStyle.None;

                        if (chart.Name == "chartTimeHistory")
                        {
                            chart.Series[0].Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                        }
                        else if (chart.Name == "chartAccuracy")
                        {
                            Color correctColor = isDarkMode ? Color.FromArgb(90, 130, 90) : Color.FromArgb(143, 188, 143);
                            Color wrongColor = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.FromArgb(224, 159, 150);

                            if (chart.Series[0].Points.Count >= 2)
                            {
                                chart.Series[0].Points[0].Color = correctColor;
                                chart.Series[0].Points[1].Color = wrongColor;
                            }

                            if (chart.Legends[0].CustomItems.Count >= 2)
                            {
                                chart.Legends[0].CustomItems[0].Color = correctColor;
                                chart.Legends[0].CustomItems[1].Color = wrongColor;
                            }
                        }
                        else
                        {
                            chart.Series[0].Color = isDarkMode ? Color.FromArgb(60, 110, 160) : Color.SteelBlue;

                            if (chart.Series.Count > 1)
                            {
                                chart.Series[1].BackGradientStyle = GradientStyle.None;
                                chart.Series[1].Color = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.IndianRed;
                            }
                        }
                    }
                }

                else if (c is GroupBox || c is Panel)
                {
                    c.BackColor = bg;
                    c.ForeColor = text;
                }

                else if (c is Button btn && c.Name != "btnGitHubPush")
                {
                    if (isDarkMode)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 90);
                        btn.BackColor = Color.FromArgb(60, 60, 65);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 100, 130);
                        btn.FlatAppearance.MouseDownBackColor = accent;
                    }
                    else
                    {
                        btn.FlatStyle = FlatStyle.Standard;
                        btn.BackColor = SystemColors.Control;
                        btn.ForeColor = SystemColors.ControlText;
                    }
                }

                else if (c is StatusStrip statusStrip)
                {
                    statusStrip.BackColor = isDarkMode ? Color.FromArgb(20, 20, 20) : SystemColors.Control;
                }

                if (c.HasChildren)
                {
                    ChangeControlColors(c, bg, text, box, gridLine, accent);
                }
            }
        }

        // ⭐ 각 언어별 기본 뼈대 코드를 관리하는 도우미 메서드
        private string GetDefaultTemplate(string lang)
        {
            switch (lang)
            {
                case "C#": return "using System;\n\nnamespace CodingTest\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            // 여기에 코드를 작성하세요\n            \n        }\n    }\n}";
                case "C++": return "#include <iostream>\nusing namespace std;\n\nint main() {\n    // 여기에 코드를 작성하세요\n    \n    return 0;\n}";
                case "Java": return "import java.util.*;\n\npublic class Main {\n    public static void main(String[] args) {\n        // 여기에 코드를 작성하세요\n        \n    }\n}";
                case "Python": return "def main():\n    # 여기에 코드를 작성하세요\n    pass\n\nif __name__ == '__main__':\n    main()";
                default: return "";
            }
        }

        // ⭐ 다시 번역 버튼 클릭 이벤트 (반드시 async void 로 적어야 합니다!)
        private async void btnRetryTranslate_Click(object sender, EventArgs e)
        {
            // 1. 방어 코드: 아직 번역을 한 번도 안 했으면 (기억해둔 원본이 없으면) 튕겨냄
            if (string.IsNullOrEmpty(lastSourceCode) || string.IsNullOrEmpty(lastTargetLang))
            {
                MessageBox.Show("아직 번역된 코드가 없습니다.\n먼저 코드를 작성하고 언어를 변경해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. 진짜 다시 돌릴 건지 물어보기
            DialogResult result = MessageBox.Show(
                "번역 결과가 이상한가요?\nAI에게 똑같은 원본 코드를 다른 방식(확률)으로 다시 번역하라고 지시합니다.\n\n(※ 기존 번역 결과는 지워집니다.)",
                "다시 번역 (Retry)",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 3. 콤보박스 바꿀 때 저장해뒀던 '원본 코드'와 '언어' 정보를 그대로 다시 던져서 재번역 실행!
                await TranslateCodeAsync(lastSourceCode, lastSourceLang, lastTargetLang);
            }
        }

        // ⭐ 콤보박스 변경 이벤트 메서드 (작성 코드 감지 완벽 강화판!)
        private async void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLang = cbLanguage.SelectedItem?.ToString();
            if (selectedLang == previousLang) return;

            bool isUserCode = false;
            string currentCode = txtCode.Text;

            if (!string.IsNullOrWhiteSpace(currentCode))
            {
                string normCurrent = currentCode.Replace("\r\n", "\n").Trim();
                string normTemplate = GetDefaultTemplate(previousLang).Replace("\r\n", "\n").Trim();

                if (normCurrent != normTemplate)
                {
                    isUserCode = true;
                }
            }

            if (isUserCode)
            {
                DialogResult result = MessageBox.Show(
                    $"작성 중인 {previousLang} 코드가 있습니다.\n선택하신 {selectedLang}(으)로 🤖AI 자동 번역하시겠습니까?\n\n(예: AI 번역 유지 / 아니요: 초기화 / 취소: 변경 취소)",
                    "AI 코드 자동 번역",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    RevertComboBox();
                    return;
                }
                else if (result == DialogResult.Yes)
                {
                    // ⭐ 번역을 시작하기 전에, 재번역을 대비해 원본 상태를 저장해둠!
                    lastSourceCode = currentCode;
                    lastSourceLang = previousLang;
                    lastTargetLang = selectedLang;

                    // 번역 시작
                    bool isSuccess = await TranslateCodeAsync(currentCode, previousLang, selectedLang);

                    if (isSuccess) previousLang = selectedLang;
                    else RevertComboBox();

                    return;
                }
            }

            txtCode.Text = GetDefaultTemplate(selectedLang);
            previousLang = selectedLang;
        }

        private async void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked) await LoadWrongListUI();
        }

        private async void rbCorrect_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCorrect.Checked) await LoadWrongListUI();
        }

        private async void rbWrong_CheckedChanged(object sender, EventArgs e)
        {
            if (rbWrong.Checked) await LoadWrongListUI();
        }

        // ⭐ 콤보박스를 원래 상태로 되돌려주는 도우미 메서드
        private void RevertComboBox()
        {
            cbLanguage.SelectedIndexChanged -= cbLanguage_SelectedIndexChanged;
            cbLanguage.SelectedItem = previousLang;
            cbLanguage.SelectedIndexChanged += cbLanguage_SelectedIndexChanged;
        }

        // ⭐ Google Gemini API 번역 메서드 (인공지능 자동 탐색 기능 탑재! 🚀)
        private async Task<bool> TranslateCodeAsync(string code, string sourceLang, string targetLang)
        {
            // 🚨 네 진짜 API 키!
            string apiKey = "AIzaSyABPN1NgXz6-kFZWpF90tnXwbh1okWC0Tw".Trim();

            cbLanguage.Enabled = false;
            string originalText = txtCode.Text;

            // 1단계 알림
            txtCode.Text = $"/* 🤖 구글 서버에서 이 API 키로 쓸 수 있는 AI 모델을 찾고 있습니다...\n   잠시만 기다려주세요! 🚀 */";

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // =====================================================================
                    // [1단계] 내 API 키로 쓸 수 있는 진짜 모델 목록을 구글에 물어보기 (자동 탐색)
                    // =====================================================================
                    string getModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                    var getResponse = await client.GetAsync(getModelsUrl);

                    if (!getResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("API 키 권한이 없거나 잘못되었습니다.", "API 키 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCode.Text = originalText;
                        return false;
                    }

                    string getResponseBody = await getResponse.Content.ReadAsStringAsync();
                    var modelsData = Newtonsoft.Json.Linq.JObject.Parse(getResponseBody);

                    string targetModel = "";

                    // 사용 가능한 수많은 모델 중 'gemini'가 포함되고 '텍스트 생성'이 가능한 첫 번째 모델을 낚아챔!
                    foreach (var model in modelsData["models"])
                    {
                        string modelName = model["name"]?.ToString();
                        var methods = model["supportedGenerationMethods"] as Newtonsoft.Json.Linq.JArray;

                        if (modelName != null && modelName.Contains("gemini") && methods != null)
                        {
                            foreach (var method in methods)
                            {
                                if (method.ToString() == "generateContent")
                                {
                                    targetModel = modelName; // 예: models/gemini-1.5-flash
                                    break;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(targetModel)) break; // 찾았으면 탐색 종료!
                    }

                    if (string.IsNullOrEmpty(targetModel))
                    {
                        MessageBox.Show("이 API 키로 사용할 수 있는 Gemini 모델이 없습니다.", "모델 검색 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCode.Text = originalText;
                        return false;
                    }

                    // 2단계 알림
                    txtCode.Text = $"/* 🤖 자동 탐색 성공! [{targetModel}] 모델로 코드를 번역합니다...\n   잠시만 기다려주세요! ✨ */";

                    // =====================================================================
                    // [2단계] 자동으로 찾아낸 진짜 모델 이름으로 완벽하게 번역 요청 보내기
                    // =====================================================================
                    string url = $"https://generativelanguage.googleapis.com/v1beta/{targetModel}:generateContent?key={apiKey}";

                    // ⭐ 프롬프트만 코딩 테스트 전용으로 초강력하게 수정했습니다!
                    string prompt = $@"You are an expert competitive programming translator. Translate the following {sourceLang} code to {targetLang}. 
Strict Rules:
1. EXACT MATCH: Maintain the exact same logic and algorithm. DO NOT optimize or fix bugs.
2. DATA TYPES: Preserve 64-bit integers (e.g. long) strictly. Do not downgrade to int.
3. FORMAT: Output ONLY the raw translated code. Do NOT wrap the code in markdown blocks like ```csharp or ```java. Do NOT add any explanations or text, just the code itself.

Code to translate:
{code}";

                    var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                    string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"번역 통신 실패 (상태 코드: {(int)response.StatusCode})\n\n[구글 서버 응답]\n{errorDetail}", "API 통신 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCode.Text = originalText;
                        return false;
                    }

                    string responseString = await response.Content.ReadAsStringAsync();
                    var responseObject = Newtonsoft.Json.Linq.JObject.Parse(responseString);

                    string translatedCode = responseObject["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();

                    // 거슬리는 마크다운 기호(```) 떼어내기
                    if (translatedCode.StartsWith("```"))
                    {
                        var lines = translatedCode.Split('\n');
                        translatedCode = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
                    }

                    // ⭐ 파이썬 들여쓰기 날아가는 것을 방지하기 위해 Trim() 대신 TrimEnd() 사용
                    txtCode.Text = translatedCode.TrimEnd();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"인터넷 연결이나 시스템 오류가 발생했습니다:\n{ex.Message}", "시스템 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCode.Text = originalText;
                return false;
            }
            finally
            {
                cbLanguage.Enabled = true;
            }
        }

        // ⭐ 창의 'X' 버튼을 눌러서 강제로 끌 때, 학습 기록이 날아가지 않게 자동 저장해주는 안전장치
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 현재 상태가 'Break(휴식)'이 아니라면 = 아직 세션 종료를 안 누르고 학습 중이라면!
            if (sessionManager.CurrentStatus != "Break")
            {
                // 1. 창이 바로 꺼져버리지 않도록 잠시 멈춤(Cancel)
                e.Cancel = true;

                // 2. 타이머를 멈추고 세션 상태를 강제로 종료시킴
                learningTimer.Stop();
                sessionManager.StopSession();

                // 3. 저장할 학습 시간 데이터 조립
                int duration = sessionManager.GetTotalSeconds();
                var sessionData = new SessionData
                {
                    status = "Break",
                    sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    sessionDuration = duration,
                    lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // 4. 파이어베이스에 안전하게 자동 저장 🚀
                await firebaseManager.SaveSessionAsync(sessionData);
                await firebaseManager.PushSessionLogAsync(sessionData);

                // 5. 저장이 완벽하게 끝났으니, 다시 강제로 닫히지 않게 연결을 끊고 진짜로 창을 닫음!
                this.FormClosing -= Form1_FormClosing;
                this.Close();
            }
        }

    }
}