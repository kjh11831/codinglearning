using codinglearning.Managers;
using codinglearning.Models;
using codinglearning.Services;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.LinkLabel;

namespace codinglearning
{
    public partial class Form1 : Form
    {
        // 1. 선언만 수행 (디자이너가 폼을 읽을 때 오류 발생 방지)
        private LearningSessionManager sessionManager;
        private GitHubManager gitHubManager;
        private FileManager fileManager;
        private FirebaseManager firebaseManager;
        private ApiService apiService;
        private GeminiService geminiService;

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCF;

        private string selId = "", selTitle = "", selDiff = "", selTags = "";
        private bool isSearching = false;
        private bool isRunningSample = false;

        private string previousLang = "C#";
        private string lastSourceCode = "";
        private string lastSourceLang = "";
        private string lastTargetLang = "";

        private Panel tabCoverPanel;
        private bool isDarkMode = false;

        public Form1()
        {
            InitializeComponent();

            // 2. 생성자 내부에서 실제 객체 할당
            sessionManager = new LearningSessionManager();
            gitHubManager = new GitHubManager();
            fileManager = new FileManager();
            firebaseManager = new FirebaseManager();
            apiService = new ApiService();
            geminiService = new GeminiService();
        }

        // 🌟 void 앞에 async를 반드시 붙여주세요!
        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                // 방어 코드: 브라우저가 아직 로딩 중이거나 닫힌 상태면 무시
                if (webViewCF == null || webViewCF.CoreWebView2 == null || webViewCF.Source == null) return;

                string currentUrl = webViewCF.Source.ToString();

                // =========================================================================
                // 🌟 [추가할 부분] 페이지가 바뀔 때마다 다크 모드 상태를 체크해서 끈질기게 색상 반전 유지!
                string darkModeCss = isDarkMode ? "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "document.documentElement.style.filter = 'none';";
                await webViewCF.CoreWebView2.ExecuteScriptAsync(darkModeCss);
                // =========================================================================

                // 🌟 핵심 방어막: 브라우저가 내부 이벤트를 안전하게 마무리할 수 있도록 0.5초(500ms) 양보합니다!
                await Task.Delay(500);

                // 1. 로그인이 안 되어 있어서 로그인 페이지(/enter)로 튕긴 경우
                if (currentUrl.Contains("codeforces.com/enter"))
                {
                    if (webViewCF.Parent == this)
                    {
                        Form loginForm = new Form();
                        loginForm.Text = "코드포스 계정 로그인";
                        loginForm.Size = new Size(800, 700);
                        loginForm.StartPosition = FormStartPosition.CenterParent;

                        webViewCF.Visible = true;
                        loginForm.Controls.Add(webViewCF);

                        loginForm.FormClosing += (s, ev) =>
                        {
                            loginForm.Controls.Remove(webViewCF);
                            this.Controls.Add(webViewCF);
                            webViewCF.Visible = false;
                        };

                        MessageBox.Show("자동 제출을 위해 로그인이 필요합니다.\n팝업창이 열리면 로그인을 완료해 주세요!", "로그인 안내");
                        loginForm.ShowDialog(this);
                    }
                }
                // 2. 로그인을 성공해서 설정 페이지나 메인 화면으로 진입한 경우
                else if (currentUrl.Contains("codeforces.com/settings") || currentUrl.Contains("codeforces.com/profile") || currentUrl == "https://codeforces.com/")
                {
                    if (webViewCF.Parent is Form parentForm && parentForm != this)
                    {
                        parentForm.Close();
                        MessageBox.Show("✅ 로그인 성공!\n이제 창을 띄우지 않고 백그라운드에서 자동 제출이 가능합니다.", "알림");
                    }
                }
            }
            catch
            {
                // 에러 무시
            }
        }

        #region [ 1. 공통 및 하단 상태 표시줄 ]
        private async void Form1_Load(object sender, EventArgs e)
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

            // 🌟 1. 코드로 WebView2 직접 생성 및 설정 (디자이너 개입 100% 차단)
            webViewCF = new Microsoft.Web.WebView2.WinForms.WebView2();
            webViewCF.Visible = false; // 처음엔 숨김
            webViewCF.Dock = DockStyle.Fill;

            // 🌟 2. 폼에 브라우저 컨트롤 부착!
            this.Controls.Add(webViewCF);

            // 🌟 3. 비동기로 안전하게 초기화 시작
            await InitializeWebViewAsync();

            ApplyMinimalStyle();
            StartLearningSession();

            lblDarkMode.Parent = this;
            lblDarkMode.Location = new Point(tabControl1.Width - lblDarkMode.Width - 15, 3);
            lblDarkMode.BringToFront();
            lblDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDarkMode.BackColor = Color.Transparent;
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            ApplyTheme();
            this.FormClosing += Form1_FormClosing;
        }

        // 🌟 4. 이름 변경 및 Task 반환형으로 수정
        private async Task InitializeWebViewAsync()
        {
            try
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodingLearning_WebView");

                if (!System.IO.Directory.Exists(userDataFolder))
                {
                    System.IO.Directory.CreateDirectory(userDataFolder);
                }

                var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder, null);

                await webViewCF.EnsureCoreWebView2Async(environment);

                webViewCF.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webViewCF.CoreWebView2.Navigate("https://codeforces.com/settings/general");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 초기화 실패!\n\n에러 메시지: {ex.Message}", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        private async void btnSearch_Click(object sender, EventArgs e) => await PerformSearch(txtKeyword.Text, txtMinDifficulty.Text, txtMaxDifficulty.Text);
        private async void btnSearchAll_Click(object sender, EventArgs e) => await PerformSearch("", "", "");

        private void btnResetSearch_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            txtKeyword.Text = ""; txtMinDifficulty.Text = ""; txtMaxDifficulty.Text = "";
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
                    dgvProblems.Columns[0].Name = "번호"; dgvProblems.Columns[1].Name = "제목";
                    dgvProblems.Columns[2].Name = "난이도"; dgvProblems.Columns[3].Name = "태그"; dgvProblems.Columns[4].Name = "결과";

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
            catch (Exception ex) { MessageBox.Show("오류: " + ex.Message); }
            finally { isSearching = false; btnSearch.Text = "검색"; }
        }

        private void dgvProblems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            sessionManager.RecordUserAction();
            if (e.RowIndex >= 0)
            {
                var row = dgvProblems.Rows[e.RowIndex];
                selId = lblSelProbNum.Text = lblCodeProbNum.Text = row.Cells[0].Value?.ToString();
                selTitle = lblSelProbTitle.Text = lblCodeProbTitle.Text = row.Cells[1].Value?.ToString();
                selDiff = lblSelProbDiff.Text = lblCodeProbDiff.Text = row.Cells[2].Value?.ToString();
                selTags = lblSelProbTags.Text = lblCodeProbTags.Text = row.Cells[3].Value?.ToString();
            }
        }

        private void btnViewProblem_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (!string.IsNullOrEmpty(selId))
            {
                string contestId = new String(selId.Where(Char.IsDigit).ToArray());
                string index = new String(selId.Where(Char.IsLetter).ToArray());
                string url = $"https://codeforces.com/problemset/problem/{contestId}/{index}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            else MessageBox.Show("목록에서 문제를 먼저 선택해주세요.");
        }
        #endregion

        #region [ 3. 탭 2: 코드 작성 및 채점 ]
        private void txtCode_TextChanged(object sender, EventArgs e) => sessionManager.RecordUserAction();
        private void btnResetCode_Click(object sender, EventArgs e) { sessionManager.RecordUserAction(); txtCode.Text = ""; txtResult.Text = ""; }

        private async void btnRunSample_Click(object sender, EventArgs e)
        {
            if (isRunningSample) return;

            // 코드가 비어있는지 검사
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("실행할 코드를 먼저 작성해주세요!");
                return;
            }

            isRunningSample = true;

            try
            {
                string currentLang = cbLanguage.SelectedItem.ToString();

                // 🌟 팝업창 및 Codeforces 예제 채점 로직 완전 삭제!
                // 어떤 상황이든 무조건 "단순 코드 실행"만 수행합니다.
                btnRunSample.Text = "코드 실행 중...";
                txtResult.Text = "⏳ 코드를 컴파일하고 실행하는 중입니다...\r\n\r\n";

                // ApiService를 통해 순수 코드만 실행하고 출력값을 받아옴
                string rawOutput = await apiService.RunCodeOnlyAsync(txtCode.Text, currentLang);

                // 결과 출력
                txtResult.AppendText("==================== [실행 결과] ====================\r\n");
                txtResult.AppendText(rawOutput);
                txtResult.AppendText("\r\n=====================================================");
            }
            catch (Exception ex)
            {
                txtResult.Text = $"오류 발생: {ex.Message}";
            }
            finally
            {
                isRunningSample = false;
                // 버튼 텍스트 원상복구
                btnRunSample.Text = "예제 테스트 실행";
            }
        }

        private async void btnSubmitCF_Click(object sender, EventArgs e)
        {
            // 🌟 1. 버튼이 이미 일하고 있다면 마우스 클릭을 무시함 (Enabled = false 역할)
            if (btnSubmitCF.Text != "CF 제출") return;

            sessionManager.RecordUserAction();

            if (string.IsNullOrEmpty(selId))
            {
                MessageBox.Show("문제를 먼저 선택해주세요.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("제출할 코드가 없습니다!");
                return;
            }

            string currentLang = cbLanguage.SelectedItem.ToString();
            string cfLangId = "51";
            if (currentLang == "C#") cfLangId = "65";
            else if (currentLang == "C++") cfLangId = "54";
            else if (currentLang == "Python") cfLangId = "71";
            else if (currentLang == "Java") cfLangId = "60";

            string contestId = new String(selId.Where(Char.IsDigit).ToArray());
            string index = new String(selId.Where(Char.IsLetter).ToArray());

            string submitUrl = $"https://codeforces.com/contest/{contestId}/submit";

            btnSubmitCF.Text = "제출 창 여는 중...";

            try
            {
                Form resultForm = new Form();
                resultForm.Text = "제출 내역 확인 (Codeforces)";
                resultForm.Size = new Size(1000, 700);
                resultForm.StartPosition = FormStartPosition.CenterParent;

                webViewCF.Visible = true;
                resultForm.Controls.Add(webViewCF);

                EventHandler<CoreWebView2NavigationCompletedEventArgs> autoFillHandler = null;
                autoFillHandler = async (s, args) =>
                {
                    // 🌟 1. 스크립트가 중복 실행되지 않도록 한 번 실행되면 즉시 연결 해제!
                    webViewCF.CoreWebView2.NavigationCompleted -= autoFillHandler;

                    // 에디터와 클라우드플레어가 넉넉히 로딩되도록 1.5초 대기
                    await Task.Delay(1500);

                    if (!resultForm.Visible) return;

                    string safeCode = txtCode.Text.Replace("\\", "\\\\").Replace("`", "\\`").Replace("$", "\\$");

                    // 🌟 다크 모드일 경우 웹페이지 색상을 반전시키는 CSS 필터 마법
                    string darkModeCss = isDarkMode ? "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "";

                    string script = $@"
            (function() {{
                {darkModeCss} // 화면 진입 즉시 다크 모드 필터 적용

                function triggerChange(el) {{
                    if(el) {{
                        el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }}

                var probSelect = document.querySelector('select[name=""submittedProblemIndex""]');
                if(probSelect) {{ 
                    probSelect.value = '{index}'; 
                    triggerChange(probSelect); 
                }}

                var langSelect = document.querySelector('select[name=""programTypeId""]');
                if(langSelect) {{ 
                    langSelect.value = '{cfLangId}'; 
                    triggerChange(langSelect); 
                }}

                // 🌟 핵심 수정: 언어 변경 후 에디터가 갱신되는 시간을 0.5초(500ms) 기다려준 뒤 코드 입력!
                setTimeout(function() {{
                    var safeCode = `{safeCode}`;
                    var editorEnv = window.ace ? window.ace.edit('editor') : null;
                    if (editorEnv) {{
                        editorEnv.setValue(safeCode, 1);
                    }} else {{
                        var textarea = document.getElementById('sourceCodeTextarea');
                        if(textarea) {{ 
                            textarea.value = safeCode; 
                            textarea.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        }}
                    }}
                }}, 500);
            }})();
        ";

                    await webViewCF.CoreWebView2.ExecuteScriptAsync(script);
                };

                webViewCF.CoreWebView2.NavigationCompleted += autoFillHandler;

                resultForm.FormClosing += (s, ev) =>
                {
                    // 창을 너무 빨리 닫았을 때를 대비한 2차 안전 해제
                    webViewCF.CoreWebView2.NavigationCompleted -= autoFillHandler;
                    resultForm.Controls.Remove(webViewCF);
                    this.Controls.Add(webViewCF);
                    webViewCF.Visible = false;
                };

                resultForm.Shown += (s, ev) =>
                {
                    webViewCF.CoreWebView2.Navigate(submitUrl);
                };

                resultForm.ShowDialog(this);

                btnSubmitCF.Text = "채점 결과 확인 중...";

                try
                {
                    string handleScript = @"
                (function() {
                    let link = document.querySelector('a[href^=""/profile/""]');
                    return link ? link.innerText.trim() : '';
                })();
            ";
                    string cfHandle = (await webViewCF.CoreWebView2.ExecuteScriptAsync(handleScript)).Trim('"');

                    if (!string.IsNullOrEmpty(cfHandle))
                    {
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            string apiUrl = $"https://codeforces.com/api/user.status?handle={cfHandle}&from=1&count=1";

                            string verdict = "TESTING";
                            bool foundSubmission = false;
                            int retryCount = 0;

                            // 🌟 핵심 수정: API 반영 지연을 고려하여 최대 30초(15회)까지 끈질기게 기다립니다.
                            while (retryCount < 15)
                            {
                                await Task.Delay(2000);
                                retryCount++;

                                string json = await client.GetStringAsync(apiUrl);
                                Newtonsoft.Json.Linq.JObject data = Newtonsoft.Json.Linq.JObject.Parse(json);

                                if (data["status"]?.ToString() == "OK" && data["result"] != null && data["result"].Any())
                                {
                                    var latestResult = data["result"][0];
                                    string pContestId = latestResult["problem"]["contestId"]?.ToString();
                                    string pIndex = latestResult["problem"]["index"]?.ToString();

                                    // API의 가장 최근 기록이 '방금 제출한 문제'와 일치하는지 확인
                                    if ($"{pContestId}{pIndex}" == selId)
                                    {
                                        foundSubmission = true;
                                        verdict = latestResult["verdict"]?.ToString() ?? "";

                                        // 채점이 완료되었으면(TESTING이 아니면) 기다림 종료!
                                        if (verdict != "TESTING" && verdict != "")
                                        {
                                            break;
                                        }
                                    }
                                    // 일치하지 않는다면 아직 API에 안 올라온 것이므로 다음 턴(2초 뒤)에 다시 확인 (continue)
                                }
                            }

                            if (!foundSubmission)
                            {
                                MessageBox.Show("API 서버 지연으로 제출 내역을 찾지 못했거나 제출이 누락되었습니다.", "저장 취소");
                            }
                            else if (verdict != "TESTING" && verdict != "")
                            {
                                string finalStatus = (verdict == "OK") ? "correct" : "wrong";

                                var record = new codinglearning.Models.SubmissionRecord
                                {
                                    code = txtCode.Text,
                                    status = finalStatus,
                                    language = currentLang,
                                    date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                    title = selTitle,
                                    diff = selDiff,
                                    tags = selTags
                                };

                                string langKey = currentLang.Replace("#", "Sharp").Replace("+", "p");
                                await firebaseManager.SaveSubmissionAsync($"{selId}_{langKey}", record, selDiff, selTags);

                                string resultMsg = (finalStatus == "correct") ? "✅ 정답 (Accepted)" : $"❌ 오답 ({verdict})";
                                MessageBox.Show($"채점 확인 완료: {resultMsg}\n통계와 오답 노트에 자동 저장되었습니다! 📊", "자동 기록 완료");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("로그인된 계정 정보를 찾을 수 없어 결과를 저장하지 못했습니다.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"결과를 가져오는 중 오류 발생: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"제출 중 오류 발생: {ex.Message}");
            }
            finally
            {
                btnSubmitCF.Text = "CF 제출";
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
            try
            {
                var wrongDict = await firebaseManager.GetWrongListAsync();
                var allDict = await firebaseManager.GetAllSubmissionsAsync();

                dgvWrongList.Rows.Clear();
                dgvWrongList.ColumnCount = 8;
                dgvWrongList.Columns[0].HeaderText = "번호"; dgvWrongList.Columns[1].HeaderText = "제목";
                dgvWrongList.Columns[2].HeaderText = "언어"; dgvWrongList.Columns[3].HeaderText = "난이도";
                dgvWrongList.Columns[4].HeaderText = "태그"; dgvWrongList.Columns[5].HeaderText = "결과";
                dgvWrongList.Columns[6].HeaderText = "발생일(풀이일)"; dgvWrongList.Columns[7].HeaderText = "복습 예정일";

                ClearWrongDetailLabels();

                HashSet<string> processedKeys = new HashSet<string>();
                List<object[]> rowDataList = new List<object[]>();

                // 1. submissions (전체 풀이 기록) 처리
                if (allDict != null)
                {
                    foreach (var entry in allDict)
                    {
                        string normKey = entry.Key;
                        processedKeys.Add(normKey);

                        Newtonsoft.Json.Linq.JObject pData = Newtonsoft.Json.Linq.JObject.FromObject(entry.Value);
                        string pNum = normKey.Split('_')[0];

                        string title = "-", lang = "-", date = "-";

                        // 🌟 [핵심 복구 1] 파이어베이스 데이터에서 난이도와 태그를 직접 긁어옵니다!
                        string diff = pData["diff"]?.ToString() ?? pData["difficulty"]?.ToString() ?? "-";
                        string tags = pData["tags"]?.ToString() ?? "-";

                        bool isSolved = false;
                        int tryCount = 0;

                        Newtonsoft.Json.Linq.JToken attemptsToken = pData["attempts"];
                        if (attemptsToken != null && attemptsToken.HasValues)
                        {
                            tryCount = attemptsToken.Children().Count();
                            var lastAttempt = attemptsToken.Last is Newtonsoft.Json.Linq.JProperty prop ? prop.Value : attemptsToken.Last;

                            title = lastAttempt["title"]?.ToString() ?? "-";
                            lang = lastAttempt["language"]?.ToString() ?? "-";
                            date = lastAttempt["date"]?.ToString() ?? "-";

                            // 만약 껍데기에 없다면 시도 기록 안에서라도 억지로 찾아오기
                            if (diff == "-") diff = lastAttempt["diff"]?.ToString() ?? "-";
                            if (tags == "-") tags = lastAttempt["tags"]?.ToString() ?? "-";

                            foreach (var attempt in attemptsToken)
                            {
                                var attData = attempt is Newtonsoft.Json.Linq.JProperty p ? p.Value : attempt;
                                if (attData["status"]?.ToString() == "correct") isSolved = true;
                            }
                        }

                        string reviewDate = "-";

                        // 🌟 [핵심 복구 2] 오답 노트 데이터와 결합하여 '복습 예정일'을 가져옵니다.
                        if (wrongDict != null && wrongDict.ContainsKey(normKey))
                        {
                            var wData = wrongDict[normKey];
                            if (diff == "-") diff = wData["diff"]?.ToString() ?? "-";
                            if (tags == "-") tags = wData["tags"]?.ToString() ?? "-";
                            reviewDate = wData["reviewDate"]?.ToString() ?? "-";
                        }

                        if (rbCorrect.Checked && !isSolved) continue;
                        if (rbWrong.Checked && isSolved) continue;

                        string resultText = isSolved ? $"✅ 해결됨 ({tryCount}-Try)" : "❌ 미해결";

                        // 🌟 해결된 문제(정답)는 복습 안 해도 되니 "-", 미해결은 복습 예정일 표시!
                        rowDataList.Add(new object[] { pNum, title, lang, diff, tags, resultText, date, isSolved ? "-" : reviewDate });
                    }
                }

                // 2. wrongList (오답 노트) 중 submissions에 없는 데이터 추가 처리
                if (wrongDict != null)
                {
                    foreach (var entry in wrongDict)
                    {
                        if (processedKeys.Contains(entry.Key)) continue;

                        string pNum = entry.Key.Split('_')[0];
                        string title = entry.Value["title"]?.ToString() ?? "-";
                        string lang = entry.Value["language"]?.ToString() ?? "알 수 없음";
                        string diff = entry.Value["diff"]?.ToString() ?? "-";
                        string tags = entry.Value["tags"]?.ToString() ?? "-";
                        string date = entry.Value["addedDate"]?.ToString() ?? "-";
                        string reviewDate = entry.Value["reviewDate"]?.ToString() ?? "-";
                        bool isSolved = entry.Value["solvedAfter"] != null && (bool)entry.Value["solvedAfter"];

                        if (rbCorrect.Checked && !isSolved) continue;
                        if (rbWrong.Checked && isSolved) continue;

                        string resultText = isSolved ? "✅ 해결됨" : "❌ 미해결";
                        rowDataList.Add(new object[] { pNum, title, lang, diff, tags, resultText, date, isSolved ? "-" : reviewDate });
                    }
                }

                // 3. 최신순 정렬 및 표 삽입
                var sortedRows = rowDataList.OrderByDescending(r => r[6]?.ToString() ?? "").ToList();
                foreach (var row in sortedRows)
                {
                    int idx = dgvWrongList.Rows.Add(row);
                    // 복습 기한 지난 미해결 문제 빨간색 글씨로 강조
                    if (row[5].ToString().Contains("미해결") && DateTime.TryParse(row[7].ToString(), out DateTime rvDate) && rvDate < DateTime.Now)
                    {
                        dgvWrongList.Rows[idx].DefaultCellStyle.ForeColor = Color.IndianRed;
                        dgvWrongList.Rows[idx].DefaultCellStyle.Font = new Font(dgvWrongList.Font, FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"내역 로드 오류: {ex.Message}");
            }
        }

        private void dgvWrongList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            sessionManager.RecordUserAction();
            if (e.RowIndex >= 0 && !dgvWrongList.Rows[e.RowIndex].IsNewRow)
            {
                DataGridViewRow row = dgvWrongList.Rows[e.RowIndex];

                // 🌟 8열 디자인에 맞춘 방 번호(Index) 재할당!
                lblWrongProbNum.Text = row.Cells[0].Value?.ToString() ?? "-";     // 0: 번호
                lblWrongProbTitle.Text = row.Cells[1].Value?.ToString() ?? "-";   // 1: 제목
                lblWrongProbDiff.Text = row.Cells[3].Value?.ToString() ?? "-";    // 3: 난이도 (2는 언어)
                lblWrongProbTags.Text = row.Cells[4].Value?.ToString() ?? "-";    // 4: 태그

                string resultText = row.Cells[5].Value?.ToString() ?? "-";        // 5: 결과
                lblWrongProbResult.Text = resultText.Contains("미해결") ? "미해결" : "해결됨";
            }
        }

        private void ClearWrongDetailLabels() { lblWrongProbNum.Text = "-"; lblWrongProbTitle.Text = "-"; lblWrongProbDiff.Text = "-"; lblWrongProbTags.Text = "-"; lblWrongProbResult.Text = "-"; }

        private void btnViewWrongProblem_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            string wrongId = lblWrongProbNum.Text;
            if (wrongId != "나야 번호" && !string.IsNullOrEmpty(wrongId))
            {
                string contestId = new String(wrongId.Where(Char.IsDigit).ToArray());
                string index = new String(wrongId.Where(Char.IsLetter).ToArray());
                string url = $"https://codeforces.com/problemset/problem/{contestId}/{index}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
            else MessageBox.Show("오답 목록에서 문제를 선택해주세요.");
        }

        private void btnSolveAgain_Click(object sender, EventArgs e)
        {
            sessionManager.RecordUserAction();
            if (lblWrongProbNum.Text == "나야 번호" || string.IsNullOrEmpty(lblWrongProbNum.Text))
            {
                MessageBox.Show("다시 풀 오답을 선택해주세요."); return;
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
                dgvRecentRecords.ColumnCount = 5;
                dgvRecentRecords.Columns[0].Name = "번호"; dgvRecentRecords.Columns[1].Name = "제목";
                dgvRecentRecords.Columns[2].Name = "언어"; dgvRecentRecords.Columns[3].Name = "결과"; dgvRecentRecords.Columns[4].Name = "날짜";
            }

            foreach (var problem in dict)
            {
                totalProblems++;
                bool hasCorrect = false;
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
                chartAccuracy.Series[0].ChartType = SeriesChartType.Column;
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

        private async Task LoadTimeStatisticsUI()
        {
            var logs = await firebaseManager.GetAllSessionLogsAsync();
            if (logs == null || logs.Count == 0) { dgvTimeRecords.Rows.Clear(); return; }

            DateTime now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");
            DateTime weekAgo = now.AddDays(-7);

            int todayTotalSeconds = 0, weeklyTotalSeconds = 0, maxFocusSeconds = 0;
            Dictionary<string, int> dailyStudyMap = new Dictionary<string, int>();
            for (int i = 6; i >= 0; i--) dailyStudyMap[now.AddDays(-i).ToString("yyyy-MM-dd")] = 0;

            dgvTimeRecords.Rows.Clear();
            dgvTimeRecords.ColumnCount = 4;
            dgvTimeRecords.Columns[0].Name = "날짜"; dgvTimeRecords.Columns[1].Name = "학습 구간";
            dgvTimeRecords.Columns[2].Name = "순공 시간"; dgvTimeRecords.Columns[3].Name = "성과";

            foreach (var item in logs.OrderByDescending(x => x.Value.sessionEnd))
            {
                var log = item.Value;
                if (!DateTime.TryParse(log.sessionEnd, out DateTime endTime)) continue;

                DateTime startTime = endTime.AddSeconds(-log.sessionDuration);
                string logDate = endTime.ToString("yyyy-MM-dd");

                if (logDate == todayStr) todayTotalSeconds += log.sessionDuration;
                if (endTime >= weekAgo) weeklyTotalSeconds += log.sessionDuration;
                if (log.sessionDuration > maxFocusSeconds) maxFocusSeconds = log.sessionDuration;
                if (dailyStudyMap.ContainsKey(logDate)) dailyStudyMap[logDate] += log.sessionDuration;

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
                TimeSpan tAvg = TimeSpan.FromSeconds(weeklyTotalSeconds / 7);
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
                series.ChartType = SeriesChartType.Bar;
                series.Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                series.IsValueShownAsLabel = true;

                // 🌟 핵심 추가: 막대그래프를 새로 만들 때 다크모드면 흰색, 아니면 검은색 지정!
                series.LabelForeColor = isDarkMode ? Color.White : Color.Black;

                foreach (var kvp in dailyStudyMap)
                {
                    string shortDate = kvp.Key.Substring(5);
                    double minutes = Math.Round(kvp.Value / 60.0, 1);
                    series.Points.AddXY(shortDate, minutes);
                }
            }
        }
        #endregion

        #region [ 5. 유틸리티 및 UI 꾸미기 ]
        // ⭐ GitHubManager 호출
        private void lblGitHubPush_Click(object sender, EventArgs e)
        {
            var (isSuccess, message, isInfo) = gitHubManager.PushToGitHub();
            MessageBox.Show(message, isInfo ? "알림" : (isSuccess ? "성공" : "오류"), MessageBoxButtons.OK, isInfo ? MessageBoxIcon.Information : (isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Error));
        }

        // ⭐ FileManager 호출
        private void btnExportWrongList_Click(object sender, EventArgs e)
        {
            var (isSuccess, message) = fileManager.ExportWrongListToMarkdown(dgvWrongList);
            MessageBox.Show(message, isSuccess ? "추출 성공" : "알림", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
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

        private void lblDarkMode_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            lblDarkMode.Text = isDarkMode ? "🌞 라이트 모드" : "🌙 다크 모드";
            ApplyTheme();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color formBgColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Rectangle bgRect = e.Bounds; bgRect.Inflate(2, 2);
            e.Graphics.FillRectangle(new SolidBrush(formBgColor), bgRect);
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color tabColor = isDarkMode ? (isSelected ? Color.FromArgb(60, 60, 65) : formBgColor) : (isSelected ? Color.White : SystemColors.Control);
            Color textColor = isDarkMode ? (isSelected ? Color.White : Color.Gray) : SystemColors.ControlText;

            Rectangle tabRect = e.Bounds; tabRect.Inflate(-1, -1);
            e.Graphics.FillRectangle(new SolidBrush(tabColor), tabRect);
            string tabText = tabControl1.TabPages[e.Index].Text;
            TextRenderer.DrawText(e.Graphics, tabText, e.Font, tabRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

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

            if (tabCoverPanel == null) { tabCoverPanel = new Panel(); this.Controls.Add(tabCoverPanel); }
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
                if (c is LinkLabel ll)
                {
                    ll.BackColor = Color.Transparent;
                    ll.LinkColor = text;             // 기본 링크 색상 (밝은 회색/흰색)
                    ll.VisitedLinkColor = text;      // 눌렀던 링크 색상
                    ll.ActiveLinkColor = accent;     // 누르는 순간의 색상
                }

                if (c is Label || c is CheckBox || c is RadioButton) { c.BackColor = Color.Transparent; c.ForeColor = text; }
                else { c.BackColor = bg; c.ForeColor = text; }

                if (c.Name == "lblStudyTime") { c.Font = new Font(c.Font.FontFamily, 24, FontStyle.Bold); c.ForeColor = accent; continue; }

                if (c is TextBox || c is ComboBox)
                {
                    if (isDarkMode) { c.BackColor = Color.FromArgb(60, 60, 65); c.ForeColor = Color.White; }
                    else { c.BackColor = box; c.ForeColor = text; }
                    if (c is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;

                    if (c is TextBox tb)
                    {
                        tb.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = box;
                    dgv.DefaultCellStyle.BackColor = box; dgv.DefaultCellStyle.ForeColor = text;
                    dgv.GridColor = gridLine; dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(50, 50, 50) : SystemColors.Control;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = text;
                }
                else if (c is Chart chart)
                {
                    chart.BackColor = bg;
                    if (chart.Titles.Count > 0) chart.Titles[0].ForeColor = text;
                    if (chart.Legends.Count > 0) { chart.Legends[0].BackColor = Color.Transparent; chart.Legends[0].ForeColor = text; }
                    foreach (var area in chart.ChartAreas)
                    {
                        area.BackColor = box;
                        area.AxisX.LabelStyle.ForeColor = text; area.AxisY.LabelStyle.ForeColor = text;
                        area.AxisX.MajorGrid.LineColor = gridLine; area.AxisY.MajorGrid.LineColor = gridLine;
                        area.AxisX.LineColor = gridLine; area.AxisY.LineColor = gridLine;
                    }
                    if (chart.Series.Count > 0)
                    {
                        chart.Series[0].BackGradientStyle = GradientStyle.None;

                        // 🌟 여기에 숫자(라벨) 색상 변경 코드를 딱 한 줄 추가합니다!
                        chart.Series[0].LabelForeColor = text;

                        if (chart.Name == "chartTimeHistory") chart.Series[0].Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                        else if (chart.Name == "chartAccuracy")
                        {
                            Color correctColor = isDarkMode ? Color.FromArgb(90, 130, 90) : Color.FromArgb(143, 188, 143);
                            Color wrongColor = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.FromArgb(224, 159, 150);
                            if (chart.Series[0].Points.Count >= 2) { chart.Series[0].Points[0].Color = correctColor; chart.Series[0].Points[1].Color = wrongColor; }
                            if (chart.Legends[0].CustomItems.Count >= 2) { chart.Legends[0].CustomItems[0].Color = correctColor; chart.Legends[0].CustomItems[1].Color = wrongColor; }
                        }
                        else
                        {
                            chart.Series[0].Color = isDarkMode ? Color.FromArgb(60, 110, 160) : Color.SteelBlue;
                            if (chart.Series.Count > 1) { chart.Series[1].BackGradientStyle = GradientStyle.None; chart.Series[1].Color = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.IndianRed; }
                        }
                    }
                }
                else if (c is GroupBox || c is Panel) { c.BackColor = bg; c.ForeColor = text; }
                else if (c is Button btn && c.Name != "btnGitHubPush")
                {
                    if (isDarkMode)
                    {
                        btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 90);
                        btn.BackColor = Color.FromArgb(60, 60, 65); btn.ForeColor = Color.White;
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 100, 130); btn.FlatAppearance.MouseDownBackColor = accent;
                    }
                    else { btn.FlatStyle = FlatStyle.Standard; btn.BackColor = SystemColors.Control; btn.ForeColor = SystemColors.ControlText; }
                }
                else if (c is StatusStrip statusStrip) { statusStrip.BackColor = isDarkMode ? Color.FromArgb(20, 20, 20) : SystemColors.Control; }

                if (c.HasChildren) ChangeControlColors(c, bg, text, box, gridLine, accent);
            }
        }
        #endregion

        #region [ 6. AI 번역 관련 메서드 ]
        private string GetDefaultTemplate(string lang)
        {
            switch (lang)
            {
                case "C#": return "using System;\n\nnamespace CodingTest\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            // 여기에 코드를 작성하세요\n            \n        }\n    }\n}";
                case "C++": return "#include <iostream>\nusing namespace std;\n\nint main() {\n    // 여기에 코드를 작성하세요\n    \n    return 0;\n}";
                case "Java": return "public class Main {\n    public static void main(String[] args) {\n        // 여기에 코드를 작성하세요\n        \n    }\n}";
                case "Python": return "";
                default: return "";
            }
        }

        private async void btnRetryTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastSourceCode) || string.IsNullOrEmpty(lastTargetLang))
            {
                MessageBox.Show("아직 번역된 코드가 없습니다.\n먼저 코드를 작성하고 언어를 변경해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("번역 결과가 이상한가요?\nAI에게 똑같은 원본 코드를 다른 방식(확률)으로 다시 번역하라고 지시합니다.\n\n(※ 기존 번역 결과는 지워집니다.)", "다시 번역 (Retry)", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                await RequestTranslation(lastSourceCode, lastSourceLang, lastTargetLang);
            }
        }

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
                if (normCurrent != normTemplate) isUserCode = true;
            }

            if (isUserCode)
            {
                DialogResult result = MessageBox.Show($"작성 중인 {previousLang} 코드가 있습니다.\n선택하신 {selectedLang}(으)로 🤖AI 자동 번역하시겠습니까?\n\n(예: AI 번역 유지 / 아니요: 초기화 / 취소: 변경 취소)", "AI 코드 자동 번역", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel) { RevertComboBox(); return; }
                else if (result == DialogResult.Yes)
                {
                    lastSourceCode = currentCode; lastSourceLang = previousLang; lastTargetLang = selectedLang;
                    bool isSuccess = await RequestTranslation(currentCode, previousLang, selectedLang);
                    if (isSuccess) previousLang = selectedLang;
                    else RevertComboBox();
                    return;
                }
            }

            txtCode.Text = GetDefaultTemplate(selectedLang);
            previousLang = selectedLang;
        }

        // ⭐ GeminiService 호출 전용 래퍼 메서드
        private async Task<bool> RequestTranslation(string code, string sourceLang, string targetLang)
        {
            cbLanguage.Enabled = false;
            string originalText = txtCode.Text;

            // UI 업데이트용 델리게이트를 GeminiService에 넘겨줌
            var (isSuccess, resultText) = await geminiService.TranslateCodeAsync(code, sourceLang, targetLang, msg =>
            {
                txtCode.Text = msg;
                txtCode.Refresh(); // 화면 즉시 갱신
            });

            cbLanguage.Enabled = true;

            if (isSuccess)
            {
                txtCode.Text = resultText;
                return true;
            }
            else
            {
                MessageBox.Show(resultText, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCode.Text = originalText;
                return false;
            }
        }

        private void RevertComboBox()
        {
            cbLanguage.SelectedIndexChanged -= cbLanguage_SelectedIndexChanged;
            cbLanguage.SelectedItem = previousLang;
            cbLanguage.SelectedIndexChanged += cbLanguage_SelectedIndexChanged;
        }
        #endregion

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

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sessionManager.CurrentStatus != "Break")
            {
                e.Cancel = true;
                learningTimer.Stop();
                sessionManager.StopSession();

                var sessionData = new SessionData
                {
                    status = "Break",
                    sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    sessionDuration = sessionManager.GetTotalSeconds(),
                    lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebaseManager.SaveSessionAsync(sessionData);
                await firebaseManager.PushSessionLogAsync(sessionData);

                this.FormClosing -= Form1_FormClosing;
                this.Close();
            }
        }
    }
}