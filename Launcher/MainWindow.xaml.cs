using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Launcher.Models;
using Launcher.Services;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly GameLaunchService _gameLaunchService;
        private readonly IJavaLocatorService _javaLocatorService;
        private readonly RestService _restService;
        private readonly LauncherSettings _settings;
        private string _latestVersion = "";

        private bool IsCustomJavaMode => ((RadioButton)FindName("JavaCustomRadio")).IsChecked == true;

        public MainWindow(
            IAuthService authService,
            GameLaunchService gameLaunchService,
            IJavaLocatorService javaLocatorService,
            RestService restService,
            LauncherSettings settings)
        {
            _authService = authService;
            _gameLaunchService = gameLaunchService;
            _javaLocatorService = javaLocatorService;
            _restService = restService;

            InitializeComponent();

            var ver = Assembly.GetExecutingAssembly().GetName().Version!;
            AppVersionText.Text = $"v{ver.Major}.{ver.Minor}.{ver.Build}";
            InfoVersionText.Text = $"버전 {ver.Major}.{ver.Minor}.{ver.Build}";
            _settings = settings;

            GameDirTextBox.Text = _settings.GameDirectory;
            MemorySlider.Value = _settings.MaximumRamMb / 1024.0;
            MemoryValueText.Text = (_settings.MaximumRamMb / 1024).ToString();
            JvmArgsTextBox.Text = _settings.JvmArguments;
            JavaPathTextBox.Text = _javaLocatorService.FindJava21() ?? "";
            JavaCustomPathPanel.Visibility = Visibility.Collapsed;

            ScreenWidthTextBox.Text = _settings.ScreenWidth.ToString();
            ScreenHeightTextBox.Text = _settings.ScreenHeight.ToString();
            FullScreenCheckBox.IsChecked = _settings.FullScreen;
            ResolutionPanel.IsEnabled = !_settings.FullScreen;

            AutoConnectCheckBox.IsChecked = _settings.AutoConnectEnabled;
            ServerAddressTextBox.Text = _settings.ServerAddress;
            ServerPortTextBox.Text = _settings.ServerPort?.ToString() ?? "";
            ServerSettingsPanel.Visibility = _settings.AutoConnectEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;

            _gameLaunchService.FileProgressChanged += (_, args) =>
            {
                Dispatcher.Invoke(() =>
                {
                    PlayButton.Content = $"설치 중... ({args.ProgressedTasks}/{args.TotalTasks})";
                });
            };

            Loaded += async (_, _) =>
            {
                await CheckForUpdatesAsync();
                await LoadNoticesAsync();
                await TrySilentLoginAsync();
            };
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var versionInfo = await _restService.GetVersionInfoAsync();
                _latestVersion = versionInfo.LauncherVersion;

                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var latestVersion = Version.Parse(_latestVersion);

                if (latestVersion > currentVersion)
                {
                    UpdateDetailText.Text = $"v{currentVersion!.Major}.{currentVersion.Minor}.{currentVersion.Build} → v{_latestVersion}";
                    UpdateBanner.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                // 업데이트 확인 실패 시 무시
            }
        }

        private async Task LoadNoticesAsync()
        {
            var notices = await _restService.GetNoticeAsync();

            foreach (var notice in notices)
            {
                var tagColor = notice.Tag switch
                {
                    "공지" => "#4CAF50",
                    "점검" => "#F44336",
                    "안내" => "#FFB74D",
                    _ => "#B0B0C0"
                };

                var border = new Border
                {
                    Background = (System.Windows.Media.Brush)FindResource("SurfaceBrush"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var stack = new StackPanel();

                var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                var tagText = new TextBlock
                {
                    Text = $"[{notice.Tag}]",
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(tagColor)),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var dateText = new TextBlock
                {
                    Text = notice.CreatedAt.ToLocalTime().ToString("yyyy.MM.dd"),
                    FontSize = 10,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                headerGrid.Children.Add(tagText);
                headerGrid.Children.Add(dateText);

                var titleText = new TextBlock
                {
                    Text = notice.Title,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextPrimaryBrush")
                };

                var contentText = new TextBlock
                {
                    Text = notice.Content.Replace("\\n", "\n"),
                    FontSize = 12,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                };

                stack.Children.Add(headerGrid);
                stack.Children.Add(titleText);
                stack.Children.Add(contentText);
                border.Child = stack;

                NoticeBoardPanel.Children.Add(border);
            }

            if (notices.Count == 0)
            {
                NoticeBoardPanel.Children.Add(new TextBlock
                {
                    Text = "공지사항이 없습니다.",
                    FontSize = 13,
                    Foreground = (System.Windows.Media.Brush)FindResource("TextSecondaryBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.IsEnabled = false;
            button.Content = "다운로드 중...";

            try
            {
                var downloadUrl = await _restService.GetLauncherDownloadUrlAsync();
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    MessageBox.Show("다운로드 URL을 가져올 수 없습니다.", "LizardMC Launcher",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentDir = AppContext.BaseDirectory;
                var tempDir = Path.Combine(Path.GetTempPath(), "LizardMCLauncher_update");
                var zipPath = Path.Combine(tempDir, "update.zip");
                var extractPath = Path.Combine(tempDir, "extracted");

                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                await _restService.DownloadFile(downloadUrl, zipPath);

                button.Content = "압축 해제 중...";
                ZipFile.ExtractToDirectory(zipPath, extractPath);

                // 배치 스크립트로 파일 교체 후 재시작
                var currentExePath = Environment.ProcessPath!;
                var batPath = Path.Combine(tempDir, "lizardmc_update.bat");
                var batContent = $"""
                    @echo off
                    timeout /t 2 /nobreak >nul
                    xcopy "{extractPath}\*" "{currentDir}" /s /e /y
                    start "" "{currentExePath}"
                    rmdir /s /q "{tempDir}"
                    del "%~f0"
                    """;
                await File.WriteAllTextAsync(batPath, batContent);

                Process.Start(new ProcessStartInfo
                {
                    FileName = batPath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"업데이트 실패: {ex.Message}", "LizardMC Launcher",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                button.Content = "업데이트";
                button.IsEnabled = true;
            }
        }

        // Title bar
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            Close();
        }

        // Navigation
        private void NavHome_Click(object sender, RoutedEventArgs e) => SwitchTab("Home");
        private void NavSettings_Click(object sender, RoutedEventArgs e) => SwitchTab("Settings");
        private void NavAbout_Click(object sender, RoutedEventArgs e) => SwitchTab("About");

        private void SwitchTab(string tab)
        {
            HomeTab.Visibility = Visibility.Collapsed;
            SettingsTab.Visibility = Visibility.Collapsed;
            AboutTab.Visibility = Visibility.Collapsed;

            NavHome.Style = (Style)FindResource("NavButtonStyle");
            NavSettings.Style = (Style)FindResource("NavButtonStyle");
            NavAbout.Style = (Style)FindResource("NavButtonStyle");

            switch (tab)
            {
                case "Home":
                    HomeTab.Visibility = Visibility.Visible;
                    NavHome.Style = (Style)FindResource("NavButtonActiveStyle");
                    break;
                case "Settings":
                    SettingsTab.Visibility = Visibility.Visible;
                    NavSettings.Style = (Style)FindResource("NavButtonActiveStyle");
                    break;
                case "About":
                    AboutTab.Visibility = Visibility.Visible;
                    NavAbout.Style = (Style)FindResource("NavButtonActiveStyle");
                    break;
            }
        }

        // Settings
        private void MemorySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MemoryValueText != null)
                MemoryValueText.Text = ((int)e.NewValue).ToString();
            if (_settings != null)
                _settings.MaximumRamMb = ((int)e.NewValue) * 1024;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "게임 디렉토리 선택"
            };

            if (dialog.ShowDialog() == true)
            {
                GameDirTextBox.Text = dialog.FolderName;
                _settings.GameDirectory = dialog.FolderName;
            }
        }

        private void JavaModeRadio_Changed(object sender, RoutedEventArgs e)
        {
            var panel = FindName("JavaCustomPathPanel") as Grid;
            if (panel != null)
                panel.Visibility = IsCustomJavaMode
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void BrowseJavaButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "javaw.exe 선택",
                Filter = "javaw.exe|javaw.exe|모든 파일|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                JavaPathTextBox.Text = dialog.FileName;
                _settings.JavaPath = dialog.FileName;
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.JvmArguments = JvmArgsTextBox.Text;
            _settings.Save();

            SaveSettingsButton.Content = "저장됨!";
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (_, _) =>
            {
                SaveSettingsButton.Content = "설정 저장";
                timer.Stop();
            };
            timer.Start();
        }

        // Game Window Settings
        private void FullScreenCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_settings == null) return;
            _settings.FullScreen = FullScreenCheckBox.IsChecked == true;
            ResolutionPanel.IsEnabled = !_settings.FullScreen;
        }

        private void ScreenWidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && int.TryParse(ScreenWidthTextBox.Text, out var width) && width > 0)
                _settings.ScreenWidth = width;
        }

        private void ScreenHeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null && int.TryParse(ScreenHeightTextBox.Text, out var height) && height > 0)
                _settings.ScreenHeight = height;
        }

        // Server Auto-Connect Settings
        private void AutoConnectCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_settings == null) return;
            _settings.AutoConnectEnabled = AutoConnectCheckBox.IsChecked == true;
            ServerSettingsPanel.Visibility = _settings.AutoConnectEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ServerAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings != null)
                _settings.ServerAddress = ServerAddressTextBox.Text;
        }

        private void ServerPortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_settings == null) return;
            if (string.IsNullOrWhiteSpace(ServerPortTextBox.Text))
                _settings.ServerPort = null;
            else if (int.TryParse(ServerPortTextBox.Text, out var port) && port > 0)
                _settings.ServerPort = port;
        }

        // Login
        private async Task TrySilentLoginAsync()
        {
            try
            {
                var session = await _authService.LoginSilentlyAsync();
                if (!string.IsNullOrEmpty(session.Username))
                {
                    PlayerNameText.Text = session.Username;
                    PlayerStatusText.Text = "로그인됨";
                    LoginButton.Content = "완료";
                    LoginButton.IsEnabled = false;
                    PlayButton.IsEnabled = true;
                }
            }
            catch
            {
                // 자동 로그인 실패 시 무시 — 유저가 수동으로 로그인
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            LoginButton.Content = "로그인 중...";

            try
            {
                var session = await _authService.LoginAsync();

                if (!string.IsNullOrEmpty(session.Username))
                {
                    PlayerNameText.Text = session.Username;
                    PlayerStatusText.Text = "로그인됨";
                    LoginButton.Content = "완료";
                    LoginButton.IsEnabled = false;
                    PlayButton.IsEnabled = true;
                }
                else
                {
                    LoginButton.Content = "로그인";
                    LoginButton.IsEnabled = true;
                    MessageBox.Show(
                        "로그인 실패: 사용자 이름을 받지 못했습니다.",
                        "LizardMC Launcher",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LoginButton.Content = "로그인";
                LoginButton.IsEnabled = true;
                MessageBox.Show(
                    $"로그인 실패: {ex.Message}",
                    "LizardMC Launcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Play
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var playButton = (Button)sender;
            playButton.IsEnabled = false;
            playButton.Content = "설치 중...";

            try
            {
                _settings.JvmArguments = JvmArgsTextBox.Text;

                if (IsCustomJavaMode)
                {
                    if (string.IsNullOrEmpty(_settings.JavaPath) || !File.Exists(_settings.JavaPath))
                    {
                        MessageBox.Show(
                            "Java 경로가 설정되지 않았습니다. 설정에서 Java 21 경로를 지정해주세요.",
                            "LizardMC Launcher",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    _settings.JavaPath = null;
                }

                var versionInfo = await _restService.GetVersionInfoAsync();
                var forceResync = ForceResyncCheckBox.IsChecked == true;

                // 마인크래프트 인스톨 상황 확인 및 인스톨
                await _gameLaunchService.CheckMinecraftInstalled(versionInfo.MinecraftVersion);

                // 네오포지 인스톨 상황 확인 및 인스톨
                var checkResult = _gameLaunchService.CheckNeoforgeInstalled(versionInfo.NeoforgeVersion);
                if (!checkResult&&!forceResync) await _gameLaunchService.InstallNeoforgeAsync(versionInfo.NeoforgeVersion);

                ForceResyncCheckBox.IsChecked = false;
                playButton.Content = "실행 중...";

                await _gameLaunchService.LaunchAsync(
                    versionInfo.NeoforgeVersion,
                    _authService.CurrentSession!,
                    _settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"실행 실패: {ex.Message}",
                    "LizardMC Launcher",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                playButton.Content = "\u25B6  플레이";
                playButton.IsEnabled = true;
            }
        }
    }
}
