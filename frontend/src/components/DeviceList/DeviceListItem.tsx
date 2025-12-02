import React from 'react';
import { DeviceViewModel, ConnectionStatus } from '../../types';
import { Smartphone, Settings, Play, Power, GripVertical, Monitor, Gauge, Zap, Wifi, Usb } from 'lucide-react';

interface DeviceListItemProps {
    device: DeviceViewModel;
    index: number;
    isDarkMode: boolean;
    onDragStart: (e: React.DragEvent<HTMLDivElement>, index: number) => void;
    onDragEnter: (e: React.DragEvent<HTMLDivElement>, index: number) => void;
    onDragEnd: () => void;
    onToggleMirroring: (id: string) => void;
    onOpenSettings: (id: string) => void;
}

const ConnectionStatusDisplay = ({ status, isDarkMode }: { status: ConnectionStatus, isDarkMode: boolean }) => {
    let text = 'オフライン';
    let colorClass = isDarkMode ? 'text-slate-400' : 'text-slate-500';
    let bgColorClass = 'bg-slate-400';

    switch (status) {
        case 'UsbAndWifi':
            text = 'USB+Wi-Fi';
            colorClass = isDarkMode ? 'text-amber-400' : 'text-amber-600';
            bgColorClass = 'bg-amber-500';
            break;
        case 'Usb':
            text = 'USB';
            colorClass = isDarkMode ? 'text-emerald-400' : 'text-emerald-500';
            bgColorClass = 'bg-emerald-500';
            break;
        case 'Wifi':
            text = 'Wi-Fi';
            colorClass = isDarkMode ? 'text-sky-400' : 'text-sky-500';
            bgColorClass = 'bg-sky-500';
            break;
    }

    return (
        <div className={`flex items-center gap-1.5 font-medium ${colorClass}`}>
            <div className={`w-1.5 h-1.5 rounded-full ${bgColorClass}`} />
            {text}
        </div>
    );
};


export const DeviceListItem = ({
    device,
    index,
    isDarkMode,
    onDragStart,
    onDragEnter,
    onDragEnd,
    onToggleMirroring,
    onOpenSettings
}: DeviceListItemProps) => {
    // バックエンドから集約されたViewModelを直接使うため、representativeDeviceは不要
    const activeProfile = (device.status === 'Wifi' && device.settings.separateSettings)
        ? device.settings.wifiProfile
        : device.settings.usbProfile;
        
    const listGridTemplate = "32px 48px 1.5fr 120px 2fr 160px";
    const isOffline = device.status === 'Offline';

    return (
        <div
            draggable
            onDragStart={(e) => onDragStart(e, index)}
            onDragEnter={(e) => onDragEnter(e, index)}
            onDragEnd={onDragEnd}
            onDragOver={(e) => e.preventDefault()}
            className={`
                grid gap-4 px-4 py-3 items-center group rounded-lg border transition-all duration-200 cursor-default
                ${isDarkMode ? 'bg-slate-800 border-slate-700 hover:border-indigo-500' : 'bg-white border-slate-200 hover:border-indigo-300 hover:shadow-md'}
                ${device.isMirroring ? 'border-indigo-500 dark:bg-indigo-900/10 bg-indigo-50/10' : ''}
                ${isOffline ? 'opacity-60 grayscale-[0.5]' : ''}
            `}
            style={{ gridTemplateColumns: listGridTemplate }}
        >
            {/* Drag Handle */}
            <div className={`flex justify-center cursor-grab active:cursor-grabbing p-1 ${isDarkMode ? 'text-slate-600 hover:text-slate-300' : 'text-slate-300 hover:text-slate-500'}`}>
                <GripVertical size={20} />
            </div>

            {/* Icon */}
            <div className={`
                w-10 h-10 rounded-lg flex items-center justify-center relative
                ${isOffline ? 'bg-slate-100 text-slate-400 dark:bg-slate-700 dark:text-slate-500' : ''}
                ${device.status === 'UsbAndWifi' ? 'bg-amber-100 text-amber-600 dark:bg-amber-900/30 dark:text-amber-400' : ''}
                ${device.status === 'Usb' ? 'bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400' : ''}
                ${device.status === 'Wifi' ? 'bg-sky-100 text-sky-600 dark:bg-sky-900/30 dark:text-sky-400' : ''}
            `}>
                <Smartphone size={20} />
                <div className="absolute -top-1 -left-1 w-4 h-4 bg-indigo-600 text-white rounded-full flex items-center justify-center text-[10px] font-bold shadow-sm border border-white dark:border-slate-800">
                    {index + 1}
                </div>
            </div>

            {/* Identity */}
            <div className="min-w-0">
                <h3 className={`font-bold leading-tight truncate ${isDarkMode ? 'text-white' : 'text-slate-800'}`}>{device.name}</h3>
                {device.ipAddress && (
                    <p className={`text-xs font-mono mt-0.5 truncate ${isDarkMode ? 'text-slate-400' : 'text-slate-400'}`}>{device.ipAddress}</p>
                )}
            </div>

            {/* Status */}
            <div className="text-sm">
                <ConnectionStatusDisplay status={device.status} isDarkMode={isDarkMode} />
            </div>

            {/* Settings Summary */}
            <div className="flex items-center gap-4">
                <div className="flex flex-col gap-1 min-w-[80px]">
                    <div className={`flex items-center gap-1.5 text-[11px] font-medium transition-colors ${device.settings.autoConnect ? 'text-blue-600 dark:text-blue-400' : 'text-slate-300 dark:text-slate-600'}`}>
                        <Zap size={12} fill={device.settings.autoConnect ? "currentColor" : "none"} />
                        <span>自動接続</span>
                    </div>
                    <div className={`flex items-center gap-1.5 text-[11px] font-medium transition-colors ${device.settings.autoSwitchToWifi ? 'text-purple-600 dark:text-purple-400' : 'text-slate-300 dark:text-slate-600'}`}>
                        <Wifi size={12} />
                        <span>無線化</span>
                    </div>
                </div>
                <div className="w-px h-6 bg-slate-200 dark:bg-slate-700"></div>
                <div className="flex flex-col gap-1">
                    <div className="flex items-center gap-1.5 text-[11px] font-medium text-slate-600 dark:text-slate-300">
                        <Monitor size={12} />
                        <span>{activeProfile.maxFps === 0 ? 'No Limit' : `${activeProfile.maxFps} fps`}</span>
                    </div>
                    <div className="flex items-center gap-1.5 text-[11px] font-medium text-slate-500 dark:text-slate-400">
                        <Gauge size={12} />
                        <span>{activeProfile.videoBitrate} Mbps</span>
                    </div>
                </div>
            </div>

            {/* Primary Actions */}
            <div className="flex items-center gap-2 justify-end">
                <button
                    onClick={() => onToggleMirroring(device.id)}
                    disabled={isOffline}
                    className={`
                        flex items-center gap-2 py-1.5 px-3 rounded-lg font-bold text-sm transition-all min-w-[100px] justify-center
                        ${device.isMirroring
                            ? 'bg-red-50 text-red-600 hover:bg-red-100 border border-red-200 dark:bg-red-900/20 dark:text-red-400 dark:border-red-900/50'
                            : 'bg-indigo-600 text-white hover:bg-indigo-700 shadow-sm disabled:bg-slate-100 disabled:text-slate-400 disabled:shadow-none dark:disabled:bg-slate-700 dark:disabled:text-slate-600'}
                    `}
                >
                    {device.isMirroring ? <><Power size={14} /> 停止</> : <><Play size={14} /> 開始</>}
                </button>
                <button
                    onClick={() => onOpenSettings(device.id)}
                    className={`p-2 rounded-lg transition-colors border ${isDarkMode ? 'border-slate-700 text-slate-400 hover:text-white hover:bg-slate-700' : 'border-slate-200 text-slate-500 hover:text-indigo-600 hover:bg-indigo-50'}`}
                    title="詳細設定"
                >
                    <Settings size={18} />
                </button>
            </div>
        </div>
    );
};