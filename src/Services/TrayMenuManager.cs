using NL_ScrcpyTray.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NL_ScrcpyTray.Services
{
    /// <summary>
    /// NotifyIcon (タスクトレイアイコン) の生成と、そのコンテキストメニューの管理を専門に担当します。
    /// </summary>
    public class TrayMenuManager : IDisposable
    {
        private readonly DeviceManager _deviceManager;
        private readonly SettingsManager _settingsManager;
        private readonly Action _showMainWindowAction;
        private readonly Action _exitApplicationAction;

        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _contextMenuStrip;

        // メニュー項目をフィールドとして保持
        private ToolStripMenuItem? _autoConnectMenuItem;
        private ToolStripMenuItem? _stopAllMenuItem;
        private ToolStripMenuItem? _openSaveFolderMenuItem;

        public TrayMenuManager(
            DeviceManager deviceManager,
            SettingsManager settingsManager,
            Action showMainWindowAction,
            Action exitApplicationAction)
        {
            _deviceManager = deviceManager;
            _settingsManager = settingsManager;
            _showMainWindowAction = showMainWindowAction;
            _exitApplicationAction = exitApplicationAction;
        }

        /// <summary>
        /// タスクトレイアイコンとメニューを初期化して表示します。
        /// </summary>
        public void Initialize(Icon appIcon)
        {
            _contextMenuStrip = new ContextMenuStrip();
            _notifyIcon = new NotifyIcon
            {
                Icon = appIcon,
                Text = "NL-ScrcpyTray",
                Visible = true,
                ContextMenuStrip = _contextMenuStrip
            };

            // イベントハンドラを登録
            _notifyIcon.DoubleClick += (s, args) => _showMainWindowAction();
            _contextMenuStrip.Opening += OnContextMenuOpening;

            // イベントを購読
            _deviceManager.DeviceListChanged += OnDeviceListChanged;

            BuildMenu();
        }

        /// <summary>
        /// コンテキストメニューの構造を構築します。
        /// </summary>
        private void BuildMenu()
        {
            if (_contextMenuStrip == null) return;

            _contextMenuStrip.Items.Clear();

            // セクションごとのプレースホルダーを追加。実際のアイテムはUpdateDeviceListMenuItemsで挿入される。
            // 1. デバイスリストセクション (動的生成)
            _contextMenuStrip.Items.Add(new ToolStripSeparator());

            // 2. グローバル操作セクション
            _stopAllMenuItem = new ToolStripMenuItem("すべてのミラーリングを終了", null, OnStopAllClick);
            _contextMenuStrip.Items.Add(_stopAllMenuItem);
            _contextMenuStrip.Items.Add(new ToolStripSeparator());

            // 3. 設定・ユーティリティセクション
            var appSettings = _settingsManager.Load();
            _autoConnectMenuItem = new ToolStripMenuItem("自動接続を有効にする", null, OnAutoConnectClick)
            {
                CheckOnClick = false, // 手動で状態をトグルするためfalseに設定
                Checked = appSettings.GlobalAutoConnect
            };
            _contextMenuStrip.Items.Add(_autoConnectMenuItem);
            
            _openSaveFolderMenuItem = new ToolStripMenuItem("保存先フォルダを開く", null, OnOpenSaveFolderClick);
            _contextMenuStrip.Items.Add(_openSaveFolderMenuItem);

            _contextMenuStrip.Items.Add(new ToolStripSeparator());

            // 4. アプリ操作セクション
            _contextMenuStrip.Items.Add("設定画面を表示...", null, (s, e) => _showMainWindowAction());
            _contextMenuStrip.Items.Add("終了", null, (s, e) => _exitApplicationAction());
        }

        private void OnContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_stopAllMenuItem != null)
            {
                _stopAllMenuItem.Enabled = _deviceManager.GetManagedDevices().Any(d => d.IsMirroring);
            }

            if (_autoConnectMenuItem != null)
            {
                var settings = _settingsManager.Load();
                _autoConnectMenuItem.Checked = settings.GlobalAutoConnect;
            }
            
            UpdateDeviceListMenuItems();
        }

        private void OnDeviceListChanged(List<DeviceViewModel> devices)
        {
            // DeviceManagerからのイベントは別スレッドから来る可能性があるため、UIスレッドで実行する
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateDeviceListMenuItems();
            });
        }

        /// <summary>
        /// デバイスリスト部分のメニュー項目を最新の状態に更新します。
        /// </summary>
        private void UpdateDeviceListMenuItems()
        {
            if (_contextMenuStrip == null) return;

            // --- 既存のデバイス項目をクリア ---
            // 最初のセパレータが見つかるまで、先頭からアイテムを削除し続ける
            while (_contextMenuStrip.Items.Count > 0)
            {
                if (_contextMenuStrip.Items[0] is ToolStripSeparator)
                {
                    break; // セパレータに到達したら削除を停止
                }
                _contextMenuStrip.Items.RemoveAt(0);
            }
            
            // --- 新しいデバイス項目を挿入 ---
            var devices = _deviceManager.GetManagedDevices();
            if (devices.Count == 0)
            {
                _contextMenuStrip.Items.Insert(0, new ToolStripMenuItem("接続されているデバイスはありません", null) { Enabled = false });
                return;
            }

            foreach (var device in devices.AsEnumerable().Reverse()) // 逆順にしてInsert(0,...)で正しい順序にする
            {
                var statusText = device.Status switch
                {
                    ConnectionStatus.Usb => "USB",
                    ConnectionStatus.Wifi => "Wi-Fi",
                    ConnectionStatus.UsbAndWifi => "USB & Wi-Fi",
                    _ => "Offline"
                };

                var menuItem = new ToolStripMenuItem($"{device.Name} ({statusText})")
                {
                    Checked = device.IsMirroring,
                    Enabled = device.Status != ConnectionStatus.Offline,
                    Tag = device // イベントハンドラでデバイスを特定するためにTagに格納
                };
                menuItem.Click += OnDeviceMenuItemClick;
                _contextMenuStrip.Items.Insert(0, menuItem);
            }
        }

        private void OnDeviceMenuItemClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem { Tag: DeviceViewModel device })
            {
                ToggleMirroring(device);
            }
        }

        private void ToggleMirroring(DeviceViewModel device)
        {
            _deviceManager.ToggleMirroring(device.Id);
        }

        private void OnStopAllClick(object? sender, EventArgs e)
        {
            _deviceManager.StopAllMirroring();
        }

        private void OnAutoConnectClick(object? sender, EventArgs e)
        {
            if (_autoConnectMenuItem == null) return;

            // チェック状態を手動で反転
            _autoConnectMenuItem.Checked = !_autoConnectMenuItem.Checked;

            var settings = _settingsManager.Load();
            settings.GlobalAutoConnect = _autoConnectMenuItem.Checked;
            _settingsManager.Save(settings);
        }

        private void OnOpenSaveFolderClick(object? sender, EventArgs e)
        {
            var settings = _settingsManager.Load();
            // グローバルな録画パス、または最初の有効なパスを探す
            var path = settings.Devices
                .Select(d => d.Settings.Recording.SavePath)
                .FirstOrDefault(p => !string.IsNullOrEmpty(p));

            if (path == null)
            {
                // TODO: パスがどこにも設定されていない場合の通知
                return;
            }

            try
            {
                if (!Directory.Exists(path))
                {
                    // フォルダが存在しない場合は作成を試みる
                    Directory.CreateDirectory(path);
                }
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                // TODO: フォルダ作成やエクスプローラー起動に失敗した場合の通知
                Console.WriteLine($"Failed to open save folder: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenuStrip?.Dispose();
            _deviceManager.DeviceListChanged -= OnDeviceListChanged;
        }
    }
}