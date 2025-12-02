import React, { useState } from 'react';
import { DeviceViewModel, DeviceSettings } from './types';
// import { groupDevices } from './utils/deviceUtils'; // 不要になった
import { Header } from './components/common/Header';
import { DeviceList } from './components/DeviceList/DeviceList';
import { SettingsModal } from './components/SettingsModal/SettingsModal';

// --- Mock Data & Hooks (will be replaced by useDeviceManager hook) ---
const useMockDeviceManager = () => {
    const [devices, setDevices] = useState<DeviceViewModel[]>([]); // Raw device list from C#
    const [globalAutoConnect, setGlobalAutoConnect] = useState(true);
    const [editingDeviceId, setEditingDeviceId] = useState<string | null>(null);
    

    // C#からのデータ更新を受け取るためのグローバル関数
    React.useEffect(() => {
        (window as any).updateDeviceList = (newDevices: DeviceViewModel[]) => {
            console.log("Received devices from C#:", newDevices);
            // バックエンドで集約済みのリストをそのままセットする
            setDevices(newDevices);
        };
        return () => { (window as any).updateDeviceList = null; };
    }, []);

    const toggleMirroring = (id: string) => {
        const device = devices.find(d => d.id === id);
        if (!device) return;

        if (device.isMirroring) {
            (window as any).chrome.webview.hostObjects.bridge.stopMirroring(id);
        } else {
            (window as any).chrome.webview.hostObjects.bridge.startMirroring(id);
        }
    };

    const updateDeviceOrder = (reorderedIds: string[]) => {
        const json = JSON.stringify(reorderedIds);
        (window as any).chrome.webview.hostObjects.bridge.updateDeviceOrder(json);
        
        // Optimistic update for the UI
        const newOrderMap = new Map(reorderedIds.map((id, index) => [id, index]));
        const reorderedDevices = [...devices].sort((a, b) => {
            const indexA = newOrderMap.get(a.id) ?? Infinity;
            const indexB = newOrderMap.get(b.id) ?? Infinity;
            return indexA - indexB;
        });
        setDevices(reorderedDevices);
    };

    const handleSaveSettings = (id: string, newSettings: DeviceSettings) => {
        const json = JSON.stringify(newSettings);
        (window as any).chrome.webview.hostObjects.bridge.updateSettings(id, json);
        setEditingDeviceId(null);
    };

    const handleDeleteDevice = (id: string) => {
        (window as any).chrome.webview.hostObjects.bridge.deleteDevice(id);
        setEditingDeviceId(null);
    };

    const handleSelectSavePath = async (deviceId: string) => {
        const newPath = await (window as any).chrome.webview.hostObjects.bridge.selectSavePath();
        if (newPath) {
            // 現在の編集中の設定を取得して更新する
            const device = devices.find(d => d.id === deviceId);
            if (device) {
                const newSettings = { ...device.settings };
                newSettings.recording.savePath = newPath;
                handleSaveSettings(deviceId, newSettings);
            }
        }
    };

    return {
        devices,
        globalAutoConnect,
        editingDeviceId,
        setGlobalAutoConnect,
        setEditingDeviceId,
        toggleMirroring,
        updateDeviceOrder,
        handleSaveSettings,
        handleDeleteDevice,
        handleSelectSavePath,
    };
};
// --------------------------------------------------------------------


export default function App() {
    const [isDarkMode, setIsDarkMode] = useState(false);
    const {
        devices,
        globalAutoConnect,
        editingDeviceId,
        setGlobalAutoConnect,
        setEditingDeviceId,
        toggleMirroring,
        updateDeviceOrder,
        handleSaveSettings,
        handleDeleteDevice,
        handleSelectSavePath,
    } = useMockDeviceManager();

    return (
        <div className={`flex h-screen w-full font-sans overflow-hidden transition-colors duration-300 ${isDarkMode ? 'bg-slate-900 text-slate-100' : 'bg-slate-50 text-slate-800'}`}>
            <div className="flex-1 flex flex-col h-full overflow-hidden">
                <Header
                    devices={devices}
                    isDarkMode={isDarkMode}
                    globalAutoConnect={globalAutoConnect}
                    onToggleTheme={() => setIsDarkMode(!isDarkMode)}
                    onToggleGlobalAutoConnect={setGlobalAutoConnect}
                />
                <DeviceList
                    devices={devices}
                    isDarkMode={isDarkMode}
                    onToggleMirroring={toggleMirroring}
                    onOpenSettings={setEditingDeviceId}
                    onUpdateDeviceOrder={updateDeviceOrder}
                />
            </div>

            <SettingsModal 
                isOpen={!!editingDeviceId}
                device={devices.find(d => d.id === editingDeviceId) || null}
                onClose={() => setEditingDeviceId(null)}
                onSave={handleSaveSettings}
                onDelete={handleDeleteDevice}
                onSelectSavePath={handleSelectSavePath}
                isDarkMode={isDarkMode}
            />
        </div>
    );
}