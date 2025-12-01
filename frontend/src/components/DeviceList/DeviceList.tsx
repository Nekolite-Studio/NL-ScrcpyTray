import React from 'react';
import { DeviceViewModel } from '../../types';
import { DeviceListItem } from './DeviceListItem';
import { Smartphone } from 'lucide-react';

interface DeviceListProps {
    devices: DeviceViewModel[];
    isDarkMode: boolean;
    onToggleMirroring: (id: string) => void;
    onOpenSettings: (id: string) => void;
    onUpdateDeviceOrder: (reorderedDevices: DeviceViewModel[]) => void;
}

export const DeviceList = ({ 
    devices, 
    isDarkMode, 
    onToggleMirroring, 
    onOpenSettings,
    onUpdateDeviceOrder
}: DeviceListProps) => {
    
    const dragItem = React.useRef<number | null>(null);
    const dragOverItem = React.useRef<number | null>(null);

    const handleDragStart = (e: React.DragEvent<HTMLDivElement>, index: number) => {
        dragItem.current = index;
        e.dataTransfer.effectAllowed = "move";
    };

    const handleDragEnter = (e: React.DragEvent<HTMLDivElement>, index: number) => {
        dragOverItem.current = index;
    };

    const handleDragEnd = () => {
        const sourceIndex = dragItem.current;
        const destinationIndex = dragOverItem.current;
        if (sourceIndex !== null && destinationIndex !== null && sourceIndex !== destinationIndex) {
            const _devices = [...devices];
            const draggedItemContent = _devices[sourceIndex];
            _devices.splice(sourceIndex, 1);
            _devices.splice(destinationIndex, 0, draggedItemContent);
            onUpdateDeviceOrder(_devices);
        }
        dragItem.current = null;
        dragOverItem.current = null;
    };

    const listGridTemplate = "32px 48px 1.5fr 120px 2fr 160px";

    return (
        <main className="flex-1 overflow-y-auto p-6">
            <div className="flex flex-col gap-3 max-w-5xl mx-auto">
                {devices.length > 0 && (
                    <div 
                        className={`grid gap-4 px-4 text-xs font-semibold uppercase tracking-wider mb-1 ${isDarkMode ? 'text-slate-500' : 'text-slate-400'}`}
                        style={{ gridTemplateColumns: listGridTemplate }}
                    >
                        <div className="text-center">#</div>
                        <div></div>
                        <div>デバイス</div>
                        <div>接続</div>
                        <div>設定概要</div>
                        <div className="text-right pr-2">操作</div>
                    </div>
                )}
                
                {devices.map((device, idx) => (
                    <DeviceListItem 
                        key={device.id} 
                        device={device} 
                        index={idx} 
                        isDarkMode={isDarkMode}
                        onDragStart={handleDragStart}
                        onDragEnter={handleDragEnter}
                        onDragEnd={handleDragEnd}
                        onToggleMirroring={onToggleMirroring}
                        onOpenSettings={onOpenSettings}
                    />
                ))}

                {devices.length === 0 && (
                    <div className={`flex flex-col items-center justify-center py-20 border-2 border-dashed rounded-xl ${isDarkMode ? 'border-slate-700 text-slate-600' : 'border-slate-200 text-slate-400'}`}>
                        <Smartphone size={48} className="mb-4 opacity-50" />
                        <p>デバイスがありません</p>
                        <p className="text-sm">USBケーブルを接続してください</p>
                    </div>
                )}
            </div>
        </main>
    );
};