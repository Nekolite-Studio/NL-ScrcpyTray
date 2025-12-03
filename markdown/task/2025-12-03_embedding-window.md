# 🛠️ Scrcpy ウィンドウ埋め込み 実装ガイド (プロトタイプ)

## 1. 概要

このガイドでは、Win32 API (`SetParent`) を使用して、外部プロセスである `scrcpy.exe` のウィンドウを WPF アプリケーションの `WindowsFormsHost` 内に強制的に表示させる手順を解説します。

## 2. 実装ステップ

### Step 1: Win32 API 定義クラスの作成

Windowsのシステム関数を呼び出すためのヘルパークラスを作成します。
`src/Helpers/NativeMethods.cs` (新規作成) として配置するか、テスト用に `MainWindow.xaml.cs` 内に記述しても構いません。

```csharp
using System;
using System.Runtime.InteropServices;

namespace NL_ScrcpyTray.Helpers
{
    public static class NativeMethods
    {
        // 親ウィンドウを変更するAPI
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        // ウィンドウの属性（スタイル）を変更するAPI
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        // ウィンドウの属性を取得するAPI
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        // ウィンドウの位置とサイズを変更するAPI
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // 定数
        public const int GWL_STYLE = -16;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_POPUP = unchecked((int)0x80000000); // タイトルバーなし
    }
}
```

### Step 2: WPF画面への埋め込み領域の追加

`src/MainWindow.xaml` に、`scrcpy` を表示するための受け皿 (`WindowsFormsHost`) を追加します。

既存の `WebView2` と重ならないように、テスト用に一時的に `Grid` でレイアウトを分けます。

```xml
<Window ...
        xmlns:wf="clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms" 
        Height="600" Width="800">
    
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="200" /> <ColumnDefinition Width="*" />   </Grid.ColumnDefinitions>

        <StackPanel Grid.Column="0" Background="#f0f0f0">
            <Button x:Name="BtnStartTest" Content="埋め込みテスト開始" Click="BtnStartTest_Click" Margin="10" Padding="10"/>
            <TextBlock Text="※USB接続したデバイスがある状態で押してください" TextWrapping="Wrap" Margin="10"/>
        </StackPanel>

        <WindowsFormsHost Grid.Column="1" x:Name="ScrcpyHost">
            <wf:Panel x:Name="EmbeddingPanel" BackColor="Black"/>
        </WindowsFormsHost>
    </Grid>
</Window>
```

### Step 3: 埋め込みロジックの実装

`src/MainWindow.xaml.cs` に、ボタンクリック時の処理として実装します。
今回はテストのため、`ScrcpyProcessManager` を経由せず、直接ここにロジックを書きます。

```csharp
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using NL_ScrcpyTray.Helpers; // Step 1で作った名前空間

namespace NL_ScrcpyTray
{
    public partial class MainWindow : Window
    {
        private Process? _scrcpyProcess;

        // ... コンストラクタ等は既存のまま ...

        private async void BtnStartTest_Click(object sender, RoutedEventArgs e)
        {
            // 1. scrcpy.exe のパス準備 (環境に合わせて調整してください)
            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var scrcpyPath = Path.Combine(baseDir, "scrcpy", "scrcpy.exe");

            // 2. 起動引数の設定
            // --window-borderless: scrcpy側でも枠を消す指定をしておくとスムーズ
            // --no-control: テスト中はマウス入力を無効にしたい場合に追加
            var startInfo = new ProcessStartInfo
            {
                FileName = scrcpyPath,
                Arguments = "--window-borderless", 
                UseShellExecute = false
            };

            // 3. プロセス起動
            _scrcpyProcess = Process.Start(startInfo);
            if (_scrcpyProcess == null) return;

            // 4. ウィンドウハンドルが生成されるまで待機 (重要!)
            IntPtr scrcpyHwnd = IntPtr.Zero;
            int maxRetries = 50; // 最大5秒待つ

            for (int i = 0; i < maxRetries; i++)
            {
                _scrcpyProcess.Refresh();
                if (_scrcpyProcess.HasExited)
                {
                    MessageBox.Show("scrcpyがすぐに終了してしまいました。");
                    return;
                }

                scrcpyHwnd = _scrcpyProcess.MainWindowHandle;
                if (scrcpyHwnd != IntPtr.Zero) break;

                await Task.Delay(100); // 0.1秒待機
            }

            if (scrcpyHwnd == IntPtr.Zero)
            {
                MessageBox.Show("ウィンドウハンドルが見つかりませんでした。");
                return;
            }

            // 5. 親子関係の変更 (埋め込み実行)
            // WPF上の Panel のハンドルを親に設定する
            NativeMethods.SetParent(scrcpyHwnd, EmbeddingPanel.Handle);

            // 6. スタイルの強制変更 (念のためタイトルバー等を削除)
            // 現在のスタイルを取得し、POPUP属性(枠なし)を付与
            int style = NativeMethods.GetWindowLong(scrcpyHwnd, NativeMethods.GWL_STYLE);
            NativeMethods.SetWindowLong(scrcpyHwnd, NativeMethods.GWL_STYLE, (style | NativeMethods.WS_POPUP));

            // 7. サイズ合わせ
            UpdateScrcpySize();
        }

        // ウィンドウサイズ変更時に scrcpy のサイズも追従させる
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrcpySize();
        }

        private void UpdateScrcpySize()
        {
            if (_scrcpyProcess != null && !_scrcpyProcess.HasExited && _scrcpyProcess.MainWindowHandle != IntPtr.Zero)
            {
                // EmbeddingPanel のサイズを取得
                // ※ WPFのサイズとWinForms/Win32のピクセルサイズはずれることがあるため、
                //    厳密には DPIスケーリングの考慮が必要ですが、テストでは Panel.Width/Height で簡易対応します。
                
                int width = EmbeddingPanel.Width; 
                int height = EmbeddingPanel.Height;
                
                // Panelのサイズが0の場合は親のWindowsFormsHostから取るなどの工夫が必要ですが
                // ここでは簡易的に親コンテナのサイズを使います
                width = (int)ScrcpyHost.ActualWidth;
                height = (int)ScrcpyHost.ActualHeight;

                NativeMethods.MoveWindow(_scrcpyProcess.MainWindowHandle, 0, 0, width, height, true);
            }
        }
        
        // アプリ終了時にプロセスを道連れにする
        protected override void OnClosed(EventArgs e)
        {
            if (_scrcpyProcess != null && !_scrcpyProcess.HasExited)
            {
                _scrcpyProcess.Kill();
            }
            base.OnClosed(e);
        }
    }
}
```

### Step 4: XAML側のイベント紐づけ

`src/MainWindow.xaml` の `<Window>` タグに `SizeChanged` イベントを追加するのを忘れないでください。

```xml
<Window x:Class="NL_ScrcpyTray.MainWindow"
        ...
        SizeChanged="Window_SizeChanged">
```

## 3. テスト実行と確認

1.  Android端末をUSB接続します。
2.  アプリを実行し、追加した「埋め込みテスト開始」ボタンを押します。
3.  **成功:** 右側の黒いエリアに Android の画面が表示されます。
4.  ウィンドウの端をドラッグしてサイズを変えると、中の Android 画面も一緒にリサイズされることを確認します。

## 注意点

  * **DPIスケーリング:** 高DPIモニター環境では、WPFのサイズ単位とWin32 APIのピクセル単位が異なるため、埋め込んだ画面が小さく表示されたり、はみ出したりすることがあります。本格実装時は `PresentationSource` からDPI倍率を取得して補正する必要があります。
  * **エラー処理:** このコードは例外処理を省略しています。実運用では `try-catch` で囲んでください。