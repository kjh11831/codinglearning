// 프로젝트 내부 매니저, 모델, 서비스 등 필요한 네임스페이스 선언
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
// 비동기 작업 처리 및 윈도우 폼 컨트롤 관련 네임스페이스 선언
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.LinkLabel;

namespace codinglearning
{
    public partial class Form1 : Form
    {
        // 학습 세션 시간을 관리하는 매니저
        private LearningSessionManager sessionManager;
        // GitHub, 파일 시스템, Firebase, API 통신, Gemini AI 번역 기능을 각각 담당하는 매니저 및 서비스 변수들
        private GitHubManager gitHubManager;
        private FileManager fileManager;
        private FirebaseManager firebaseManager;
        private ApiService apiService;
        private GeminiService geminiService;

        // Codeforces 웹 화면을 띄우거나 백그라운드 자동 제출을 처리할 WebView2 컨트롤
        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCF;
        // 현재 선택된 문제의 ID, 제목, 난이도, 태그 정보를 임시 저장할 문자열 변수들
        private string selId = "", selTitle = "", selDiff = "", selTags = "";
        // 현재 문제 검색이 진행 중인지 여부를 나타내는 플래그
        private bool isSearching = false;
        // 현재 예제 코드가 실행 중인지 여부를 나타내는 플래그
        private bool isRunningSample = false;

        // AI 번역 기능에서 이전 언어 상태 및 코드를 기억하기 위한 변수들
        private string previousLang = "C#";
        private string lastSourceCode = "";
        private string lastSourceLang = "";
        // AI가 번역할 목표 언어를 저장하는 변수
        private string lastTargetLang = "";

        // UI 다크 모드 처리를 위한 패널 및 모드 상태 변수
        private Panel tabCoverPanel;
        private bool isDarkMode = false;

        // 폼 초기화 생성자
        public Form1()
        {
            InitializeComponent();
            // 각 매니저와 서비스 객체들을 생성하여 초기화
            sessionManager = new LearningSessionManager();
            gitHubManager = new GitHubManager();
            fileManager = new FileManager();
            firebaseManager = new FirebaseManager();
            apiService = new ApiService();
            // Gemini 기반 번역 서비스 초기화
            geminiService = new GeminiService();
        }

        // WebView2에서 특정 페이지로 네비게이션이 완료되었을 때 실행되는 이벤트 핸들러
        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                // WebView가 초기화되지 않았으면 이벤트 처리 중단
                if (webViewCF == null || webViewCF.CoreWebView2 == null || webViewCF.Source == null) return;
                // 현재 열린 페이지의 URL 가져오기
                string currentUrl = webViewCF.Source.ToString();

                // 다크모드 활성화 여부에 따라 웹 페이지 화면의 색상을 반전시키는 CSS 스크립트 실행
                string darkModeCss = isDarkMode ? "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "document.documentElement.style.filter = 'none';";
                await webViewCF.CoreWebView2.ExecuteScriptAsync(darkModeCss);
                // 스크립트 적용 후 잠시 대기
                await Task.Delay(500);

                // Codeforces 로그인 페이지에 진입한 경우
                if (currentUrl.Contains("codeforces.com/enter"))
                {
                    if (webViewCF.Parent == this)
                    {
                        // 사용자 로그인을 위한 새로운 팝업 폼 생성
                        Form loginForm = new Form();
                        // 팝업 폼 제목 및 크기, 위치 설정
                        loginForm.Text = "코드포스 계정 로그인";
                        loginForm.Size = new Size(800, 700);
                        loginForm.StartPosition = FormStartPosition.CenterParent;

                        // WebView를 부모 폼에서 분리하여 로그인 폼에 부착 후 화면에 표시
                        webViewCF.Visible = true;
                        loginForm.Controls.Add(webViewCF);
                        // 로그인 폼이 닫힐 때 발생하는 이벤트 (WebView 반환 처리)
                        loginForm.FormClosing += (s, ev) =>
                        {
                            // 로그인 폼에서 WebView 제거
                            loginForm.Controls.Remove(webViewCF);
                            // 다시 원래 메인 폼으로 WebView를 가져온 뒤 숨김 처리 (백그라운드용)
                            this.Controls.Add(webViewCF);
                            webViewCF.Visible = false;
                        };

                        // 로그인 안내 메시지 출력 후 로그인 모달 창 열기
                        MessageBox.Show("자동 제출을 위해 로그인이 필요합니다.\n팝업창이 열리면 로그인을 완료해 주세요!", "로그인 안내");
                        loginForm.ShowDialog(this);
                    // 로그인 페이지 처리 완료
                    }
                }
                // 로그인에 성공하여 설정 페이지나 메인 프로필 화면으로 넘어갔을 경우
                else if (currentUrl.Contains("codeforces.com/settings") || currentUrl.Contains("codeforces.com/profile") || currentUrl == "https://codeforces.com/")
                {
                    // 로그인 팝업 폼에서 호출된 상태라면
                    if (webViewCF.Parent is Form parentForm && parentForm != this)
                    // 해당 팝업 창을 닫아버림
                    {
                        parentForm.Close();
                        // 로그인 성공 알림 메시지 출력
                        MessageBox.Show("✅ 로그인 성공!\n이제 창을 띄우지 않고 백그라운드에서 자동 제출이 가능합니다.", "알림");
                    // 처리 끝
                    }
                }
            }
            catch
            {
                // 웹뷰 처리 도중 발생하는 자잘한 에러는 프로그램 진행을 위해 무시
            }
        }

        #region [ 1. 공통 및 하단 상태 표시줄 ]
        // 하단 상태 표시줄 및 전체 공통 초기 설정을 담당하는 영역
        private async void Form1_Load(object sender, EventArgs e)
        {
            // 앱 로드 시 파이어베이스 연결 시도, 실패 시 경고창 띄우고 중단
            if (!firebaseManager.Initialize())
            {
                MessageBox.Show("Firebase 연결 실패. 인터넷 상태를 확인하세요.");
                // 로드 중단
                return;
            }

            // 언어 선택 콤보박스에 지원하는 프로그래밍 언어 목록 추가
            cbLanguage.Items.AddRange(new string[] { "C#", "C++", "Python", "Java" });
            // 기본 선택 값을 첫 번째 언어인 'C#'으로 설정
            cbLanguage.SelectedIndex = 0;

            // 통계 차트의 기본 시리즈 이름 설정
            if (chartAccuracy != null && chartAccuracy.Series.Count > 0)
            {
                chartAccuracy.Series[0].Name = "풀이 통계";
                // 차트 초기화 블록 끝
            }

            // WebView2 객체를 생성하고 기본 설정 지정
            webViewCF = new Microsoft.Web.WebView2.WinForms.WebView2();
            webViewCF.Visible = false;
            // 초기엔 사용자 눈에 보이지 않도록 백그라운드로 숨겨둠
            // 컨트롤 크기를 부모 영역에 맞춤
            webViewCF.Dock = DockStyle.Fill;

            // 메인 폼에 생성한 WebView 컨트롤 추가
            this.Controls.Add(webViewCF);

            // WebView 비동기 초기화 호출
            await InitializeWebViewAsync();

            // 기본 UI 스타일 적용 및 학습 세션 측정 시작
            ApplyMinimalStyle();
            StartLearningSession();

            // 다크모드 전환 라벨의 부모를 메인 폼으로 설정하고 우측 상단으로 위치 지정
            lblDarkMode.Parent = this;
            lblDarkMode.Location = new Point(tabControl1.Width - lblDarkMode.Width - 15, 3);
            lblDarkMode.BringToFront();
            lblDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            // 배경 투명화 및 탭 컨트롤을 직접 그리기 모드로 설정
            lblDarkMode.BackColor = Color.Transparent;
            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;

            // 현재 설정된 테마(다크/라이트)를 적용하고, 폼 종료 이벤트 등록
            ApplyTheme();
            this.FormClosing += Form1_FormClosing;
        // 폼 로드 메서드 종료
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // 웹뷰의 캐시 및 유저 데이터가 저장될 로컬 폴더 경로 설정
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CodingLearning_WebView");
                // 만약 해당 경로가 없으면
                if (!System.IO.Directory.Exists(userDataFolder))
                {
                    // 폴더를 새로 생성
                    System.IO.Directory.CreateDirectory(userDataFolder);
                    // 조건문 종료
                }

                // WebView2 구동 환경(환경변수 및 캐시 폴더 포함)을 비동기로 생성
                var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                // 만들어진 환경을 바탕으로 브라우저 엔진 확실히 초기화
                await webViewCF.EnsureCoreWebView2Async(environment);

                // 네비게이션 완료 시 앞서 만든 이벤트가 호출되도록 연결하고, 초기 설정 확인을 위해 일반 페이지 로드 시도
                webViewCF.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webViewCF.CoreWebView2.Navigate("https://codeforces.com/settings/general");
            }
            catch (Exception ex)
            {
                // 초기화 도중 에러가 나면 원인을 메시지 박스로 출력
                MessageBox.Show($"WebView2 초기화 실패!\n\n에러 메시지: {ex.Message}", "초기화 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // 에러 캐치 블록 끝
            }
        }

        // 주기적으로 세션 시간을 체크하는 타이머의 틱 이벤트 (1초마다 발생)
        private void learningTimer_Tick(object sender, EventArgs e)
        {
            // 현재 상태가 활성('Active')인 경우
            if (sessionManager.CurrentStatus == "Active")
            {
                // 진행된 시간을 UI(라벨)에 시:분:초 포맷으로 표시
                lblTimer.Text = sessionManager.GetCurrentDuration().ToString(@"hh\:mm\:ss");
                // 키보드/마우스 입력이 오랫동안 없었는지 판단하여 Idle 상태 갱신
                sessionManager.UpdateIdleState();
            }
            // 업데이트된 상태에 맞추어 UI 색상이나 텍스트 변경 적용
            UpdateStatusUI();
        // 틱 이벤트 블록 끝
        }

        private void UpdateStatusUI()
        {
            // 테마(다크/라이트)에 맞춰 상태별(활성, 유휴, 휴식) 색상을 다르게 지정
            // 활성 상태 색상 정의
            Color activeColor = isDarkMode ? Color.LimeGreen : Color.Green;
            Color idleColor = isDarkMode ? Color.Gold : Color.Orange;
            Color breakColor = isDarkMode ? Color.LightCoral : Color.Crimson;
            // 현재 활성 상태일 때 UI 업데이트
            if (sessionManager.CurrentStatus == "Active")
            {
                // 학습 중임을 텍스트로 나타냄
                lblStatus.Text = "▶ 학습 중";
                // 색상을 활성 색상(초록 계열)으로 적용하고, 버튼 텍스트를 종료로 변경
                lblStatus.ForeColor = activeColor;
                lblTimer.ForeColor = activeColor;
                btnStopSession.Text = "⏹ 세션 종료";
                // 조건 분기 끝
            }
            // 현재 아무 동작이 없는 유휴(Idle) 상태일 때
            else if (sessionManager.CurrentStatus == "Idle")
            {
                // 잠깐 쉬는 중임을 표시
                lblStatus.Text = "Ⅱ 잠깐 쉬는 중";
                // 색상을 유휴 색상(주황/노랑 계열)으로 지정
                lblStatus.ForeColor = idleColor;
                lblTimer.ForeColor = idleColor;
                btnStopSession.Text = "⏹ 세션 종료";
                // 조건 분기 끝
            }
            // 아예 정지된 휴식(Break) 상태일 때
            else
            {
                // 명시적으로 휴식 중임을 표시
                lblStatus.Text = "■ 휴식 중";
                // 색상을 붉은 계열로 지정하고, 버튼을 다시 시작 모드로 변경
                lblStatus.ForeColor = breakColor;
                lblTimer.ForeColor = breakColor;
                btnStopSession.Text = "▶ 세션 시작";
                // 상태 업데이트 분기 종료
            }
        }

        // 새로운 학습 세션을 시작하는 메서드
        private void StartLearningSession()
        {
            // 매니저를 통해 세션 기록 시작 처리
            sessionManager.StartSession();
            // 시간 측정을 위한 UI 타이머 가동
            learningTimer.Start();
            UpdateStatusUI();
        }

        // 세션 시작/종료 버튼 클릭 시 발생하는 이벤트
        private async void btnStopSession_Click(object sender, EventArgs e)
        {
            // 만약 현재 휴식 상태라면, 새로 세션을 시작하고 클릭 처리 종료
            if (sessionManager.CurrentStatus == "Break")
            {
                StartLearningSession();
                // 그대로 리턴하여 아래 로직 실행 방지
                return;
            }

            // 활성 상태에서 종료를 누른 것이므로 타이머 정지 및 세션 매니저 기록 마감
            learningTimer.Stop();
            sessionManager.StopSession();
            UpdateStatusUI();

            // 총 누적 진행 시간을 가져옴
            int duration = sessionManager.GetTotalSeconds();
            // Firebase에 저장할 세션 데이터 구조체(모델) 생성 및 데이터 바인딩
            var sessionData = new SessionData
            {
                status = "Break",
                sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                sessionDuration = duration,
                lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")

                // 모델 생성 종료
            };

            // DB에 해당 세션 데이터를 저장하고 로그에 푸시 (비동기)
            await firebaseManager.SaveSessionAsync(sessionData);
            await firebaseManager.PushSessionLogAsync(sessionData);

            // 출력용 포맷팅을 위해 총 소요 시간을 시/분/초 문자열로 변환
            duration = sessionManager.GetTotalSeconds();
            TimeSpan ts = TimeSpan.FromSeconds(duration);
            string formattedTime = "";
            // 시간 단위가 있으면 문자열에 추가
            if (ts.Hours > 0) formattedTime += $"{ts.Hours}시간 ";
            if (ts.Minutes > 0) formattedTime += $"{ts.Minutes}분 ";
            formattedTime += $"{ts.Seconds}초";
            // 사용자에게 이번 세션 동안 공부한 총 시간을 팝업으로 안내
            MessageBox.Show($"학습 세션이 종료되었습니다!\n🔥 이번 세션 학습 시간: {formattedTime}", "세션 종료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        // 세션 버튼 클릭 처리 이벤트 종료
        }
        #endregion

        #region [ 2. 탭 1: 문제 탐색 ]
        // 키워드와 난이도 범위 텍스트박스 값을 읽어와 문제 검색 수행
        private async void btnSearch_Click(object sender, EventArgs e) => await PerformSearch(txtKeyword.Text, txtMinDifficulty.Text, txtMaxDifficulty.Text);
        // 아무 조건 없이 전체 문제를 탐색(상위 50개 등)
        private async void btnSearchAll_Click(object sender, EventArgs e) => await PerformSearch("", "", "");
        // 사용자가 검색 조건과 결과 목록을 초기화하고자 할 때 호출
        private void btnResetSearch_Click(object sender, EventArgs e)
        {
            // 사용자가 클릭했음을 세션 매니저에 알려 Idle 상태가 되는 것을 방지
            sessionManager.RecordUserAction();
            // 입력 텍스트 박스들과 데이터그리드뷰(표) 목록을 싹 비움
            txtKeyword.Text = ""; txtMinDifficulty.Text = ""; txtMaxDifficulty.Text = "";
            dgvProblems.Rows.Clear();
        // 초기화 메서드 종료
        }

        // 실제로 Codeforces API를 호출하고 필터링해 UI에 뿌려주는 메인 검색 비동기 함수
        private async Task PerformSearch(string keyword, string minDiffStr, string maxDiffStr)
        {
            // 이미 검색 중이라면 중복 호출 방지를 위해 즉시 리턴
            if (isSearching) return;
            // 검색 상태 활성화 및 유저 액션 기록
            isSearching = true;
            sessionManager.RecordUserAction();

            // 검색 버튼 텍스트를 로딩 상태로 변경하고 다크모드 여부에 따라 색상 조절 및 새로고침
            btnSearch.Text = "검색 중...";
            btnSearch.ForeColor = isDarkMode ? Color.White : SystemColors.ControlText;
            btnSearch.Refresh();
            // 외부 API 통신이므로 예외 처리를 위한 try 블록 시작
            try
            {
                // API Service를 통해 문제 목록 전체 JSON 배열을 받아옴
                JArray problems = await apiService.FetchCodeforcesProblemsAsync();
                // 데이터가 제대로 반환되었다면 표 그리기 시작
                if (problems != null)
                {
                    // 표의 모든 기존 행 삭제
                    dgvProblems.Rows.Clear();
                    // 총 5개의 열(칼럼) 생성 및 각각의 이름 지정 (번호, 제목, 난이도, 태그, 결과)
                    dgvProblems.ColumnCount = 5;
                    dgvProblems.Columns[0].Name = "번호"; dgvProblems.Columns[1].Name = "제목";
                    dgvProblems.Columns[2].Name = "난이도"; dgvProblems.Columns[3].Name = "태그"; dgvProblems.Columns[4].Name = "결과";
                    // 대소문자 구분 없이 검색하기 위해 입력 키워드를 모두 소문자로 치환
                    keyword = keyword.ToLower();

                    // 최소/최대 난이도 입력이 비어있으면 기본값(0~3500)으로 설정하고, 입력되었다면 숫자로 파싱
                    int minDiff = string.IsNullOrEmpty(minDiffStr) ? 0 : int.Parse(minDiffStr);
                    int maxDiff = string.IsNullOrEmpty(maxDiffStr) ? 3500 : int.Parse(maxDiffStr);
                    // 검색된 목록의 개수를 세기 위한 카운터 변수
                    int count = 0;

                    // 전체 JSON 데이터를 반복하며 필터 조건에 맞는지 확인
                    foreach (var p in problems)
                    {
                        // 최대 50개까지만 표에 추가하여 UI 렉 방지
                        if (count >= 50) break;
                        // 각 문제의 제목, 태그, 난이도 속성값을 추출 (null 처리 포함)
                        string pTitle = p["name"]?.ToString();
                        string pTags = string.Join(", ", p["tags"]);
                        int pRating = p["rating"] != null ? (int)p["rating"] : 0;
                        // 난이도가 설정된 최소/최대 범위 안에 들어오는지 필터링
                        if (pRating >= minDiff && (pRating <= maxDiff || maxDiff == 0))
                        {
                            // 키워드가 없거나, 제목 또는 태그에 검색어가 포함되어 있는지 확인
                            if (string.IsNullOrEmpty(keyword) || pTitle.ToLower().Contains(keyword) || pTags.ToLower().Contains(keyword))
                            {
                                // 조건에 모두 부합하면 데이터그리드뷰에 행 추가. 번호는 콘테스트ID+인덱스로 구성
                                dgvProblems.Rows.Add($"{p["contestId"]}{p["index"]}", pTitle, pRating, pTags, "-");
                                // UI에 뿌려진 카운트 증가
                                count++;
                            }
                        }
                    }
                }
            }
            // 난이도 칸에 숫자가 아닌 문자를 적어 int.Parse()에서 예외가 발생한 경우
            catch (FormatException)
            {
                // 오류 메시지 팝업
                MessageBox.Show("난이도 입력 칸에는 반드시 숫자만 입력해주세요.", "입력 형식 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // 형식 예외 캐치 끝
            }
            // 기타 네트워크 에러 및 API 파싱 관련 일반 예외 처리
            catch (Exception ex)
            {
                // 실패 원인을 알려주는 메시지 출력
                MessageBox.Show($"UI를 그리는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // 에러 캐치 끝
            }
            // 에러가 났든 성공했든 함수 종료 전 무조건 실행되는 블록
            finally
            {
                // 검색 중 상태 해제 및 버튼 원래 텍스트 복구
                isSearching = false;
                // 버튼 텍스트 원상복구
                btnSearch.Text = "검색";
            }
        }

        // 문제 목록 표의 특정 셀(행)을 클릭했을 때 발생하는 이벤트
        private void dgvProblems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 활동 상태 기록
            sessionManager.RecordUserAction();
            // 유효한 행을 클릭했을 경우 (헤더가 아닌 곳)
            if (e.RowIndex >= 0)
            {
                // 해당 인덱스의 행 데이터 전체를 가져옴
                var row = dgvProblems.Rows[e.RowIndex];
                // 각 전역 변수(selId 등)와 코드 작성 탭의 라벨에 선택한 문제 정보들을 일괄 바인딩
                selId = lblSelProbNum.Text = lblCodeProbNum.Text = row.Cells[0].Value?.ToString();
                selTitle = lblSelProbTitle.Text = lblCodeProbTitle.Text = row.Cells[1].Value?.ToString();
                // 난이도 텍스트 지정
                selDiff = lblSelProbDiff.Text = lblCodeProbDiff.Text = row.Cells[2].Value?.ToString();
                // 태그 텍스트 지정
                selTags = lblSelProbTags.Text = lblCodeProbTags.Text = row.Cells[3].Value?.ToString();
            // 인덱스 체크 조건 종료
            }
        }

        // [문제 보기] 버튼을 클릭해 실제 Codeforces 문제 페이지를 모달창으로 열어보는 이벤트
        private void btnViewProblem_Click(object sender, EventArgs e)
        {
            // 액션 기록
            sessionManager.RecordUserAction();
            // 선택된 문제가 정상적으로 변수에 담겨 있는지 검증
            if (!string.IsNullOrEmpty(selId))
            {
                // 문제 번호 중 숫자 부분(Contest ID) 추출
                string contestId = new String(selId.Where(Char.IsDigit).ToArray());
                // 문제 번호 중 알파벳 부분(Problem Index, 예: A, B1 등) 추출
                string index = new String(selId.Where(Char.IsLetter).ToArray());
                // 조합하여 문제 원본 웹사이트 주소 생성
                string url = $"https://codeforces.com/problemset/problem/{contestId}/{index}";

                // 띄울 창(Form) 인스턴스화
                Form probForm = new Form();
                // 폼의 캡션 바에 문제 번호와 제목 표시
                probForm.Text = $"[문제 보기] {selId} - {selTitle}";
                probForm.Size = new Size(1000, 800);
                probForm.StartPosition = FormStartPosition.CenterScreen;

                // 새 창에 넣을 독립적인 일회용 WebView2 컨트롤 생성
                var webView = new Microsoft.Web.WebView2.WinForms.WebView2();
                // 창에 꽉 차게 도킹하고 부모 폼의 컨트롤로 등록
                webView.Dock = DockStyle.Fill;
                probForm.Controls.Add(webView);

                // 창이 닫힐 때 자원을 낭비하지 않도록 WebView를 즉시 폐기 처리
                probForm.FormClosed += (s, ev) => { webView.Dispose(); };

                // 화면에 창을 띄움
                probForm.Show();

                // 웹뷰 환경 셋업 후 방금 만든 주소로 로드 시작하는 유틸 메서드 호출
                InitializeAndNavigate(webView, url);
            // 조건 블록 종료 (문제 선택된 경우)
            }
            else
            {
                // 문제를 선택하지 않고 버튼을 누르면 경고
                MessageBox.Show("목록에서 문제를 먼저 선택해주세요.");
            // 조건 블록 완전 종료
            }
        }

        // 일회용 WebView 객체 초기화 및 타겟 URL로 이동, 그리고 다크모드 적용까지 해주는 비동기 헬퍼 함수
        private async void InitializeAndNavigate(Microsoft.Web.WebView2.WinForms.WebView2 wv, string url)
        {
            // 엔진 초기화 보장
            await wv.EnsureCoreWebView2Async(null);
            // 로드가 완료되었을 때 실행할 JS 코드(다크모드 인버트 등) 설정
            wv.CoreWebView2.NavigationCompleted += (s, e) => {
                string darkModeCss = isDarkMode ?
                // 다크모드 시 색상 반전 CSS 스크립트 문자열 준비
                "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "";
                // 스크립트 삽입 실행
                wv.CoreWebView2.ExecuteScriptAsync(darkModeCss);
            };

            // 설정이 다 끝나면 해당 url로 진짜 이동 명령
            wv.CoreWebView2.Navigate(url);
        }
        #endregion

        #region [ 3. 탭 2: 코드 작성 및 채점 ]
        // 코드 에디터 입력 박스의 텍스트가 변경될 때마다 사용자가 활동 중임을 세션 매니저에 기록
        private void txtCode_TextChanged(object sender, EventArgs e) => sessionManager.RecordUserAction();
        // 코드 초기화 버튼 클릭 시 입력 창과 결과 창 모두 텍스트 비우기
        private void btnResetCode_Click(object sender, EventArgs e) { sessionManager.RecordUserAction(); txtCode.Text = ""; txtResult.Text = "";
        // 초기화 메서드 종료
        }

        // 사용자가 작성한 코드를 서버에서 실행하거나 예제 데이터를 가져와 자동으로 테스트해 보는 버튼 클릭 이벤트
        private async void btnRunSample_Click(object sender, EventArgs e)
        {
            // 이미 실행 중복 요청이 진행 중이면 무시
            if (isRunningSample) return;
            // 작성된 코드가 없으면 경고 후 종료
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("실행할 코드를 먼저 작성해주세요!");
                // 처리 중단
                return;
            }

            // 실행 중 플래그 활성화
            isRunningSample = true;
            // API 에러를 잡기 위한 try 블록
            try
            {
                // 현재 콤보박스에서 선택된 언어 이름 추출
                string currentLang = cbLanguage.SelectedItem.ToString();

                // UI 갱신으로 사용자에게 처리 중임을 알림
                btnRunSample.Text = "코드 실행 중...";
                txtResult.Text = "⏳ 코드를 컴파일하고 실행하는 중입니다...\r\n\r\n";

                // 만약 현재 선택된 문제(selId)가 없다면 예제를 가져올 수 없으므로 '단순 실행'만 진행
                if (string.IsNullOrEmpty(selId) || selId == "나야 번호" || selId == "-")
                {
                    txtResult.Text = "⏳ (선택된 문제가 없어 단순 코드 실행만 진행합니다...)\r\n\r\n";
                    // ApiService를 호출해 코드를 넘기고 실행 결과(표준 출력, 에러 등)를 받음
                    string rawOutput = await apiService.RunCodeOnlyAsync(txtCode.Text, currentLang);

                    txtResult.AppendText("==================== [단순 실행 결과] ====================\r\n");
                    txtResult.AppendText(rawOutput);
                    txtResult.AppendText("\r\n=====================================================");
                }
                // 선택된 문제가 있다면 Codeforces에서 예제를 긁어와 '자동 채점 테스트' 진행
                else
                {
                    txtResult.Text = $"⏳ [{selId}] 문제의 예제 입출력 데이터를 가져오는 중입니다...\r\n\r\n";

                    // ApiService의 유령이었던 'FetchSampleDataFromWebAsync'를 호출해 입력/출력 쌍을 리스트로 받아옴
                    var (inputs, outputs, errorMsg) = await apiService.FetchSampleDataFromWebAsync(selId);

                    // 만약 크롤링에 실패했거나 예제를 찾지 못했다면
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        txtResult.AppendText($"[예제 데이터 가져오기 실패]\r\n{errorMsg}\r\n\r\n");
                        txtResult.AppendText("⚠️ 문제가 발생하여 기존의 '단순 코드 실행' 모드로 전환합니다...\r\n\r\n");

                        // 예제 테스트를 포기하고 단순 실행으로 Fallback(대체) 처리
                        string rawOutput = await apiService.RunCodeOnlyAsync(txtCode.Text, currentLang);
                        txtResult.AppendText("==================== [단순 실행 결과] ====================\r\n");
                        txtResult.AppendText(rawOutput);
                        txtResult.AppendText("\r\n=====================================================");
                    }
                    // 성공적으로 예제 쌍을 가져왔다면 본격적인 테스트 시작
                    else
                    {
                        txtResult.Text = $"✅ 총 {inputs.Count}개의 예제를 찾았습니다. 자동 테스트를 시작합니다...\r\n\r\n";

                        // 모든 예제를 통과했는지 확인하기 위한 플래그
                        bool allPassed = true;

                        // 가져온 예제의 개수만큼 반복문을 돌면서 하나씩 Judge0로 채점
                        for (int i = 0; i < inputs.Count; i++)
                        {
                            txtResult.AppendText($"==================== [예제 {i + 1}] ====================\r\n");

                            // 'RunJudge0Async' 호출. 코드, 언어, 예제입력값, 예상출력값을 모두 던져서 비교 판별함
                            var (isCorrect, msg) = await apiService.RunJudge0Async(txtCode.Text, currentLang, inputs[i], outputs[i]);

                            // 각 예제별 결과를 UI 결과창에 덧붙여 출력
                            txtResult.AppendText(msg + "\r\n\r\n");

                            // 하나라도 틀렸다면 올패스 플래그를 false로 변경
                            if (!isCorrect) allPassed = false;
                        }

                        txtResult.AppendText("=====================================================\r\n");

                        // 모든 예제를 정답 처리받았을 경우 격려 메시지
                        if (allPassed)
                        {
                            txtResult.AppendText($"🎉 완벽합니다! 모든 예제 테스트를 통과했습니다. 이제 [CF 제출]을 눌러 실제 서버에 채점을 받아보세요!\r\n");
                            MessageBox.Show("🎉 완벽합니다! 모든 예제 테스트를 통과했습니다.\n이제 [CF 제출]을 눌러 실제 서버에 채점을 받아보세요!", "테스트 통과", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        // 하나라도 틀렸을 경우 수정 권고 메시지
                        else
                        {
                            txtResult.AppendText($"⚠️ 오답이 발생한 예제가 있습니다. 코드의 로직이나 출력 형식을 다시 한번 확인해 주세요.\r\n");
                            MessageBox.Show("⚠️ 오답이 발생한 예제가 있습니다.\n결과창을 보고 코드의 로직이나 출력 형식을 다시 한번 확인해 주세요.", "테스트 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 실행에 실패하거나 기타 예외 발생 시 결과창에 에러 메시지 텍스트 표기
                txtResult.Text = $"오류 발생: {ex.Message}";
                // 예외 블록 종료
            }
            finally
            {
                // 모든 처리가 끝났으므로 진행 중 플래그를 끄고 버튼 상태를 원상 복구
                isRunningSample = false;
                // 원래 문구
                btnRunSample.Text = "예제 테스트 실행";
            }
        }

        // Codeforces 웹사이트에 작성된 코드를 실제로 "제출"하는 버튼 클릭 이벤트
        private async void btnSubmitCF_Click(object sender, EventArgs e)
        {
            // 중복 제출 방지를 위해 버튼 텍스트가 초기 상태가 아니면 무시
            if (btnSubmitCF.Text != "CF 제출") return;
            // 액션 기록
            sessionManager.RecordUserAction();

            // 문제가 선택되지 않은 상태에서 제출을 막음
            if (string.IsNullOrEmpty(selId))
            {
                MessageBox.Show("문제를 먼저 선택해주세요.");
                // 제출 로직 종료
                return;
            }
            // 코드 내용이 비어있으면 제출을 막음
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("제출할 코드가 없습니다!");
                // 제출 로직 종료
                return;
            }

            // 현재 선택된 언어 가져오기
            string currentLang = cbLanguage.SelectedItem.ToString();
            // 앱에 정의된 Codeforces 시스템 상의 언어 ID 매핑 딕셔너리를 조회해 실제 전송할 ID값을 가져옴
            if (!codinglearning.Models.AppConstants.CFLangIds.TryGetValue(currentLang, out string cfLangId))
            {
                // 매핑 실패 시 지원하지 않는 언어 오류 안내
                MessageBox.Show($"지원하지 않는 언어입니다: {currentLang}");
                // 텍스트 원래대로 복구 후 종료
                btnSubmitCF.Text = "CF 제출";
                return;
            }

            // 제출할 웹페이지 URL 조립을 위해 ID에서 콘테스트 숫자 분리
            string contestId = new String(selId.Where(Char.IsDigit).ToArray());
            // 인덱스 문자 부분 분리
            string index = new String(selId.Where(Char.IsLetter).ToArray());

            // Codeforces 제출 폼 URL 조립 (예: contest/1234/submit)
            string submitUrl = $"https://codeforces.com/contest/{contestId}/submit";

            // 처리 과정 UI 안내
            btnSubmitCF.Text = "제출 창 여는 중...";
            // 로직 에러 대비 블록 시작
            try
            {
                // 사용자가 제출 과정을 직접 볼 수 있도록 백그라운드의 WebView를 담을 창 생성
                Form resultForm = new Form();
                // 팝업 캡션 설정
                resultForm.Text = "제출 내역 확인 (Codeforces)";
                resultForm.Size = new Size(1000, 700);
                resultForm.StartPosition = FormStartPosition.CenterParent;

                // 기존 백그라운드용 WebViewCF를 보이게 한 후 팝업 폼에 부착
                webViewCF.Visible = true;
                resultForm.Controls.Add(webViewCF);
                // WebView 네비게이션이 완료되었을 때 코드를 자동 삽입할 이벤트 핸들러 선언
                EventHandler<CoreWebView2NavigationCompletedEventArgs> autoFillHandler = null;
                autoFillHandler = async (s, args) =>
                {
                    // 페이지 이동 1회 완료 시 이벤트 핸들러 해제 (중복 주입 방지)
                    webViewCF.CoreWebView2.NavigationCompleted -= autoFillHandler;
                    // 페이지 렌더링을 기다리기 위해 1.5초 대기
                    await Task.Delay(1500);

                    // 창이 닫혀버렸다면 스크립트 실행 중지
                    if (!resultForm.Visible) return;

                    // JS로 넘기기 전 문자열 내부 특수기호(\, `, $)들을 이스케이프 처리하여 문법 에러 방지
                    string safeCode = txtCode.Text.Replace("\\", "\\\\").Replace("`", "\\`").Replace("$", "\\$");

                    // 다크모드에 맞춰 스크립트 내부에서도 인버트 처리 문자열 생성
                    string darkModeCss = isDarkMode ?
                    // 필터 문자열
                    "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "";

                    // 웹 화면에 삽입할 자바스크립트 코드 덩어리 작성
                    string script = $@"
            (function() {{
                {darkModeCss} // 화면 진입 즉시 다크 모드 필터 적용

                // DOM 요소의 값을 바꾼 뒤 강제로 change 이벤트를 발생시켜 사이트 자체 스크립트가 인지하도록 하는 헬퍼 함수
                function triggerChange(el) {{
                    if(el) {{

                        // 변경 이벤트 디스패치
                        el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }}

                // Codeforces의 문제 번호 선택 콤보박스(select) DOM 찾기
                var probSelect = document.querySelector('select[name=""submittedProblemIndex""]');
                if(probSelect) {{ 

                    // 값을 현재 인덱스로 설정하고 체인지 이벤트 발생
                    probSelect.value = '{index}'; 
                    triggerChange(probSelect); 
                }}

                // 제출 언어를 고르는 콤보박스 DOM 찾기
                var langSelect = document.querySelector('select[name=""programTypeId""]');
                if(langSelect) {{ 

                    // C#, Java 등 앞서 얻은 Codeforces 전용 언어 ID로 값 변경 후 이벤트 트리거
                    langSelect.value = '{cfLangId}'; 
                    triggerChange(langSelect); 
                }}

                // 🌟 핵심 수정: 언어 변경 후 에디터가 갱신되는 시간을 0.5초(500ms) 기다려준 뒤 코드 입력!
                setTimeout(function() {{

                    // 안전하게 변환된 코드 문자열 할당
                    var safeCode = `{safeCode}`;
                    // Codeforces 코드 입력기는 Ace 에디터를 주로 쓰므로 window.ace 객체가 존재하는지 확인
                    var editorEnv = window.ace ? window.ace.edit('editor') : null;
                    // 객체가 있으면 에디터 인스턴스 가져오기
                    if (editorEnv) {{
                        // Ace 에디터의 네이티브 API(setValue)를 사용해 커서 위치(1)와 함께 텍스트 주입
                        editorEnv.setValue(safeCode, 1);
                    // 만약 Ace 에디터가 로드되지 않았다면 일반 textarea 폼( fallback )을 찾기
                    }} else {{
                        // 원시적인 textarea ID 값 가져오기
                        var textarea = document.getElementById('sourceCodeTextarea');
                        // 해당 객체가 존재한다면
                        if(textarea) {{ 
                            // 텍스트를 바로 주입하고
                            textarea.value = safeCode; 
                            // input 이벤트 디스패치하여 폼 밸리데이션 통과
                            textarea.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        }}
                    }}
                }}, 500); // 0.5초 대기 후 로직
            // 익명함수 호출 종료
            }})();
        ";

                    // 완성된 JavaScript 문자열을 WebView의 JS 엔진에서 실행
                    await webViewCF.CoreWebView2.ExecuteScriptAsync(script);
                };

                // 해당 스크립트 실행 이벤트를 네비게이션 완료 시점에 바인딩
                webViewCF.CoreWebView2.NavigationCompleted += autoFillHandler;

                // 제출 확인 모달 창이 닫힐 때 발생하는 이벤트
                resultForm.FormClosing += (s, ev) =>
                {
                    // 주입용 이벤트 핸들러 혹시 남아있을 수 있으니 해제
                    webViewCF.CoreWebView2.NavigationCompleted -= autoFillHandler;
                    // WebView를 팝업 폼에서 제거
                    resultForm.Controls.Remove(webViewCF);
                    // 메인 폼으로 복귀시키고 다시 백그라운드 형태로 숨김
                    this.Controls.Add(webViewCF);
                    webViewCF.Visible = false;
                };

                // 결과 모달 창이 화면에 렌더링된 직후 이벤트
                resultForm.Shown += (s, ev) =>
                {
                    // 아까 조립해 둔 URL(submit 경로)로 웹 페이지 이동 시작 (이후 NavigationCompleted 발생)
                    webViewCF.CoreWebView2.Navigate(submitUrl);
                };

                // 사용자가 팝업 화면을 닫을 때까지 (혹은 제출 완료를 볼 때까지) 대기하는 동기식 팝업 오픈
                resultForm.ShowDialog(this);

                // 창이 닫힌 뒤 백그라운드에서는 채점이 어떻게 되었는지 API를 찔러 확인 준비
                // UI 갱신
                btnSubmitCF.Text = "채점 결과확인 중...";
                // API 통신 관련 로직 오류 방지를 위한 Try 캐치 블록
                try
                {
                    // 현재 사이트 내부에 로그인된 유저의 아이디(Handle)를 크롤링해오기 위한 JS 스크립트
                    string handleScript = @"
                (function() {
                    // 상단 프로필 링크를 가져와 텍스트를 추출
                    let link = document.querySelector('a[href^=""/profile/""]');
                    // 텍스트가 있으면 반환, 없으면 빈 문자열
                    return link ?
                    // 텍스트 공백 제거
                    link.innerText.trim() : '';
                })();
            ";

                    // 스크립트를 실행하여 유저 아이디 획득 후 큰따옴표 이스케이프 제거
                    string cfHandle = (await webViewCF.CoreWebView2.ExecuteScriptAsync(handleScript)).Trim('"');
                    // 유저 아이디를 성공적으로 가져왔다면
                    if (!string.IsNullOrEmpty(cfHandle))
                    {
                        // HTTP 통신을 위한 HttpClient 생성 및 Using문(자동 리소스 반환)
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            // 해당 핸들러(유저)의 가장 최근 제출 내역 1개를 조회하는 Codeforces Public API 주소
                            string apiUrl = $"https://codeforces.com/api/user.status?handle={cfHandle}&from=1&count=1";

                            // 채점 상태 초기값 (TESTING 상태면 계속 폴링 예정)
                            string verdict = "TESTING";
                            // 내역을 정상적으로 찾았는지 체크하는 플래그
                            bool foundSubmission = false;
                            // 무한 루프 방지용 횟수 제한 변수
                            int retryCount = 0;

                            // 최대 15회 반복(총 약 30초 내외)하면서 서버 상태 확인
                            while (retryCount < 15)
                            {
                                // 과도한 API 호출 방지를 위해 2초간 딜레이
                                await Task.Delay(2000);
                                // 반복 횟수 증가
                                retryCount++;

                                // API로 HTTP GET 요청 날려 JSON 응답 문자열 수신
                                string json = await client.GetStringAsync(apiUrl);
                                // Json 포맷을 C#의 JObject 객체로 파싱
                                Newtonsoft.Json.Linq.JObject data = Newtonsoft.Json.Linq.JObject.Parse(json);
                                // 상태값이 'OK'이고 결과 배열에 원소가 하나라도 있다면
                                if (data["status"]?.ToString() == "OK" && data["result"] != null && data["result"].Any())
                                {
                                    // 최근 1개 결과 참조
                                    var latestResult = data["result"][0];
                                    // JSON 필드에서 문제 ContestID와 Index 추출
                                    string pContestId = latestResult["problem"]["contestId"]?.ToString();
                                    string pIndex = latestResult["problem"]["index"]?.ToString();

                                    // 제출 기록의 문제 번호가 우리가 지금 제출한 문제의 ID와 같다면
                                    if ($"{pContestId}{pIndex}" == selId)
                                    {
                                        // 제대로 내 제출기록을 잡았으므로 플래그 On
                                        foundSubmission = true;
                                        // 채점 결과 판정 문자열 획득 (예: OK, WRONG_ANSWER 등)
                                        verdict = latestResult["verdict"]?.ToString() ?? "";

                                        // 아직 진행 중(TESTING)이거나 빈 문자열이 아니면 최종 채점이 끝난 것이므로
                                        if (verdict != "TESTING" && verdict != "")
                                        {
                                            // 상태 확인 루프 탈출
                                            break;
                                        // 내부 분기 끝
                                        }
                                    }
                                }
                            }

                            // 폴링 루프 종료 후, 아직도 내 제출 기록을 찾지 못한 경우 (API 지연 문제 등)
                            if (!foundSubmission)
                            {
                                // 실패 메시지 안내
                                MessageBox.Show("API 서버 지연으로 제출 내역을 찾지 못했거나 제출이 누락되었습니다.", "저장 취소");
                            // 조건 종료
                            }
                            // 찾았고 최종 판정 결과가 나왔다면
                            else if (verdict != "TESTING" && verdict != "")
                            {
                                // API에서 주는 상태값 "OK"는 우리 모델에서 "correct"로 취급하고, 그 외는 "wrong"으로 분류
                                string finalStatus = (verdict == "OK") ? "correct" : "wrong";

                                // Firebase 및 통계에 저장할 모델 객체 인스턴스화
                                var record = new codinglearning.Models.SubmissionRecord
                                {
                                    // 내가 작성했던 코드 원본 보관
                                    code = txtCode.Text,

                                    // 최종 상태, 언어, 제출 시간 등
                                    status = finalStatus,
                                    language = currentLang,
                                    date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                                    // 문제 정보 복사
                                    title = selTitle,
                                    diff = selDiff,

                                    // 태그 추가
                                    tags = selTags
                                };

                                // Firebase Realtime Database의 키로 사용할 문자열 치환 (샵이나 플러스 기호는 특수기호 에러 유발 방지)
                                string langKey = currentLang.Replace("#", "Sharp").Replace("+", "p");
                                // DB 매니저를 통해 온라인에 데이터 적재 (문제_언어 키 방식)
                                await firebaseManager.SaveSubmissionAsync($"{selId}_{langKey}", record, selDiff, selTags);

                                // 로컬 디스크의 오답 노트 백업 기능으로 정답 여부 계산
                                bool isCorrect = (finalStatus == "correct");
                                // 정답 여부에 따라 파일 매니저를 이용해 지정 경로에 작성 코드 마크다운 파일로 저장
                                fileManager.SaveCodeToLocalFile(selId, selTitle, txtCode.Text, currentLang, isCorrect);
                                // 사용자에게 보여줄 안내 메시지 삼항 연산자로 구성
                                string resultMsg = (finalStatus == "correct") ? "✅ 정답 (Accepted)" : $"❌ 오답 ({verdict})";
                                // 최종 저장 성공 팝업 출력
                                MessageBox.Show($"채점 확인 완료: {resultMsg}\n통계와 오답 노트에 자동 저장되었습니다! 📊", "자동 기록 완료");
                            // 조건문 종료
                            }
                        }
                    }
                    // Handle(유저 아이디) 값을 찾지 못한 경우
                    else
                    {
                        // 웹뷰 내에서 로그인이 풀려있을 가능성 고지
                        MessageBox.Show("로그인된 계정 정보를 찾을 수 없어 결과를 저장하지 못했습니다.");
                    // 에러 블록
                    }
                }
                // 결과를 얻어오는 로직 도중 내부 파싱 에러 등
                catch (Exception ex)
                {
                    // 오류 원인 팝업
                    MessageBox.Show($"결과를 가져오는 중 오류 발생: {ex.Message}");
                // 내부 catch 끝
                }
            }
            // 가장 외곽의 창 열기/조작 중 발생하는 전체 에러 잡기
            catch (Exception ex)
            {
                // 실패 메시지
                MessageBox.Show($"제출 중 오류 발생: {ex.Message}");
            // 바깥 예외처리 끝
            }
            // 전체 과정 완료 후 항상 버튼 초기화 보장
            finally
            {
                // 텍스트 원래대로 롤백
                btnSubmitCF.Text = "CF 제출";
            // 최종 종료
            }
        }
        #endregion

        #region [ 4. 탭 3 & 4: 오답 목록 및 통계 데이터 갱신 ]
        // 메인 탭 컨트롤의 인덱스가 바뀌면 (다른 탭을 누르면) 발생하는 이벤트
        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 사용자가 화면을 조작했음을 인지하여 세션 매니저 유휴 해제
            sessionManager.RecordUserAction();
            // 3번째 탭(오답노트) 진입 시 목록 새로고침 렌더링 시작
            if (tabControl1.SelectedIndex == 2) await LoadWrongListUI();
            // 4번째 탭(통계) 진입 시 차트 등 새로고침
            else if (tabControl1.SelectedIndex == 3) await LoadStatisticsUI();
            // 5번째 탭(시간 통계) 진입 시 기록 갱신
            else if (tabControl1.SelectedIndex == 4) await LoadTimeStatisticsUI();
        // 분기문 끝
        }

        // 오답 리스트 데이터를 Firebase에서 가져와 그리드뷰에 뿌려주는 비동기 함수
        private async Task LoadWrongListUI()
        {
            try
            {
                // 오답 DB 노드 통째로 로드 (예전 스키마 지원용)
                var wrongDict = await firebaseManager.GetWrongListAsync();
                // 전체 제출 기록 DB 노드 로드 (신규 통합 스키마 분석용)
                var allDict = await firebaseManager.GetAllSubmissionsAsync();

                // 테이블 초기화 및 8개의 열 구성
                dgvWrongList.Rows.Clear();
                dgvWrongList.ColumnCount = 8;
                dgvWrongList.Columns[0].HeaderText = "번호";
                dgvWrongList.Columns[1].HeaderText = "제목";
                dgvWrongList.Columns[2].HeaderText = "언어";
                // 컬럼 명칭 삽입
                dgvWrongList.Columns[3].HeaderText = "난이도";
                dgvWrongList.Columns[4].HeaderText = "태그";
                dgvWrongList.Columns[5].HeaderText = "결과";
                dgvWrongList.Columns[6].HeaderText = "발생일(풀이일)";
                dgvWrongList.Columns[7].HeaderText = "복습 예정일";

                // 하단의 세부 정보 표기 라벨값들을 하이픈으로 비움
                ClearWrongDetailLabels();
                // 중복 렌더링 방지를 위해 이미 순회한 문제 키를 담을 해시셋 생성
                HashSet<string> processedKeys = new HashSet<string>();
                // 표에 삽입하기 전 날짜순 정렬을 위해 배열 리스트로 임시 저장
                List<object[]> rowDataList = new List<object[]>();
                // 새 스키마 데이터가 존재한다면 반복문 실행
                if (allDict != null)
                {
                    // 각 문제별(Entry) 모든 시도 기록 파싱
                    foreach (var entry in allDict)
                    {
                        // JSON 키 (문제번호_언어 등) 추출
                        string normKey = entry.Key;
                        // 중복 방지 세트에 삽입
                        processedKeys.Add(normKey);

                        // JSON value 데이터를 JObject로 캐스팅
                        Newtonsoft.Json.Linq.JObject pData = Newtonsoft.Json.Linq.JObject.FromObject(entry.Value);
                        // _ 기준으로 자르고 앞부분인 문제번호 추출
                        string pNum = normKey.Split('_')[0];

                        // 초기 변수 하이픈 세팅
                        string title = "-", lang = "-", date = "-";
                        // DB 내부에 diff 또는 difficulty 키값으로 흩어진 데이터 예외처리
                        string diff = pData["diff"]?.ToString() ?? pData["difficulty"]?.ToString() ?? "-";
                        string tags = pData["tags"]?.ToString() ?? "-";

                        // 해당 문제를 '해결'했는지 플래그
                        bool isSolved = false;
                        // 몇 번 시도했는지 횟수 저장 변수
                        int tryCount = 0;

                        // attempts 배열(각 제출 기록 리스트) 가져오기
                        Newtonsoft.Json.Linq.JToken attemptsToken = pData["attempts"];
                        // 시도 데이터가 유효하면
                        if (attemptsToken != null && attemptsToken.HasValues)
                        {
                            // 총 시도 횟수 세팅
                            tryCount = attemptsToken.Children().Count();
                            // 가장 최신(마지막) 시도의 데이터를 가져오기
                            var lastAttempt = attemptsToken.Last is Newtonsoft.Json.Linq.JProperty prop ? prop.Value : attemptsToken.Last;

                            // 마지막 시도 기준 문제 이름과 언어 할당
                            title = lastAttempt["title"]?.ToString() ?? "-";
                            // 하이픈 예외
                            lang = lastAttempt["language"]?.ToString() ?? "-";
                            // 날짜 할당
                            date = lastAttempt["date"]?.ToString() ?? "-";

                            // 바깥쪽(상위 노드)에 데이터가 없으면 자식 노드에서 끌어올려 채우기
                            if (diff == "-") diff = lastAttempt["diff"]?.ToString() ?? "-";
                            // 태그 채우기
                            if (tags == "-") tags = lastAttempt["tags"]?.ToString() ?? "-";

                            // 각 시도 항목을 전부 돌면서 "정답"인 항목이 하나라도 있는지 확인
                            foreach (var attempt in attemptsToken)
                            {
                                // 속성 파싱
                                // 값 추출
                                var attData = attempt is Newtonsoft.Json.Linq.JProperty p ? p.Value : attempt;
                                // status 필드가 'correct'면 풀이 완료로 인정
                                if (attData["status"]?.ToString() == "correct") isSolved = true;
                            // 반복 종료
                            }
                        }

                        // 복습 예정일 초기화
                        string reviewDate = "-";
                        // 만약 구버전 DB 스키마(wrongDict)에도 이 문제가 있다면 추가 데이터 병합
                        if (wrongDict != null && wrongDict.ContainsKey(normKey))
                        {
                            // 값 가져오기
                            var wData = wrongDict[normKey];
                            // 없는 데이터 메우기
                            if (diff == "-") diff = wData["diff"]?.ToString() ?? "-";
                            if (tags == "-") tags = wData["tags"]?.ToString() ?? "-";
                            // 구버전 스키마에 존재하는 복습 날짜 가져오기
                            reviewDate = wData["reviewDate"]?.ToString() ?? "-";
                        }

                        // 라디오 버튼 필터 로직: "오답만 보기"일 때 맞춘 문제는 스킵
                        if (rbCorrect.Checked && !isSolved) continue;
                        // 라디오 버튼 필터: "정답만 보기"일 때 틀린 문제는 스킵
                        if (rbWrong.Checked && isSolved) continue;

                        // 최종 결과 텍스트를 해결됨/미해결에 따라 이모지와 함께 설정
                        string resultText = isSolved ? $"✅ 해결됨 ({tryCount}-Try)" : "❌ 미해결";
                        // 리스트에 데이터 배열 단위로 통째로 Add
                        rowDataList.Add(new object[] { pNum, title, lang, diff, tags, resultText, date, isSolved ? "-" : reviewDate });
                    // 메인 JSON 순회 끝
                    }
                }

                // 구버전 데이터(WrongList 노드)만 존재하고 새 데이터에는 없는 레거시 항목 백워드 호환 처리
                if (wrongDict != null)
                {
                    // 하나씩 돌면서
                    foreach (var entry in wrongDict)
                    {
                        // 이미 위에서 새 통합 스키마 로직으로 삽입된 데이터면 패스
                        if (processedKeys.Contains(entry.Key)) continue;
                        // 번호 추출
                        string pNum = entry.Key.Split('_')[0];
                        // 제목, 언어 할당
                        string title = entry.Value["title"]?.ToString() ?? "-";
                        string lang = entry.Value["language"]?.ToString() ?? "알 수 없음";
                        // 난이도, 태그 파싱
                        string diff = entry.Value["diff"]?.ToString() ?? "-";
                        string tags = entry.Value["tags"]?.ToString() ?? "-";
                        // 등록일 파싱
                        string date = entry.Value["addedDate"]?.ToString() ?? "-";
                        // 복습 예정일 파싱
                        string reviewDate = entry.Value["reviewDate"]?.ToString() ?? "-";
                        // 이후에 해결했는지 부울 값으로 판단
                        bool isSolved = entry.Value["solvedAfter"] != null && (bool)entry.Value["solvedAfter"];

                        // 필터 적용
                        if (rbCorrect.Checked && !isSolved) continue;
                        // 필터 역적용
                        if (rbWrong.Checked && isSolved) continue;

                        // 레거시 데이터는 시도 횟수 트래킹이 없으므로 단순 해결됨/미해결 표시
                        string resultText = isSolved ? "✅ 해결됨" : "❌ 미해결";
                        // 표 데이터에 추가
                        rowDataList.Add(new object[] { pNum, title, lang, diff, tags, resultText, date, isSolved ? "-" : reviewDate });
                    // 레거시 반복 끝
                    }
                }

                // 모인 데이터 리스트를 발생일자(날짜) 문자열 역순(최신순)으로 LINQ 정렬하여 새 리스트로 만듦
                var sortedRows = rowDataList.OrderByDescending(r => r[6]?.ToString() ?? "").ToList();
                // 정렬된 리스트를 차례대로 DataGridView 컨트롤에 삽입
                foreach (var row in sortedRows)
                {
                    // 행을 삽입하고 해당 행의 인덱스를 반환받음
                    int idx = dgvWrongList.Rows.Add(row);
                    // 만약 현재 미해결 문제인데, 복습 예정일이 지났다면(오늘 날짜보다 전이라면)
                    if (row[5].ToString().Contains("미해결") && DateTime.TryParse(row[7].ToString(), out DateTime rvDate) && rvDate < DateTime.Now)
                    {
                        // 사용자의 주의를 끌기 위해 행의 글자색을 붉은색(IndianRed)으로 칠함
                        dgvWrongList.Rows[idx].DefaultCellStyle.ForeColor = Color.IndianRed;
                        // 폰트 또한 굵은 스타일로 변경하여 강조
                        dgvWrongList.Rows[idx].DefaultCellStyle.Font = new Font(dgvWrongList.Font, FontStyle.Bold);
                    }
                }
            }
            // 전체 로딩 과정 중 DB 연결 끊김 등 에러가 나면 메시지 출력
            catch (Exception ex)
            {
                MessageBox.Show($"내역 로드 오류: {ex.Message}");
            // Catch 종료
            }
        }

        // 오답 목록 표의 셀을 클릭했을 때 하단 상세 정보 UI를 갱신하는 이벤트
        private void dgvWrongList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 액션 기록
            sessionManager.RecordUserAction();
            // 빈 행이나 헤더가 아닌 정상적인 행을 클릭했을 때
            if (e.RowIndex >= 0 && !dgvWrongList.Rows[e.RowIndex].IsNewRow)
            {
                // 데이터 가져오기
                DataGridViewRow row = dgvWrongList.Rows[e.RowIndex];
                // 각 라벨에 표에서 얻은 번호, 제목, 난이도, 태그 텍스트 박아넣기
                lblWrongProbNum.Text = row.Cells[0].Value?.ToString() ?? "-";
                lblWrongProbTitle.Text = row.Cells[1].Value?.ToString() ?? "-";
                lblWrongProbDiff.Text = row.Cells[3].Value?.ToString() ?? "-";
                lblWrongProbTags.Text = row.Cells[4].Value?.ToString() ?? "-";
                // 해결 상태 텍스트도 추출
                string resultText = row.Cells[5].Value?.ToString() ?? "-";
                // 결과에 '미해결'이라는 단어가 있으면 그대로 표기, 아니면 '해결됨'
                lblWrongProbResult.Text = resultText.Contains("미해결") ? "미해결" : "해결됨";
            // 조건문 끝
            }
        }

        // 선택된 오답 정보 라벨을 모두 리셋해주는 유틸리티
        private void ClearWrongDetailLabels()
        {
            // 전부 초기값 하이픈으로 복원
            lblWrongProbNum.Text = "-";
            lblWrongProbTitle.Text = "-";
            lblWrongProbDiff.Text = "-";
            lblWrongProbTags.Text = "-";
            lblWrongProbResult.Text = "-";
        // 끝
        }

        // [오답 문제 보기] 버튼으로 브라우저 모달창을 띄우는 이벤트
        private async void btnViewWrongProblem_Click(object sender, EventArgs e)
        {
            // 액션 기록
            sessionManager.RecordUserAction();
            // 현재 선택된 오답 문제의 번호 가져오기
            string wrongId = lblWrongProbNum.Text;

            // 라벨 값이 초기값이 아니거나 비어있지 않다면 유효한 문제 클릭
            if (wrongId != "나야 번호" && !string.IsNullOrEmpty(wrongId) && wrongId != "-")
            {
                // 앞선 탐색 탭과 동일하게 숫자와 문자를 분리
                string contestId = new String(wrongId.Where(Char.IsDigit).ToArray());
                // 인덱스 분리
                string index = new String(wrongId.Where(Char.IsLetter).ToArray());
                // URL 조합
                string url = $"https://codeforces.com/problemset/problem/{contestId}/{index}";

                // 문제 폼 생성
                Form probForm = new Form();
                // 제목 설정
                probForm.Text = $"[문제 보기] {wrongId} - {lblWrongProbTitle.Text}";
                probForm.Size = new Size(1000, 800);
                probForm.StartPosition = FormStartPosition.CenterScreen;

                // 웹뷰 생성
                var webView = new Microsoft.Web.WebView2.WinForms.WebView2();
                // 꽉 차게 도킹
                webView.Dock = DockStyle.Fill;
                probForm.Controls.Add(webView);

                // 폼 꺼지면 메모리 해제
                probForm.FormClosed += (s, ev) => { webView.Dispose(); };

                // 보여주기
                probForm.Show();

                // 웹뷰 환경 렌더링 대기
                await webView.EnsureCoreWebView2Async(null);
                // 이동 끝났을 때 다크모드 적용 스크립트 바인딩
                webView.CoreWebView2.NavigationCompleted += (s, args) => {
                    string darkModeCss = isDarkMode ?
                    // 스크립트 텍스트
                    "document.documentElement.style.filter = 'invert(85%) hue-rotate(180deg)';" : "";
                    webView.CoreWebView2.ExecuteScriptAsync(darkModeCss);
                };

                // 해당 URL로 렌더링 호출
                webView.CoreWebView2.Navigate(url);
            }
            // 문제 선택 안 했을 때
            else
            {
                // 안내 메시지
                MessageBox.Show("오답 목록에서 문제를 먼저 선택해주세요.");
            // 종료
            }
        }

        // 오답 탭에서 [다시 풀기] 버튼을 눌러 코드 작성 탭으로 전환하는 로직
        private void btnSolveAgain_Click(object sender, EventArgs e)
        {
            // 액션 기록
            sessionManager.RecordUserAction();
            // 선택된 문제가 없으면 경고 후 종료
            if (lblWrongProbNum.Text == "나야 번호" || string.IsNullOrEmpty(lblWrongProbNum.Text))
            {
                MessageBox.Show("다시 풀 오답을 선택해주세요.");
                // 탈출
                return;
            }

            // 코드 작성용 탭(탭 인덱스 1)의 전역 변수와 라벨 값을 오답 탭에서 복사해 세팅
            selId = lblCodeProbNum.Text = lblWrongProbNum.Text;
            // 제목 복사
            selTitle = lblCodeProbTitle.Text = lblWrongProbTitle.Text;
            selDiff = lblCodeProbDiff.Text = lblWrongProbDiff.Text;
            selTags = lblCodeProbTags.Text = lblWrongProbTags.Text;

            // 곧바로 원본 사이트를 띄워주는 함수 호출하여 바로 문제를 볼 수 있게 함
            btnViewWrongProblem_Click(sender, e);

            // 화면을 코드 작성 탭으로 강제 이동시킴
            tabControl1.SelectedIndex = 1;
        // 종료
        }

        // 전체 풀이 통계 데이터를 파이어베이스에서 가져와 막대그래프 등에 렌더링하는 비동기 함수
        private async Task LoadStatisticsUI()
        {
            // 모든 제출 JSON 정보 호출
            var dict = await firebaseManager.GetAllSubmissionsAsync();
            // 없으면 그냥 리턴
            if (dict == null) return;

            // 통계 집계를 위한 총 문제수 및 맞힌 문제수 카운터 변수
            int totalProblems = 0, correctCount = 0;
            // 최근 기록을 보여줄 표(dgvRecentRecords)가 있으면 초기화
            if (dgvRecentRecords != null)
            {
                // 행 비우기
                dgvRecentRecords.Rows.Clear();
                // 5개 열 생성
                dgvRecentRecords.ColumnCount = 5;

                // 헤더 텍스트 지정 (최근 기록 표 용)
                dgvRecentRecords.Columns[0].HeaderText = "번호";
                dgvRecentRecords.Columns[1].HeaderText = "제목";
                dgvRecentRecords.Columns[2].HeaderText = "언어";
                dgvRecentRecords.Columns[3].HeaderText = "결과";
                dgvRecentRecords.Columns[4].HeaderText = "날짜";
            // 조건 종료
            }

            // 제출 데이터 반복 순회
            foreach (var problem in dict)
            {
                // 전체 문제 수 1개 추가
                totalProblems++;
                // 이번 문제 내역 중 하나라도 맞춘 내역이 있는지 판단하는 플래그
                bool hasCorrect = false;
                // 문제 번호 추출
                string pNum = problem.Key.Split('_')[0];

                // 제출된 "시도 내역 배열"을 낱개로 순회
                foreach (var attempt in problem.Value["attempts"])
                {
                    // 각 시도의 정답 유무 상태 파싱
                    string status = attempt.Value["status"];
                    // 시도 날짜
                    string date = attempt.Value["date"];
                    // 문제 제목 (오류 시 하이픈)
                    string title = attempt.Value["title"]?.ToString() ?? "-";


                    // 작성 언어 파싱
                    string lang = attempt.Value["language"]?.ToString() ?? "알 수 없음";
                    // 현재 시도가 정답이라면, 해당 문제는 정답 처리됨으로 플래그 켬
                    if (status == "correct") hasCorrect = true;

                    // 최근 풀이 내역 표가 존재하면 행 데이터를 삽입
                    if (dgvRecentRecords != null)
                    {
                        // UI에 이모지와 함께 값 렌더링
                        dgvRecentRecords.Rows.Add(pNum, title, lang, status == "correct" ? "✅ 정답" : "❌ 오답", date);
                    // 조건 끝
                    }
                }
                // 하나의 문제에 대한 여러 시도 순회를 끝냈을 때, 하나라도 맞은 적이 있으면 정답 카운트 1 올림
                if (hasCorrect) correctCount++;
            // 반복 끝
            }

            // 오답 수는 전체 문제 수에서 맞춘 수를 빼서 도출
            int wrongCount = totalProblems - correctCount;
            // 정답률을 퍼센테이지로 실수 계산 (0 나누기 방지 포함)
            double accuracy = totalProblems > 0 ? ((double)correctCount / totalProblems) * 100 : 0;

            // 계산된 숫자들을 UI 통계 라벨에 텍스트로 반영
            lblTotalSolved.Text = totalProblems.ToString();
            lblCorrect.Text = correctCount.ToString();
            // 오답 텍스트
            lblWrong.Text = wrongCount.ToString();
            // 정답률 소수점 1자리까지 반올림하여 %와 함께 표시
            lblAccuracy.Text = $"{Math.Round(accuracy, 1)}%";

            // 차트 컨트롤이 존재하면 시각적으로 그리기 시작
            if (chartAccuracy != null && chartAccuracy.Series.Count > 0)
            {
                // 기존 데이터 포인트 싹 삭제
                chartAccuracy.Series[0].Points.Clear();
                // 기둥(Column) 형태의 차트 지정
                chartAccuracy.Series[0].ChartType = SeriesChartType.Column;
                // 정답 포인트, 오답 포인트 추가
                chartAccuracy.Series[0].Points.AddXY("정답", correctCount);
                chartAccuracy.Series[0].Points.AddXY("오답", wrongCount);

                // 현재 다크 모드 여부에 따라 정답 색상을 밝은 녹색 또는 어두운 녹색으로 설정
                Color correctColor = isDarkMode ? Color.FromArgb(90, 130, 90) : Color.FromArgb(143, 188, 143);
                // 오답 색상은 붉은 계열
                Color wrongColor = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.FromArgb(224, 159, 150);

                // 차트에 추가한 각 기둥에 색깔 입히기
                chartAccuracy.Series[0].Points[0].Color = correctColor;
                chartAccuracy.Series[0].Points[1].Color = wrongColor;
                // 시리즈 자체의 범례(Legend)는 중복이므로 숨김
                chartAccuracy.Series[0].IsVisibleInLegend = false;

                // 대신 우리가 직접 커스텀 범례 항목 생성 (색상+텍스트 매칭)
                chartAccuracy.Legends[0].CustomItems.Clear();
                chartAccuracy.Legends[0].CustomItems.Add(correctColor, "정답");
                chartAccuracy.Legends[0].CustomItems.Add(wrongColor, "오답");
            }
        }

        // 사용자의 '공부 시간' 세션 로그를 가져와 UI 차트 및 리스트에 그려주는 비동기 통계 함수
        private async Task LoadTimeStatisticsUI()
        {
            // 모든 학습 세션 로그 딕셔너리로 받아오기
            var logs = await firebaseManager.GetAllSessionLogsAsync();
            // 빈 데이터면 표 비우고 리턴
            if (logs == null || logs.Count == 0)
            {
                dgvTimeRecords.Rows.Clear();
                return;
                // 탈출
            }

            // 오늘 날짜 및 포맷팅용 변수
            DateTime now = DateTime.Now;
            string todayStr = now.ToString("yyyy-MM-dd");
            // 정확히 1주일 전 시간 계산
            DateTime weekAgo = now.AddDays(-7);

            // 오늘 누적, 주간 누적, 역대 최대 집중(1세션 최장) 기록 변수 초기화
            int todayTotalSeconds = 0, weeklyTotalSeconds = 0, maxFocusSeconds = 0;
            // 일별 공부 시간을 해시맵으로 저장해 차트 데이터 구성
            Dictionary<string, int> dailyStudyMap = new Dictionary<string, int>();
            // 최근 7일치 날짜를 Key로 잡고 시간을 0으로 초기화
            for (int i = 6; i >= 0; i--) dailyStudyMap[now.AddDays(-i).ToString("yyyy-MM-dd")] = 0;

            // 표 비우기
            dgvTimeRecords.Rows.Clear();
            // 4개 칼럼 설정
            dgvTimeRecords.ColumnCount = 4;
            dgvTimeRecords.Columns[0].Name = "날짜";
            dgvTimeRecords.Columns[1].Name = "학습 구간";
            dgvTimeRecords.Columns[2].Name = "순공 시간";
            dgvTimeRecords.Columns[3].Name = "성과";
            // 로그 종료일(sessionEnd) 기준으로 최신순 정렬해서 순회 시작
            foreach (var item in logs.OrderByDescending(x => x.Value.sessionEnd))
            {
                // 객체 꺼내기
                var log = item.Value;
                // 파싱 에러 방지
                if (!DateTime.TryParse(log.sessionEnd, out DateTime endTime)) continue;

                // 세션 시작 시각 = 종료 시각 - 진행된 초(Duration) 역추적 계산
                DateTime startTime = endTime.AddSeconds(-log.sessionDuration);
                // 해당 로그가 작성된 날짜 문자열
                string logDate = endTime.ToString("yyyy-MM-dd");
                // 오늘 작성된 로그면 당일 누적 시간에 합산
                if (logDate == todayStr) todayTotalSeconds += log.sessionDuration;
                // 최근 일주일 내의 데이터라면 주간 합산에 더함
                if (endTime >= weekAgo) weeklyTotalSeconds += log.sessionDuration;
                // 이번 세션 지속 시간이 역대 최대인지 비교 갱신
                if (log.sessionDuration > maxFocusSeconds) maxFocusSeconds = log.sessionDuration;
                // 차트용 날짜별 해시맵에 값 누적
                if (dailyStudyMap.ContainsKey(logDate)) dailyStudyMap[logDate] += log.sessionDuration;

                // 순수 지속 시간 포맷을 위한 TimeSpan 형변환
                TimeSpan duration = TimeSpan.FromSeconds(log.sessionDuration);
                // 표에 보여줄 학습 시간 간격 (시:분 ~ 시:분)
                string interval = $"{startTime:HH:mm} ~ {endTime:HH:mm}";
                // 순공 시간 (1시간 이상이면 시간+분, 아니면 분+초 출력)
                string netTime = duration.Hours > 0 ? $"{duration.Hours}시간 {duration.Minutes}분" : $"{duration.Minutes}분 {duration.Seconds}초";
                // 표 행에 삽입
                dgvTimeRecords.Rows.Add(logDate, interval, netTime, "학습 완료");
            }

            // UI의 오늘 누적 라벨이 있으면 갱신
            if (lblTodayTotal != null)
            {
                TimeSpan tToday = TimeSpan.FromSeconds(todayTotalSeconds);
                // 시 분 초 렌더링
                lblTodayTotal.Text = $"{tToday.Hours}h {tToday.Minutes}m {tToday.Seconds}s";
            }
            // 주간 평균(총합 / 7) 갱신
            if (lblWeeklyAvg != null)
            {
                TimeSpan tAvg = TimeSpan.FromSeconds(weeklyTotalSeconds / 7);
                // 평균 렌더링
                lblWeeklyAvg.Text = $"{tAvg.Hours}h {tAvg.Minutes}m {tAvg.Seconds}s";
            }
            // 최대 집중 라벨 갱신
            if (lblMaxFocus != null)
            {
                TimeSpan tMax = TimeSpan.FromSeconds(maxFocusSeconds);
                // 렌더링
                lblMaxFocus.Text = $"{tMax.Hours}h {tMax.Minutes}m {tMax.Seconds}s";
            }
            // 일별 공부 시간 막대 그래프 차트 객체 조작
            if (chartTimeHistory != null)
            {
                // 기존 시리즈 날리기
                chartTimeHistory.Series.Clear();
                // 새로운 시리즈(데이터 묶음) 이름 지정 후 생성
                var series = chartTimeHistory.Series.Add("학습 시간(분)");
                // 바 차트로 형태 지정
                series.ChartType = SeriesChartType.Bar;
                // 다크모드/라이트모드에 따라 바 색상 변경
                series.Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                // 막대 끝부분에 숫자 라벨 표시 허용
                series.IsValueShownAsLabel = true;

                // 숫자 라벨 글자색
                series.LabelForeColor = isDarkMode ? Color.White : Color.Black;
                // 앞서 7일치 일별 누적된 맵을 순회
                foreach (var kvp in dailyStudyMap)
                {
                    // 연도를 자르고 월-일만 X축으로 표기
                    string shortDate = kvp.Key.Substring(5);
                    // 초 단위 누적값을 60으로 나누어 '분' 단위 소수점 1자리수로 반올림 연산
                    double minutes = Math.Round(kvp.Value / 60.0, 1);
                    // 차트에 좌표 추가
                    series.Points.AddXY(shortDate, minutes);
                }
            }
        }
        #endregion

        #region [ 5. 유틸리티 및 UI 꾸미기 ]
        // 좌측 하단 등에 위치한 "GitHub Push" 링크/버튼 클릭 시 발생하는 이벤트
        private void lblGitHubPush_Click(object sender, EventArgs e)
        {
            // 백그라운드로 로컬 마크다운 파일 등을 실제 깃허브 원격 저장소에 Push하는 매니저 실행
            var (isSuccess, message, isInfo) = gitHubManager.PushToGitHub();
            // 반환 결과에 맞게 팝업 아이콘과 성공/오류 메시지를 뿌려줌
            MessageBox.Show(message, isInfo ? "알림" : (isSuccess ? "성공" : "오류"), MessageBoxButtons.OK, isInfo ? MessageBoxIcon.Information : (isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Error));
        // 끝
        }

        // 현재 오답 리스트에 있는 목록을 사용자의 PC에 Markdown 텍스트 파일로 추출(백업)해주는 버튼 이벤트
        private void btnExportWrongList_Click(object sender, EventArgs e)
        {
            // 파일 매니저가 그리드뷰 데이터를 읽어 MD 파일로 컨버팅 후 다운로드 처리
            var (isSuccess, message) = fileManager.ExportWrongListToMarkdown(dgvWrongList);
            // 성공 여부 팝업
            MessageBox.Show(message, isSuccess ? "추출 성공" : "알림", MessageBoxButtons.OK, isSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        // 끝
        }

        // 앱 초기 실행 시 각 표(DataGridView)와 차트의 기본 윤곽선 스타일을 미니멀하게 잡아주는 함수
        private void ApplyMinimalStyle()
        {
            // 앱에 있는 모든 데이터그리드뷰를 배열로 묶어서 일괄 포어치 처리
            DataGridView[] grids = { dgvTimeRecords, dgvWrongList, dgvProblems, dgvRecentRecords };
            // 하나씩 순회
            foreach (var grid in grids)
            {
                // 없으면 패스
                if (grid == null) continue;
                // 배경색을 흰색, 테두리를 투명(없음)으로 하여 모던한 디자인
                grid.BackgroundColor = Color.White;
                grid.BorderStyle = BorderStyle.None;
                // 가로로만 줄이 생기도록 지정
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                // 줄 색상
                grid.GridColor = Color.FromArgb(240, 240, 240);
                // 셀 선택(드래그) 시 배경색 미세 변경
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 245, 245);
                // 선택 시 글자색을 검정으로 고정해 가독성 확보
                grid.DefaultCellStyle.SelectionForeColor = Color.Black;
                // 왼쪽의 여백(Row Header) 삭제
                grid.RowHeadersVisible = false;
            }

            // 차트 1번 (학습 시간 기록) 세부 선 스타일 지우기
            if (chartTimeHistory != null)
            {
                // X축 모눈종이 세로선 삭제
                chartTimeHistory.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                // Y축 가로선 색상 옅게
                chartTimeHistory.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                // 테두리 틱 색상
                chartTimeHistory.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartTimeHistory.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            // 조건문 끝
            }
            // 차트 2번 (정답률) 동일하게 깔끔한 선 스타일 지정
            if (chartAccuracy != null)
            {
                // 세로선 삭제
                chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
                // 가로선 연하게
                chartAccuracy.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
                // 틱 연하게
                chartAccuracy.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chartAccuracy.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
            // 끝
            }
        }

        // 우측 상단의 🌙 다크/라이트 모드 라벨 클릭 시 전역 테마 토글하는 이벤트
        private void lblDarkMode_Click(object sender, EventArgs e)
        {
            // 플래그 반전 (true <-> false)
            isDarkMode = !isDarkMode;
            // 텍스트 아이콘 변경
            lblDarkMode.Text = isDarkMode ? "🌞 라이트 모드" : "🌙 다크 모드";
            // 색상 싹 다 칠하는 함수 호출
            ApplyTheme();
        // 끝
        }

        // WinForms 기본 TabControl의 탭 헤더(버튼)를 사용자가 지정한 색으로 직접 그리기(DrawItem) 위한 이벤트
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // 뒷배경색을 다크/라이트에 맞춰 결정
            // 어두운 회색
            Color formBgColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            // 탭 버튼의 사각형 영역을 구하고 2픽셀 늘림
            Rectangle bgRect = e.Bounds; bgRect.Inflate(2, 2);
            // 해당 영역에 배경색 채우기
            e.Graphics.FillRectangle(new SolidBrush(formBgColor), bgRect);
            // 현재 렌더링중인 탭이 "선택된 활성화 탭"인지 비트 연산으로 판별
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // 탭 자체 배경색 (선택되었으면 좀 더 밝거나 하얀색, 아니면 배경에 묻히게)
            // 다크/라이트별 조건 분기
            Color tabColor = isDarkMode ? (isSelected ? Color.FromArgb(60, 60, 65) : formBgColor) : (isSelected ? Color.White : SystemColors.Control);
            // 탭의 글씨 색상
            // 선택 시 흰색
            Color textColor = isDarkMode ? (isSelected ? Color.White : Color.Gray) : SystemColors.ControlText;

            // 실제 버튼 텍스트가 들어갈 사각형 미세 조정
            Rectangle tabRect = e.Bounds; tabRect.Inflate(-1, -1);
            // 색 채우기
            e.Graphics.FillRectangle(new SolidBrush(tabColor), tabRect);
            // 탭 인덱스에 맞는 이름 문자열 획득
            string tabText = tabControl1.TabPages[e.Index].Text;
            // 중앙 정렬하여 글씨 렌더링 그리기
            TextRenderer.DrawText(e.Graphics, tabText, e.Font, tabRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        // 현재 isDarkMode 플래그를 바탕으로 폼 내의 모든 UI 컨트롤의 배경/글자 색을 덮어씌우는 함수
        private void ApplyTheme()
        {
            // 전체 윈도우 폼 뒷배경 색상 정의
            // 어둡게 또는 시스템 컨트롤 색
            Color bgColor = isDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            // 일반 글씨 색상 정의 (어두우면 흰회색, 밝으면 검정 계열)
            Color textColor = isDarkMode ? Color.FromArgb(212, 212, 212) : SystemColors.ControlText;
            // 입력창, 콤보박스 등의 박스형 컨트롤의 바탕색 정의
            // 어두운 박스 또는 흰색
            Color boxColor = isDarkMode ? Color.FromArgb(37, 37, 38) : Color.White;
            // 표나 차트의 구분선(그리드) 색상
            Color gridLineColor = isDarkMode ? Color.FromArgb(63, 63, 70) : Color.LightGray;
            // 클릭되거나 활성화되는 텍스트 등의 강조 포인트 컬러 (파란색 계통)
            // 다크 블루
            Color accentColor = isDarkMode ? Color.FromArgb(86, 156, 214) : Color.SteelBlue;

            // 폼 자체에 적용
            this.BackColor = bgColor;
            this.ForeColor = textColor;
            // 탭 컨트롤 외형도 노멀하게 변경 후 배경 맞춤
            tabControl1.Appearance = TabAppearance.Normal;
            tabControl1.BackColor = bgColor;
            // 우측의 빈 공간을 가려줄 패널이 없으면 만들어서 부착 (탭 버튼 우측 배경 다크모드 대응용 꼼수)
            if (tabCoverPanel == null)
            {
                tabCoverPanel = new Panel();
                this.Controls.Add(tabCoverPanel);
            // 끝
            }
            // 마지막 탭 우측 좌표 가져오기
            Rectangle lastTab = tabControl1.GetTabRect(tabControl1.TabCount - 1);
            // 커버용 패널 색칠
            tabCoverPanel.BackColor = bgColor;
            // 위치와 크기를 맞춤
            tabCoverPanel.Location = new Point(tabControl1.Left + lastTab.Right, tabControl1.Top);
            tabCoverPanel.Size = new Size(tabControl1.Width - lastTab.Right, lastTab.Bottom);
            // 화면 크기 변환 대응 앵커링
            tabCoverPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 층을 맨 앞으로 당겨와서 지저분한 뒷부분 은폐
            tabCoverPanel.BringToFront();
            lblDarkMode.BringToFront();

            // 내부 재귀 함수를 호출하여 폼 내의 모든 자식 컨트롤(버튼, 라벨 등)을 일괄적으로 칠함
            ChangeControlColors(this, bgColor, textColor, boxColor, gridLineColor, accentColor);
        // 메서드 종료
        }

        // 재귀적으로 컨트롤 트리를 순회하며 타입에 맞게 색상을 주입하는 테마 엔진 코어 메서드
        private void ChangeControlColors(Control parent, Color bg, Color text, Color box, Color gridLine, Color accent)
        {
            // 자식 컨트롤 루프
            foreach (Control c in parent.Controls)
            {
                // 링크 라벨(URL처럼 파랗게 뜨는 텍스트)인 경우
                if (c is LinkLabel ll)
                {
                    // 배경 투명하게
                    ll.BackColor = Color.Transparent;
                    // 링크 기본색
                    ll.LinkColor = text;
                    // 방문색도 동일하게
                    ll.VisitedLinkColor = text;
                    // 활성(클릭)시 포인트 컬러
                    ll.ActiveLinkColor = accent;
                }

                // 일반 라벨이거나 라디오/체크박스처럼 텍스트만 떠있는 컨트롤은 배경 투명
                if (c is Label || c is CheckBox || c is RadioButton)
                {
                    c.BackColor = Color.Transparent;
                    // 글씨는 테마 텍스트색
                    c.ForeColor = text;
                }
                // 그 외 범용
                else
                {
                    c.BackColor = bg;
                    // 색 칠하기
                    c.ForeColor = text;
                }

                // 특별히 눈에 띄어야 하는 '순공 시간' 라벨 예외 처리 (크게 키우고 색상 포인트 지정)
                if (c.Name == "lblStudyTime")
                {
                    c.Font = new Font(c.Font.FontFamily, 24, FontStyle.Bold);
                    // 넘어가기
                    c.ForeColor = accent;
                    continue;
                }

                // 입력 박스 종류일 때
                if (c is TextBox || c is ComboBox)
                {
                    // 다크모드면 박스 내부를 진회색, 글자는 흰색
                    if (isDarkMode)
                    {
                        c.BackColor = Color.FromArgb(60, 60, 65);
                        // 흰색
                        c.ForeColor = Color.White;
                    }
                    // 라이트모드면
                    else
                    {
                        c.BackColor = box;
                        // 글자색
                        c.ForeColor = text;
                    }
                    // 콤보박스는 Flat 디자인으로 깔끔하게
                    if (c is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;
                    // 텍스트 박스의 외곽선(보더) 지정
                    if (c is TextBox tb)
                    {
                        // 다크 모드에선 선명하게(FixedSingle), 아니면 입체(Fixed3D)
                        tb.BorderStyle = isDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
                    }
                }
                // 표 컨트롤인 경우 배경 및 헤더 색상을 어둡게 덮어씌움
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = box;
                    // 셀과 라인 색상 매핑
                    dgv.DefaultCellStyle.BackColor = box; dgv.DefaultCellStyle.ForeColor = text;
                    dgv.GridColor = gridLine; dgv.EnableHeadersVisualStyles = false;
                    // 헤더를 살짝 더 밝은 회색으로
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = isDarkMode ? Color.FromArgb(50, 50, 50) : SystemColors.Control;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = text;
                }
                // 차트 컨트롤인 경우 각 영역, 축, 라벨 등을 전부 분해해서 색 지정
                else if (c is Chart chart)
                {
                    chart.BackColor = bg;
                    // 타이틀 글자색
                    if (chart.Titles.Count > 0) chart.Titles[0].ForeColor = text;
                    // 범례 글자색
                    if (chart.Legends.Count > 0)
                    {
                        chart.Legends[0].BackColor = Color.Transparent;
                        chart.Legends[0].ForeColor = text;
                    // 끝
                    }
                    // 그리는 영역 배경 및 라벨/그리드 선
                    foreach (var area in chart.ChartAreas)
                    {
                        area.BackColor = box;
                        // 축 글자
                        area.AxisX.LabelStyle.ForeColor = text; area.AxisY.LabelStyle.ForeColor = text;
                        // 선 색
                        area.AxisX.MajorGrid.LineColor = gridLine; area.AxisY.MajorGrid.LineColor = gridLine;
                        area.AxisX.LineColor = gridLine; area.AxisY.LineColor = gridLine;
                    // 차트 에어리어 루프 끝
                    }
                    // 실제 막대나 선 데이터(시리즈)가 존재한다면
                    if (chart.Series.Count > 0)
                    {
                        // 그라디언트 끄기
                        chart.Series[0].BackGradientStyle = GradientStyle.None;
                        // 막대 위 수치 색상
                        chart.Series[0].LabelForeColor = text;

                        // 학습 시간 차트는 파스텔 톤
                        if (chart.Name == "chartTimeHistory") chart.Series[0].Color = isDarkMode ? Color.FromArgb(70, 85, 100) : Color.FromArgb(160, 170, 180);
                        // 정답/오답 분포 차트는 녹/적 색상
                        else if (chart.Name == "chartAccuracy")
                        {
                            // 색상 변수
                            Color correctColor = isDarkMode ? Color.FromArgb(90, 130, 90) : Color.FromArgb(143, 188, 143);
                            Color wrongColor = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.FromArgb(224, 159, 150);
                            // 정답, 오답 컬럼에 각각 주입
                            if (chart.Series[0].Points.Count >= 2)
                            {
                                chart.Series[0].Points[0].Color = correctColor;
                                chart.Series[0].Points[1].Color = wrongColor;
                            // 끝
                            }
                            // 범례 아이콘 색상도 동기화
                            if (chart.Legends[0].CustomItems.Count >= 2)
                            {
                                chart.Legends[0].CustomItems[0].Color = correctColor;
                                // 두 번째
                                chart.Legends[0].CustomItems[1].Color = wrongColor;
                            }
                        }
                        // 그 외 차트들은 기본 액센트/경고 색상 사용
                        else
                        {
                            // 시리즈 1
                            chart.Series[0].Color = isDarkMode ? Color.FromArgb(60, 110, 160) : Color.SteelBlue;
                            // 시리즈 2가 있으면 붉은색
                            if (chart.Series.Count > 1)
                            {
                                chart.Series[1].BackGradientStyle = GradientStyle.None;
                                // 적용
                                chart.Series[1].Color = isDarkMode ? Color.FromArgb(160, 90, 90) : Color.IndianRed;
                            }
                        }
                    }
                }
                // 그룹 박스(네모 틀) 및 탭 뒷배경 패널일 경우
                else if (c is GroupBox || c is Panel)
                {
                    c.BackColor = bg;
                    // 포그라운드
                    c.ForeColor = text;
                }
                // 일반 누르는 버튼일 경우 (우측 하단 깃허브 라벨은 예외)
                else if (c is Button btn && c.Name != "btnGitHubPush")
                {
                    // 다크모드면 플랫(단순) 외곽선을 쓰고 회색조 컬러를 적용
                    if (isDarkMode)
                    {
                        // 플랫 선언
                        btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 90);
                        // 배경, 글자색
                        btn.BackColor = Color.FromArgb(60, 60, 65); btn.ForeColor = Color.White;
                        // 마우스 호버(올렸을 때), 클릭 시 색상도 커스텀
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 100, 130);
                        btn.FlatAppearance.MouseDownBackColor = accent;
                    // 조건 끝
                    }
                    // 라이트면 시스템 기본 버튼
                    else
                    {
                        btn.FlatStyle = FlatStyle.Standard;
                        // 칠하기
                        btn.BackColor = SystemColors.Control;
                        btn.ForeColor = SystemColors.ControlText;
                    }
                }
                // 제일 아래 상태 줄(StatusStrip)
                else if (c is StatusStrip statusStrip)
                {
                    // 다크모드면 완전 까맣게
                    statusStrip.BackColor = isDarkMode ? Color.FromArgb(20, 20, 20) : SystemColors.Control;
                }

                // 지금 조작한 컨트롤 안에 그룹박스처럼 자식 컨트롤들이 또 있다면 재귀 호출하여 남김없이 칠함
                if (c.HasChildren) ChangeControlColors(c, bg, text, box, gridLine, accent);
                // 메인 포어치 종료
            }
        }
        #endregion

        #region [ 6. AI 번역 관련 메서드 ]
        // 코드 텍스트 박스가 비어있을 때 사용자가 선택한 언어에 맞춰 기본 보일러플레이트 구조(using, class, main 등)를 제공하는 함수
        private string GetDefaultTemplate(string lang)
        {
            switch (lang)
            {
                // C# 기본 구조 템플릿
                case "C#": return "using System;\n\nnamespace CodingTest\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            // 여기에 코드를 작성하세요\n            \n        }\n    }\n}";
                // C++ 구조
                case "C++": return "#include <iostream>\nusing namespace std;\n\nint main() {\n    // 여기에 코드를 작성하세요\n    \n    return 0;\n}";
                // Java 구조
                case "Java": return "public class Main {\n    public static void main(String[] args) {\n        // 여기에 코드를 작성하세요\n        \n    }\n}";
                // 파이썬은 스크립트라 껍데기가 필요없음
                case "Python": return "";
                default: return "";
            }
        }
        // 이미 AI로 코드를 번역했는데 맘에 안들어서 "재시도(Retry)"를 누른 경우 동작하는 이벤트

        private async void btnRetryTranslate_Click(object sender, EventArgs e)
        {
            // 이전에 시도한 원본 코드나 대상 언어 정보가 없으면 시도할 수 없음
            if (string.IsNullOrEmpty(lastSourceCode) || string.IsNullOrEmpty(lastTargetLang))
            {
                MessageBox.Show("아직 번역된 코드가 없습니다.\n먼저 코드를 작성하고 언어를 변경해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 리턴
                return;
            }

            // 진짜 덮어쓸 것인지 확인 메시지
            DialogResult result = MessageBox.Show("번역 결과가 이상한가요?\nAI에게 똑같은 원본 코드를 다른 방식(확률)으로 다시 번역하라고 지시합니다.\n\n(※ 기존 번역 결과는 지워집니다.)", "다시 번역 (Retry)", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            // Yes를 누르면
            if (result == DialogResult.Yes)
            {
                // 기억해둔 원본 코드/언어로 다시 API 번역 요청
                await RequestTranslation(lastSourceCode, lastSourceLang, lastTargetLang);
            // 끝
            }
        }

        // 콤보박스(언어 선택 드롭다운)가 변경되었을 때 발생하는 핵심 AI 번역 트리거
        private async void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택한 언어가 뭐였는지 텍스트 추출
            string selectedLang = cbLanguage.SelectedItem?.ToString();
            // 똑같은 언어를 다시 고른 경우 무시
            if (selectedLang == previousLang) return;

            // 현재 텍스트 박스에 뭔가 "유저가 진짜 작성한 의미 있는 코드"가 있는지 확인하는 플래그
            bool isUserCode = false;
            // 박스 내용 가져오기
            string currentCode = txtCode.Text;
            // 코드가 비어있지 않다면
            if (!string.IsNullOrWhiteSpace(currentCode))
            {
                // 줄바꿈 통일 및 공백 제거하여 노멀라이즈
                string normCurrent = currentCode.Replace("\r\n", "\n").Trim();
                // 해당 언어의 기본 템플릿도 동일하게 노멀라이즈하여 가져오기
                string normTemplate = GetDefaultTemplate(previousLang).Replace("\r\n", "\n").Trim();
                // 템플릿과 완전히 다르면, 유저가 무언가 로직을 입력했다고 판단
                if (normCurrent != normTemplate) isUserCode = true;
            // 끝
            }

            // 진짜 작성된 코드가 있는데 언어를 바꿨다면
            if (isUserCode)
            {
                // 번역할 건지 물어봄
                DialogResult result = MessageBox.Show($"작성 중인 {previousLang} 코드가 있습니다.\n선택하신 {selectedLang}(으)로 🤖AI 자동 번역하시겠습니까?\n\n(예: AI 번역 유지 / 아니요: 초기화 / 취소: 변경 취소)", "AI 코드 자동 번역", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                // 취소면 콤보박스 선택 내역을 롤백
                if (result == DialogResult.Cancel)
                {
                    RevertComboBox();
                    return;
                }
                // 번역해달라고 '예'를 누른 경우
                else if (result == DialogResult.Yes)
                {
                    // 재시도를 위해 현재 텍스트(원본) 백업
                    lastSourceCode = currentCode;
                    // 언어들도 기억
                    lastSourceLang = previousLang;
                    lastTargetLang = selectedLang;
                    // Gemini API에 번역 요청을 던지는 유틸 비동기 호출
                    bool isSuccess = await RequestTranslation(currentCode, previousLang, selectedLang);
                    // 성공했으면 현재 언어를 타겟으로 바꾸고, 실패했으면 원상복구
                    if (isSuccess) previousLang = selectedLang;
                    else RevertComboBox();
                    // 종료
                    return;
                }
            }

            // 만약 유저 코드가 없는 상태거나(빈 화면), 아니요(번역 안 하고 날림)를 눌렀다면, 새로 선택한 언어의 템플릿을 화면에 뿌림
            txtCode.Text = GetDefaultTemplate(selectedLang);
            // 현재 언어 상태 갱신
            previousLang = selectedLang;
        }

        // 실제로 AI Gemini 서비스에 통신을 지시하는 래퍼 메서드
        private async Task<bool> RequestTranslation(string code, string sourceLang, string targetLang)
        {
            // 통신 중 유저가 또 언어를 바꾸는 걸 막기 위해 드롭다운 비활성화
            cbLanguage.Enabled = false;
            // 만약 에러나면 돌려놓을 백업용 기존 텍스트
            string originalText = txtCode.Text;

            // Gemini 서비스 호출 (스트리밍 혹은 단발성). 콜백으로 메시지를 계속 UI 텍스트 박스에 뿌려줌
            var (isSuccess, resultText) = await geminiService.TranslateCodeAsync(code, sourceLang, targetLang, msg =>
            {
                txtCode.Text = msg;
                txtCode.Refresh();
            });
            // 번역이 끝나면 콤보박스 잠금 해제
            cbLanguage.Enabled = true;

            // 결과 성공 유무 판단
            if (isSuccess)
            {
                // 결과 코드를 확정 렌더링
                txtCode.Text = resultText;
                // true 리턴
                return true;
            }
            // 실패했다면 (API 제한량, 네트워크 오류 등)
            else
            {
                // 실패 메시지 팝업
                MessageBox.Show(resultText, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 입력 박스 코드를 번역 전 원본으로 강제 롤백
                txtCode.Text = originalText;
                return false;
            }
        }

        // 콤보박스 값을 취소시켜 이전으로 돌리는 헬퍼 함수
        private void RevertComboBox()
        {
            // 값을 바꿀 때 이벤트가 두 번 트리거되지 않도록 잠깐 핸들러 제거
            cbLanguage.SelectedIndexChanged -= cbLanguage_SelectedIndexChanged;
            // 값 롤백
            cbLanguage.SelectedItem = previousLang;
            // 핸들러 다시 부착
            cbLanguage.SelectedIndexChanged += cbLanguage_SelectedIndexChanged;
        }
        #endregion

        // 오답 탭의 라디오 버튼 '전체 보기'를 체크하면 목록 갱신
        private async void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAll.Checked) await LoadWrongListUI();
        // 끝
        }

        // 라디오 버튼 '정답된 것만 보기'
        private async void rbCorrect_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCorrect.Checked) await LoadWrongListUI();
        // 끝
        }

        // 라디오 버튼 '틀린 채 남아있는 것만 보기'
        private async void rbWrong_CheckedChanged(object sender, EventArgs e)
        {
            if (rbWrong.Checked) await LoadWrongListUI();
        // 끝
        }

        // 프로그램 윈도우 X 버튼을 눌러 창을 종료하려 할 때 실행되는 최후의 이벤트
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 사용자가 아직 Break(휴식) 상태로 전환하지 않고 (즉 학습 중에) 창을 닫아버린 경우
            if (sessionManager.CurrentStatus != "Break")
            {
                // 당장 닫히는 것을 일단 취소(홀드)시킴
                e.Cancel = true;
                // 세션 타이머 강제 종료
                learningTimer.Stop();
                sessionManager.StopSession();

                // 누락 방지를 위해 현재까지 공부한 데이터를 객체로 포장
                var sessionData = new SessionData
                {
                    status = "Break",
                    sessionEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    sessionDuration = sessionManager.GetTotalSeconds(),

                    // 최근 활동 시간
                    lastActiveTime = sessionManager.LastActiveTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
                // 파이어베이스에 비동기로 안전하게 업로드 (프로그램이 닫히기 직전 마지막 기록)
                await firebaseManager.SaveSessionAsync(sessionData);
                await firebaseManager.PushSessionLogAsync(sessionData);

                // 업로드가 완료되었으므로 종료를 가로막던 이벤트를 삭제하고, 수동으로 폼을 완전히 Close() 해버림
                this.FormClosing -= Form1_FormClosing;
                this.Close();
            }
        }
    }
}