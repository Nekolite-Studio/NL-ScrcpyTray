import React from 'react';
import { DeviceSettings } from '../../types';
import { Zap } from 'lucide-react';
import { Toggle } from '../common/Toggle';

interface GeneralTabProps {
    settings: DeviceSettings;
    onChange: (changes: Partial<DeviceSettings>) => void;
    isDarkMode: boolean;
}

export const GeneralTab = ({ settings, onChange, isDarkMode }: GeneralTabProps) => {
    return (
        <div className="space-y-8 max-w-2xl mx-auto">
            <div className="space-y-6">
                <h3 className="text-sm font-bold uppercase tracking-wider opacity-60 border-b pb-2 dark:border-slate-700">自動接続</h3>
                
                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-medium">自動接続を有効にする</div>
                    <div className="text-xs opacity-60 mt-1">デバイス検出時に自動的に接続を試みます</div>
                  </div>
                  <Toggle checked={settings.autoConnect} onChange={(v) => onChange({ autoConnect: v })} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-medium">無線への自動切り替え</div>
                    <div className="text-xs opacity-60 mt-1">ミラーリング中、USBが切断されると自動で無線接続に切り替えます</div>
                  </div>
                  <Toggle checked={settings.autoSwitchToWifi} onChange={(v) => onChange({ autoSwitchToWifi: v })} />
                </div>

                <div className="flex items-center justify-between">
                  <div>
                    <div className="font-medium">有線を優先</div>
                    <div className="text-xs opacity-60 mt-1">USBと無線両方で接続されている場合、USBを優先します</div>
                  </div>
                  <Toggle checked={settings.autoSwitchToUsb} onChange={(v) => onChange({ autoSwitchToUsb: v })} />
                </div>
            </div>

            <div className="space-y-6">
                <h3 className="text-sm font-bold uppercase tracking-wider opacity-60 border-b pb-2 dark:border-slate-700">プロファイル管理</h3>
                <div className="flex items-center justify-between p-4 rounded-lg bg-indigo-50 dark:bg-indigo-900/20 border border-indigo-100 dark:border-indigo-900/50">
                    <div>
                        <div className="font-medium flex items-center gap-2 text-indigo-900 dark:text-indigo-200">
                            <Zap size={18} className="text-amber-500" />
                            有線/無線で別設定にする
                        </div>
                        <div className="text-xs opacity-70 mt-1 text-indigo-800 dark:text-indigo-300">
                            有効にすると、映像・音声設定を有線時と無線時で個別に保存できます。
                        </div>
                    </div>
                    <Toggle checked={settings.separateSettings} onChange={(v) => onChange({ separateSettings: v })} />
                </div>
            </div>
        </div>
    );
};