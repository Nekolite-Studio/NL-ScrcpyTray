import React from 'react';
import { Monitor, Sun, Moon, PauseCircle } from 'lucide-react';
import { DeviceViewModel } from '../../types';
// import { Toggle } from './Toggle'; // Will be created later

// --- 仮のコンポーネント ---
const Toggle = ({ checked, onChange }: { checked: boolean, onChange: (c: boolean) => void }) => (
    <div onClick={() => onChange(!checked)}>
        <input type="checkbox" checked={checked} readOnly />
    </div>
);
// -------------------------

interface HeaderProps {
    devices: DeviceViewModel[];
    isDarkMode: boolean;
    globalAutoConnect: boolean;
    onToggleTheme: () => void;
    onToggleGlobalAutoConnect: (enabled: boolean) => void;
}

export const Header = ({
    devices,
    isDarkMode,
    globalAutoConnect,
    onToggleTheme,
    onToggleGlobalAutoConnect
}: HeaderProps) => {
    // devicesがundefinedの場合でも空配列として扱い、エラーを防ぐ
    const safeDevices = devices ?? [];
    const connectedCount = safeDevices.filter(d => d.status !== 'Offline').length;
    const mirroringCount = safeDevices.filter(d => d.isMirroring).length;

    return (
        <header className={`border-b px-6 py-4 flex items-center justify-between shadow-sm z-10 transition-colors duration-300 ${isDarkMode ? 'bg-slate-800 border-slate-700' : 'bg-white border-slate-200'}`}>
            <div className="flex items-center gap-3">
                <div className="bg-indigo-600 p-2 rounded-lg">
                    <Monitor className="text-white" size={24} />
                </div>
                <div>
                    <h1 className={`text-xl font-bold tracking-tight ${isDarkMode ? 'text-white' : 'text-slate-800'}`}>Scrcpy Manager</h1>
                    <p className={`text-xs ${isDarkMode ? 'text-slate-400' : 'text-slate-500'}`}>軽量マルチデバイス管理</p>
                </div>
            </div>
          
            <div className="flex items-center gap-6">
                <div className={`flex items-center gap-2 px-3 py-1.5 rounded-lg border ${isDarkMode ? 'bg-slate-900/50 border-slate-700' : 'bg-slate-50 border-slate-200'}`}>
                    <span className={`text-xs font-bold uppercase tracking-wider ${isDarkMode ? 'text-slate-400' : 'text-slate-500'}`}>
                        一括自動接続
                    </span>
                    <div 
                        className={`flex items-center gap-1 transition-colors ${globalAutoConnect ? 'text-green-500' : 'text-slate-400'}`}
                        title="全てのデバイスの自動接続を一時的に無効化します"
                    >
                        {globalAutoConnect ? <PauseCircle size={20} className="hidden" /> : <PauseCircle size={20} />}
                        <Toggle checked={globalAutoConnect} onChange={onToggleGlobalAutoConnect} />
                    </div>
                </div>

                <div className={`h-6 w-px mx-2 ${isDarkMode ? 'bg-slate-700' : 'bg-slate-200'}`}></div>

                <div className="hidden md:flex gap-2">
                    <span className={`text-xs font-medium px-3 py-1.5 rounded-full flex items-center gap-2 ${isDarkMode ? 'bg-slate-700 text-slate-300' : 'bg-slate-100 text-slate-500'}`}>
                        <div className="w-2 h-2 rounded-full bg-green-500"></div>
                        接続中: {connectedCount}
                    </span>
                    <span className={`text-xs font-medium px-3 py-1.5 rounded-full flex items-center gap-2 ${isDarkMode ? 'bg-slate-700 text-slate-300' : 'bg-slate-100 text-slate-500'}`}>
                        <div className="w-2 h-2 rounded-full bg-indigo-500 animate-pulse"></div>
                        ミラーリング: {mirroringCount}
                    </span>
                </div>

                <button 
                    onClick={onToggleTheme}
                    className={`p-2 rounded-full transition-colors ${isDarkMode ? 'text-yellow-400 hover:bg-slate-700' : 'text-slate-600 hover:bg-slate-100'}`}
                >
                    {isDarkMode ? <Sun size={20} /> : <Moon size={20} />}
                </button>
            </div>
        </header>
    );
};