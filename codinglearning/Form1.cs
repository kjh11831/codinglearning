using codinglearning.Managers;
using codinglearning.Models;
using codinglearning.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO; // 파일 저장을 위해 추가
using System.Diagnostics; // 백그라운드에서 cmd 명령어를 실행하기 위해 필요

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

            // UI 스타일 적용 메서드 호출
            ApplyMinimalStyle();

            // 앱 실행 시 세션 시작
            StartLearningSession();
        }

        private void learningTimer_Tick(object sender, EventArgs e)
        {
            if (sessionManager.CurrentStatus == "Active")
            {
                lblTimer.Text = sessionManager.GetCurrentDuration().ToString(@"hh\:mm\:ss");
                sessionManager.UpdateIdleState();
            }

            // 매개변수 없이 호출하도록 수정
            UpdateStatusUI();
        }

        // 매개변수를 없애고 sessionManager의 상태를 기준으로 UI를 업데이트하도록 통합
        private void UpdateStatusUI()
        {
            if (sessionManager.CurrentStatus == "Active")
            {
                lblStatus.Text = "▶ 학습 중";
                lblStatus.ForeColor = Color.Green; // 초록색
                lblTimer.ForeColor = Color.Green;  // 타이머도 같이 초록색
                btnStopSession.Text = "⏹ 세션 종료";
            }
            else if (sessionManager.CurrentStatus == "Idle")
            {
                lblStatus.Text = "Ⅱ 잠깐 쉬는 중";
                lblStatus.ForeColor = Color.Orange; // 주황색
                lblTimer.ForeColor = Color.Orange;  // 타이머도 같이 주황색
                btnStopSession.Text = "⏹ 세션 종료";
            }
            else // "Break" 상태
            {
                lblStatus.Text = "■ 휴식 중";
                lblStatus.ForeColor = Color.Crimson; // 약간 어두운 빨간색
                lblTimer.ForeColor = Color.Crimson;  // 타이머도 같이 빨간색
                btnStopSession.Text = "▶ 세션 시작";
            }
        }

        // 누락되었던 세션 시작 메서드 추가
        private void StartLearningSession()
        {
            sessionManager.StartSession();
            learningTimer.Start();
            UpdateStatusUI();
        }

        private async void btnStopSession_Click(object sender, EventArgs e)
        {
            // 현재 휴식 중이라면 -> 다시 시작! (sessionManager의 상태로 확인)
            if (sessionManager.CurrentStatus == "Break")
            {
                StartLearningSession(); // 위에서 추가한 메서드 호출
                return;
            }

            // 현재 학습 중이라면 -> 데이터 저장 후 종료
            learningTimer.Stop();
            sessionManager.StopSession(); // Manager의 상태를 Break로 변경
            UpdateStatusUI(); // UI 상태 업데이트 (매개변수 제거됨)

            int duration = sessionManager.GetTotalSeconds();
            var sessionData = new SessionData
            {
                status = "Break",
                sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sessionDuration = duration,
                lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 1. 현재 상태 저장 (기존)
            await firebaseManager.SaveSessionAsync(sessionData);

            // 2. 그래프와 표를 위한 통계 기록 누적 저장
            await firebaseManager.PushSessionLogAsync(sessionData);

            // 1. 총 학습 시간(초) 가져오기
            duration = sessionManager.GetTotalSeconds();

            // ⭐ 2. 초(Seconds)를 시간, 분, 초로 변환하기
            TimeSpan ts = TimeSpan.FromSeconds(duration);
            string formattedTime = "";

            // 0시간이나 0분일 때는 굳이 안 나오게 깔끔하게 처리
            if (ts.Hours > 0) formattedTime += $"{ts.Hours}시간 ";
            if (ts.Minutes > 0) formattedTime += $"{ts.Minutes}분 ";
            formattedTime += $"{ts.Seconds}초";

            // 3. 변환된 예쁜 시간으로 알림창 띄우기
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
            sessionManager.RecordUserAction();
            btnSearch.Enabled = false; btnSearch.Text = "검색중...";

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
                        if (count >= 50) break; // MVP 성능 제한

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
                btnSearch.Enabled = true; btnSearch.Text = "검색";
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

                // 양쪽 탭 라벨 동시 갱신
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
            sessionManager.RecordUserAction();
            if (string.IsNullOrEmpty(selId))
            {
                MessageBox.Show("먼저 문제를 선택해주세요!");
                return;
            }

            btnRunSample.Enabled = false; txtResult.Text = "Judge0 서버에 채점 요청 중...";

            try
            {
                var (isCorrect, message) = await apiService.RunJudge0Async(txtCode.Text, cbLanguage.SelectedItem.ToString());
                txtResult.Text = message;

                // 정답/오답 상관없이 무조건 저장 메서드 실행 (isCorrect 결과를 함께 넘겨줌)
                SaveCodeToLocalFile(selId, selTitle, txtCode.Text, cbLanguage.SelectedItem.ToString(), isCorrect);

                var record = new SubmissionRecord
                {
                    code = txtCode.Text,
                    status = isCorrect ? "correct" : "wrong",
                    language = cbLanguage.SelectedItem.ToString(),
                    date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    title = selTitle
                };

                await firebaseManager.SaveSubmissionAsync(selId, record, selDiff, selTags);
            }
            catch (Exception ex)
            {
                txtResult.Text = $"채점 서버 통신 오류: {ex.Message}";
            }
            finally
            {
                btnRunSample.Enabled = true;
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

        private async Task LoadWrongListUI()
        {
            // 1. Firebase에서 데이터 가져오기
            var dict = await firebaseManager.GetWrongListAsync();
            dgvWrongList.Rows.Clear();
            if (dict == null) return;

            // 2. 컬럼 헤더 텍스트 설정 (화면상 7개 컬럼 기준)
            dgvWrongList.ColumnCount = 7;
            dgvWrongList.Columns[0].HeaderText = "번호";
            dgvWrongList.Columns[1].HeaderText = "제목";
            dgvWrongList.Columns[2].HeaderText = "난이도";
            dgvWrongList.Columns[3].HeaderText = "태그";
            dgvWrongList.Columns[4].HeaderText = "결과";
            dgvWrongList.Columns[5].HeaderText = "오답 발생일";
            dgvWrongList.Columns[6].HeaderText = "복습 예정일";

            // 3. 데이터 바인딩
            foreach (var item in dict)
            {
                string pNum = item.Key;
                string title = item.Value["title"]?.ToString() ?? "-";
                string diff = item.Value["diff"]?.ToString() ?? "-";
                string tags = item.Value["tags"]?.ToString() ?? "-";
                bool isSolved = item.Value["solvedAfter"] != null && (bool)item.Value["solvedAfter"];
                string date = item.Value["addedDate"]?.ToString() ?? "-";
                string reviewDateStr = item.Value["reviewDate"]?.ToString() ?? "-"; 

                // 1. 표에 먼저 데이터를 한 줄 추가하고, 그 줄의 인덱스(순서)를 기억합니다.
                int rowIndex = dgvWrongList.Rows.Add(
                    pNum,
                    title,
                    diff,
                    tags,
                    isSolved ? "✅ 해결됨" : "❌ 미해결",
                    date,
                    reviewDateStr 
                );

                // 2. 강조 로직: 미해결 상태이고, 날짜 데이터가 정상적으로 변환될 때
                if (!isSolved && DateTime.TryParse(reviewDateStr, out DateTime reviewDate))
                {
                    // 복습 예정일이 현재 시간(지금)보다 과거라면? = 기한 지남!
                    if (reviewDate < DateTime.Now)
                    {
                        // 튀지 않는 차분한 붉은색(IndianRed)으로 글자색 변경 후 살짝 굵게 처리
                        dgvWrongList.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.IndianRed;
                        dgvWrongList.Rows[rowIndex].DefaultCellStyle.Font = new Font(dgvWrongList.Font, FontStyle.Bold);
                    }
                }
            }
        }

        private async void dgvWrongList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. 학습 상태 갱신 (사용자 활동 감지)
            sessionManager.RecordUserAction();

            // 2. 유효한 행을 클릭했는지 확인 (헤더 클릭 방지)
            if (e.RowIndex >= 0)
            {
                // 3. 클릭한 행의 첫 번째 셀(문제 번호) 값을 가져옴
                object cellValue = dgvWrongList.Rows[e.RowIndex].Cells[0].Value;
                string probNum = cellValue?.ToString();

                // 4. Firebase에서 오답 목록 데이터 호출
                var dict = await firebaseManager.GetWrongListAsync();

                // 5. 방어 코드: probNum이 null이 아니고, 딕셔너리에 해당 키가 존재할 때만 실행
                // string.IsNullOrWhiteSpace는 null, "", " " 모두를 체크해주는 아주 안전한 함수야.
                if (!string.IsNullOrWhiteSpace(probNum) && dict != null && dict.ContainsKey(probNum))
                {
                    // 6. 데이터를 JObject로 변환하여 안전하게 접근
                    JObject data = JObject.FromObject(dict[probNum]);

                    // 7. UI 라벨 업데이트 (값이 없을 경우를 대비해 null 체크 포함)
                    lblWrongProbNum.Text = probNum;
                    lblWrongProbTitle.Text = data["title"] != null ? data["title"].ToString() : "-";
                    lblWrongProbDiff.Text = data["diff"] != null ? data["diff"].ToString() : "-";
                    lblWrongProbTags.Text = data["tags"] != null ? data["tags"].ToString() : "-";

                    // 8. 해결 여부 판단 로직[cite: 1]
                    bool isSolved = data["solvedAfter"] != null && (bool)data["solvedAfter"];
                    lblWrongProbResult.Text = isSolved ? "해결됨" : "미해결";
                }
                else
                {
                    // 데이터가 없는 빈 줄을 클릭했을 경우 라벨 초기화 (선택 사항)
                    ClearWrongDetailLabels();
                }
            }
        }

        // 라벨 초기화용 도우미 메서드
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

            tabControl1.SelectedIndex = 1; // 2번 탭으로 이동
        }

        private async Task LoadStatisticsUI()
        {
            var dict = await firebaseManager.GetAllSubmissionsAsync();
            if (dict == null) return;

            int totalProblems = 0, correctCount = 0;

            if (dgvRecentRecords != null)
            {
                dgvRecentRecords.Rows.Clear();
                dgvRecentRecords.ColumnCount = 4;
                dgvRecentRecords.Columns[0].Name = "번호";
                dgvRecentRecords.Columns[1].Name = "제목";
                dgvRecentRecords.Columns[2].Name = "결과";
                dgvRecentRecords.Columns[3].Name = "날짜";
            }

            foreach (var problem in dict)
            {
                totalProblems++;
                bool hasCorrect = false;

                foreach (var attempt in problem.Value["attempts"])
                {
                    string status = attempt.Value["status"];
                    string date = attempt.Value["date"];
                    string title = attempt.Value["title"] != null ? attempt.Value["title"].ToString() : "-";

                    if (status == "correct") hasCorrect = true;

                    if (dgvRecentRecords != null)
                    {
                        dgvRecentRecords.Rows.Add(problem.Key, title, status == "correct" ? "✅ 정답" : "❌ 오답", date);
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

                // 1. 차트 막대 색상 지정 (뮤트 톤)
                Color correctColor = Color.FromArgb(143, 188, 143);
                Color wrongColor = Color.FromArgb(224, 159, 150);

                chartAccuracy.Series[0].Points[0].Color = correctColor;
                chartAccuracy.Series[0].Points[1].Color = wrongColor;

                // 2. 기본으로 생기는 파란색 '풀이 통계' 범례 숨기기
                chartAccuracy.Series[0].IsVisibleInLegend = false;

                // 3. 우리가 지정한 색상으로 직접 예쁜 범례(Legend) 만들기
                chartAccuracy.Legends[0].CustomItems.Clear();
                chartAccuracy.Legends[0].CustomItems.Add(correctColor, "정답");
                chartAccuracy.Legends[0].CustomItems.Add(wrongColor, "오답");
            }
        }
        #endregion

        private void lblGitHubPush_Click(object sender, EventArgs e)
        {
            // 1. 코드를 저장하던 폴더 경로 가져오기
            string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string targetFolder = Path.Combine(docsPath, "CodingLearning_Submissions");

            if (!Directory.Exists(targetFolder))
            {
                MessageBox.Show("아직 저장된 코드가 없어요.\n먼저 문제를 풀어보세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 2. 터미널(cmd) 명령어 실행 준비
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe");
                processInfo.WorkingDirectory = targetFolder; // 이 폴더 안에서 명령어를 실행하겠다는 뜻

                // 3. 커밋 메시지에 오늘 날짜와 시간 자동으로 넣기
                string commitMessage = $"Auto commit: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")} 학습 기록";

                // 4. 실행할 깃허브 명령어 3단 콤보 (add -> commit -> push)
                processInfo.Arguments = $"/c git add . && git commit -m \"{commitMessage}\" && git push";

                // 5. 까만 콘솔 창을 화면에 안 띄우고 몰래(백그라운드에서) 실행하기!
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;

                // 6. 실행 및 결과 확인
                using (Process process = Process.Start(processInfo))
                {
                    process.WaitForExit(); // 업로드가 끝날 때까지 프로그램이 잠시 기다림
                    string error = process.StandardError.ReadToEnd(); // 혹시 에러가 났는지 확인

                    // ExitCode가 0이면 성공, 아니면 실패
                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show("🌱 깃허브에 성공적으로 코드가 업로드되었습니다!", "잔디 심기 성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // 변경 사항이 없어서 커밋할 게 없는 경우도 에러(ExitCode 1)로 잡히기 때문에 예외 처리
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

        // 1. 이 변수는 반드시 메서드 바깥(위쪽)에 있어야 함 (이벤트 메서드가 호출될 때마다 초기화되면 안 되니까)
        private string previousLang = "C#";

        // 2. 여기서부터 끝까지 콤보박스 이벤트 메서드 덮어쓰기
        private void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLang = cbLanguage.SelectedItem?.ToString();

            // 같은 언어를 또 클릭한 거면 무시
            if (selectedLang == previousLang) return;

            bool isUserCode = false;
            string currentCode = txtCode.Text;

            if (!string.IsNullOrWhiteSpace(currentCode))
            {
                // 조건 A: 기본 주석을 아예 지워버린 경우
                if (!currentCode.Contains("여기에 코드를 작성하세요"))
                {
                    isUserCode = true;
                }
                // 조건 B: 주석은 놔뒀지만, 코드를 추가해서 '언어별 기본 길이'보다 길어진 경우
                else
                {
                    int len = currentCode.Length;
                    if (previousLang == "C#" && len > 190) isUserCode = true;
                    else if (previousLang == "C++" && len > 110) isUserCode = true;
                    else if (previousLang == "Java" && len > 140) isUserCode = true;
                    else if (previousLang == "Python" && len > 95) isUserCode = true;
                }
            }

            // 유저가 짠 코드가 맞다면 경고창 띄우기
            if (isUserCode)
            {
                DialogResult result = MessageBox.Show("이미 작성 중인 코드가 있습니다. 언어를 변경하고 초기화할까요?", "언어 변경", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // '아니요'를 누른 경우
                if (result == DialogResult.No)
                {
                    // 콤보박스 선택이 바뀌는 이벤트 잠시 끄고 원상복구
                    cbLanguage.SelectedIndexChanged -= cbLanguage_SelectedIndexChanged;
                    cbLanguage.SelectedItem = previousLang;
                    cbLanguage.SelectedIndexChanged += cbLanguage_SelectedIndexChanged;
                    return; // 뼈대 코드 생성 취소
                }
            }

            // '예'를 누르거나, 처음 빈 화면일 때 뼈대 코드 세팅
            switch (selectedLang)
            {
                case "C#":
                    txtCode.Text =
    @"using System;

namespace CodingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // 여기에 코드를 작성하세요
            
        }
    }
}";
                    break;

                case "C++":
                    txtCode.Text =
    @"#include <iostream>
using namespace std;

int main() {
    // 여기에 코드를 작성하세요
    
    return 0;
}";
                    break;

                case "Java":
                    txtCode.Text =
    @"import java.util.*;

public class Main {
    public static void main(String[] args) {
        // 여기에 코드를 작성하세요
        
    }
}";
                    break;

                case "Python":
                    txtCode.Text =
    @"def main():
    # 여기에 코드를 작성하세요
    pass

if __name__ == '__main__':
    main()";
                    break;

                default:
                    txtCode.Text = "";
                    break;
            }

            // 방금 세팅 완료된 언어를 '이전 언어'로 업데이트
            previousLang = selectedLang;
        }

        private async Task LoadTimeStatisticsUI()
        {
            // 1. Firebase에서 누적 세션 기록 가져오기
            var logs = await firebaseManager.GetAllSessionLogsAsync();

            // 데이터가 아예 없으면 표 초기화 후 종료
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

            // 최근 7일치 날짜 기본 틀 만들기 (그래프용)
            Dictionary<string, int> dailyStudyMap = new Dictionary<string, int>();
            for (int i = 6; i >= 0; i--)
            {
                dailyStudyMap[now.AddDays(-i).ToString("yyyy-MM-dd")] = 0;
            }

            // 2. 표(DataGridView) 설정 및 데이터 가공
            dgvTimeRecords.Rows.Clear();
            dgvTimeRecords.ColumnCount = 4;
            dgvTimeRecords.Columns[0].Name = "날짜";
            dgvTimeRecords.Columns[1].Name = "학습 구간";
            dgvTimeRecords.Columns[2].Name = "순공 시간";
            dgvTimeRecords.Columns[3].Name = "성과";

            // 최신 기록이 위로 오도록 정렬
            foreach (var item in logs.OrderByDescending(x => x.Value.sessionEnd))
            {
                var log = item.Value;
                if (!DateTime.TryParse(log.sessionEnd, out DateTime endTime)) continue;

                // 시작 시간 계산 (종료 시간 - 학습 시간)
                DateTime startTime = endTime.AddSeconds(-log.sessionDuration);
                string logDate = endTime.ToString("yyyy-MM-dd");

                // [상단 요약] 데이터 합산
                if (logDate == todayStr) todayTotalSeconds += log.sessionDuration;
                if (endTime >= weekAgo) weeklyTotalSeconds += log.sessionDuration;
                if (log.sessionDuration > maxFocusSeconds) maxFocusSeconds = log.sessionDuration;

                // [그래프] 해당 날짜에 시간 누적
                if (dailyStudyMap.ContainsKey(logDate))
                {
                    dailyStudyMap[logDate] += log.sessionDuration;
                }

                // [표] 한 줄 데이터 만들기
                TimeSpan duration = TimeSpan.FromSeconds(log.sessionDuration);
                string interval = $"{startTime:HH:mm} ~ {endTime:HH:mm}"; // 예: 14:00 ~ 15:30
                string netTime = duration.Hours > 0 ? $"{duration.Hours}시간 {duration.Minutes}분" : $"{duration.Minutes}분 {duration.Seconds}초";

                // 성과는 현재 세션 저장 로직 상 '학습 완료'로 기본 세팅 (추후 문제 풀이 수와 연동 가능)
                dgvTimeRecords.Rows.Add(logDate, interval, netTime, "학습 완료");
            }

            // 3. 상단 요약(KPI) 라벨 텍스트 적용
            if (lblTodayTotal != null)
            {
                TimeSpan tToday = TimeSpan.FromSeconds(todayTotalSeconds);
                // 조건문을 없애고 항상 시간(h), 분(m), 초(s)를 출력하도록 변경
                lblTodayTotal.Text = $"{tToday.Hours}h {tToday.Minutes}m {tToday.Seconds}s";
            }

            if (lblWeeklyAvg != null)
            {
                int avgSeconds = weeklyTotalSeconds / 7; // 7일 평균
                TimeSpan tAvg = TimeSpan.FromSeconds(avgSeconds);
                lblWeeklyAvg.Text = $"{tAvg.Hours}h {tAvg.Minutes}m {tAvg.Seconds}s";
            }

            if (lblMaxFocus != null)
            {
                TimeSpan tMax = TimeSpan.FromSeconds(maxFocusSeconds);
                lblMaxFocus.Text = $"{tMax.Hours}h {tMax.Minutes}m {tMax.Seconds}s";
            }

            // 4. 그래프(Chart) 업데이트
            if (chartTimeHistory != null)
            {
                chartTimeHistory.Series.Clear();
                var series = chartTimeHistory.Series.Add("학습 시간(분)");

                // 캡처하신 화면에 맞게 가로 막대(Bar)로 설정
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;
                series.Color = Color.FromArgb(160, 170, 180); // 세련된 웜그레이+블루 느낌의 뉴트럴 톤
                series.IsValueShownAsLabel = true; // 막대 끝에 숫자(분) 표시

                foreach (var kvp in dailyStudyMap)
                {
                    // "2026-05-06" 에서 "05-06"만 잘라서 Y축 글씨 겹침 방지
                    string shortDate = kvp.Key.Substring(5);
                    double minutes = Math.Round(kvp.Value / 60.0, 1);
                    series.Points.AddXY(shortDate, minutes);
                }
            }
        }

        private void ApplyMinimalStyle()
        {
            // 1. 데이터그리드뷰(표) 다이어트 및 색상 빼기
            // 폼 안에 있는 모든 표에 동일한 스타일을 적용합니다.
            DataGridView[] grids = { dgvTimeRecords, dgvWrongList, dgvProblems, dgvRecentRecords };

            foreach (var grid in grids)
            {
                if (grid == null) continue;

                // 배경을 완전 하얗게, 겉 테두리는 없애기
                grid.BackgroundColor = Color.White;
                grid.BorderStyle = BorderStyle.None;

                // 셀 테두리는 가로선만 아주 연한 회색으로 (세로선을 없애서 시원하게 만듦)
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                grid.GridColor = Color.FromArgb(240, 240, 240);

                // 선택했을 때 눈 아픈 파란색 대신, 세련된 라이트 그레이로 설정
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 245, 245);
                grid.DefaultCellStyle.SelectionForeColor = Color.Black;

                // 불필요한 맨 왼쪽 화살표 칸(RowHeaders) 숨기기
                grid.RowHeadersVisible = false;
            }

            // 2. 차트 격자무늬(Grid) 연하게 만들거나 없애기
            if (chartTimeHistory != null)
            {
                // X축 (세로선) 완전 삭제
                chartTimeHistory.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;

                // Y축 (가로선) 아주 연한 회색으로 얇게 처리
                chartTimeHistory.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                chartTimeHistory.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartTimeHistory.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            }

            // chartAccuracy(정답률 차트) 설정
            if (chartAccuracy != null)
            {
                // 세로 막대그래프의 세로선(X축 격자) 완전 제거
                chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;

                // 가로선(Y축 격자)은 눈에 띄지 않게 아주 연한 회색으로
                chartAccuracy.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                chartAccuracy.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartAccuracy.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            }
        }

        private void SaveCodeToLocalFile(string problemId, string title, string code, string language, bool isCorrect)
        {
            try
            {
                // 1. 기본 저장 폴더 설정
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string baseFolder = Path.Combine(docsPath, "CodingLearning_Submissions");

                // 2. 정답/오답에 따라 예쁘게 하위 폴더 나누기 📁
                string subFolder = isCorrect ? "Correct" : "Wrong";
                string targetFolder = Path.Combine(baseFolder, subFolder);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder); // 폴더가 없으면 알아서 생성!
                }

                // 3. 언어에 맞는 확장자 결정
                string extension = ".txt";
                if (language == "C#") extension = ".cs";
                else if (language == "C++") extension = ".cpp";
                else if (language == "Python") extension = ".py";
                else if (language == "Java") extension = ".java";

                // 4. 파일명에 못 쓰는 특수문자 필터링 (깔끔한 파일명을 위해)
                string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));

                // 5. 상황에 맞는 파일 이름 만들기
                string fileName;
                if (isCorrect)
                {
                    // 정답일 경우: [123A] Watermelon.cpp (최종 정답본 1개만 깔끔하게 유지)
                    fileName = $"[{problemId}] {safeTitle}{extension}";
                }
                else
                {
                    // 오답일 경우: [123A] Watermelon_20260506_160436.cpp (시간을 붙여서 오답 히스토리 누적)
                    string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    fileName = $"[{problemId}] {safeTitle}_{timeStamp}{extension}";
                }

                string filePath = Path.Combine(targetFolder, fileName);

                // 6. 파일 쓰기
                File.WriteAllText(filePath, code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 자동 저장 실패: {ex.Message}");
            }
        }
    }
}