import React from 'react';
import { DeviceSettings, RecordingSettings } from '../../types';
import { Info, FolderOpen } from 'lucide-react';
import { Toggle } from '../common/Toggle';

interface RecordTabProps {
    settings: DeviceSettings;
    onChange: (changes: Partial<DeviceSettings>) => void;
    onSelectSavePath: () => void;
    isDarkMode: boolean;
}

export const RecordTab = ({ settings, onChange, onSelectSavePath, isDarkMode }: RecordTabProps) => {

    const handleRecordingChange = (field: keyof RecordingSettings, value: any) => {
        onChange({
            recording: {
                ...settings.recording,
                [field]: value
            }
        });
    };

    return (
        <div className="space-y-8 max-w-2xl mx-auto">
            <div className="p-4 rounded-lg bg-amber-50 text-amber-800 dark:bg-amber-900/30 dark:text-amber-200 text-sm flex gap-3">
                <Info size={20} className="flex-shrink-0" />
                <p>録画・録音はミラーリング開始と同時に開始され、終了時にPC上の指定フォルダに保存されます。</p>
            </div>

            <div className="space-y-4">
                <div className="flex items-center justify-between">
                    <span className="font-medium">画面を録画する</span>
                    <Toggle checked={settings.recording.recordVideo} onChange={(v) => handleRecordingChange('recordVideo', v)} />
                </div>
                
                <div className="flex items-center justify-between">
                    <span className="font-medium">音声を録音する</span>
                    <Toggle checked={settings.recording.recordAudio} onChange={(v) => handleRecordingChange('recordAudio', v)} />
                </div>
            </div>

            <div className={`space-y-6 transition-all ${settings.recording.recordVideo ? 'opacity-100' : 'opacity-40 pointer-events-none'}`}>
                <div className="space-y-2">
                    <label className="text-xs font-bold uppercase opacity-60">保存形式</label>
                    <select
                        title="保存形式"
                        value={settings.recording.recordFormat}
                        onChange={(e) => handleRecordingChange('recordFormat', e.target.value)}
                        className={`w-full p-2.5 rounded border focus:ring-2 focus:ring-indigo-500 outline-none ${isDarkMode ? 'bg-slate-700 border-slate-600' : 'bg-white border-slate-300'}`}
                    >
                        <option value="mp4">MP4</option>
                        <option value="mkv">MKV</option>
                    </select>
                </div>
            </div>

            <div className="space-y-2">
                <label className="text-xs font-bold uppercase opacity-60">保存先フォルダ</label>
                <div className="flex gap-2">
                    <input
                        type="text"
                        title="保存先フォルダ"
                        value={settings.recording.savePath}
                        onChange={(e) => handleRecordingChange('savePath', e.target.value)}
                        className={`flex-1 p-2.5 rounded border focus:ring-2 focus:ring-indigo-500 outline-none font-mono text-sm ${isDarkMode ? 'bg-slate-700 border-slate-600' : 'bg-white border-slate-300'}`}
                    />
                    <button onClick={onSelectSavePath} title="フォルダを選択" className={`p-2.5 rounded border hover:bg-opacity-80 transition-colors ${isDarkMode ? 'bg-slate-700 border-slate-600 hover:bg-slate-600' : 'bg-slate-100 border-slate-300 hover:bg-slate-200'}`}>
                        <FolderOpen size={20} />
                    </button>
                </div>
            </div>
        </div>
    );
};