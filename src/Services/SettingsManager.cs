using NL_ScrcpyTray.Models;
using System;
using System.IO;
using System.Text.Json;

namespace NL_ScrcpyTray.Services
{
    /// <summary>
    /// アプリケーション設定の永続化を担当します (settings.json)。
    /// </summary>
    public class SettingsManager
    {
        private readonly string _configPath;

        public SettingsManager(string configPath)
        {
            _configPath = configPath;
            Console.WriteLine($"SettingsManager initialized with config path: {_configPath}");
        }

        /// <summary>
        /// 設定ファイルを読み込みます。ファイルが存在しない場合はデフォルト設定を返します。
        /// </summary>
        public AppSettings Load()
        {
            if (!File.Exists(_configPath))
            {
                return new AppSettings(); // デフォルト設定
            }

            try
            {
                var jsonString = File.ReadAllText(_configPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(jsonString);
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new AppSettings(); // エラー時もデフォルト設定
            }
        }

        /// <summary>
        /// 現在の設定をファイルに保存します。
        /// </summary>
        public void Save(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_configPath, jsonString);
            }
            catch (Exception ex)
            {
                // TODO: Log the exception
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}