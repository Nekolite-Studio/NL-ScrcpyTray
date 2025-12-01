import React, { useState, useEffect } from 'react';
import {
  Smartphone,
  Settings,
  X,
  CheckCircle,
  Video,
  Mic,
  FileVideo,
  Info,
  Cable,
  Wifi
} from 'lucide-react';
import { DeviceViewModel, DeviceSettings } from '../../types';
import { GeneralTab } from './GeneralTab';
import { VideoTab } from './VideoTab';
import { AudioTab } from './AudioTab';
import { RecordTab } from './RecordTab';
import { InfoTab } from './InfoTab';

export const SettingsModal = ({ 
  device, 
  isOpen, 
  onClose, 
  onSave,
  onDelete,
  onSelectSavePath,
  isDarkMode
}: {
  device: DeviceViewModel | null,
  isOpen: boolean,
  onClose: () => void,
  onSave: (id: string, newSettings: DeviceSettings) => void,
  onDelete: (id: string) => void,
  onSelectSavePath: (deviceId: string) => void,
  isDarkMode: boolean
}) => {
  const [activeTab, setActiveTab] = useState<'general' | 'video' | 'audio' | 'record' | 'info'>('general');
  const [localSettings, setLocalSettings] = useState<DeviceSettings | null>(null);
  const [editingProfile, setEditingProfile] = useState<'usbProfile' | 'wifiProfile'>('usbProfile');

  useEffect(() => {
    if (device && isOpen) {
      setLocalSettings(JSON.parse(JSON.stringify(device.settings)));
      setActiveTab('general');
      setEditingProfile('usbProfile');
    }
  }, [device, isOpen]);

  if (!isOpen || !device || !localSettings) return null;

  const handleSettingsChange = (changes: Partial<DeviceSettings>) => {
    setLocalSettings(prev => prev ? { ...prev, ...changes } : null);
  };
  
  const tabs = [
    { id: 'general', label: '一般', icon: Settings },
    { id: 'video', label: '映像', icon: Video },
    { id: 'audio', label: '音声', icon: Mic },
    { id: 'record', label: '録画', icon: FileVideo },
    { id: 'info', label: '情報', icon: Info },
  ] as const;

  const getTabClass = (id: string) => `col-start-1 row-start-1 transition-opacity duration-300 ${activeTab === id ? 'opacity-100 z-10' : 'opacity-0 invisible z-0'}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm animate-fade-in p-4">
      <div className={`w-[800px] max-h-[90vh] rounded-2xl shadow-2xl flex flex-col ${isDarkMode ? 'bg-slate-800 text-slate-100' : 'bg-white text-slate-800'}`}>
        
        {/* Header */}
        <div className={`flex items-center justify-between px-6 py-4 border-b flex-shrink-0 ${isDarkMode ? 'border-slate-700' : 'border-slate-200'}`}>
          <div className="flex items-center gap-3">
            <Smartphone className="text-indigo-500" />
            <div>
              <h2 className="text-lg font-bold leading-tight">設定: {device.name}</h2>
              <p className={`text-xs ${isDarkMode ? 'text-slate-400' : 'text-slate-500'}`}>{device.serial}</p>
            </div>
          </div>
          <button onClick={onClose} title="閉じる" className={`p-2 rounded-full hover:bg-slate-100 dark:hover:bg-slate-700 transition-colors`}>
            <X size={20} />
          </button>
        </div>

        {/* Tabs */}
        <div className={`flex px-6 pt-2 border-b gap-6 flex-shrink-0 ${isDarkMode ? 'border-slate-700' : 'border-slate-200'}`}>
          {tabs.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`pb-3 text-sm font-medium flex items-center gap-2 border-b-2 transition-colors ${
                activeTab === tab.id 
                  ? 'border-indigo-500 text-indigo-500' 
                  : 'border-transparent text-slate-400 hover:text-slate-600 dark:hover:text-slate-300'
              }`}
            >
              <tab.icon size={16} />
              {tab.label}
            </button>
          ))}
        </div>

        {/* Content Area */}
        <div className="flex-1 overflow-y-auto custom-scrollbar p-8">
          {/* プロファイル切り替え (共通UI) */}
          {localSettings.separateSettings && (activeTab === 'video' || activeTab === 'audio') && (
            <div className="flex justify-center mb-6">
              <div className={`flex p-1 rounded-lg ${isDarkMode ? 'bg-slate-700' : 'bg-slate-100'}`}>
                <button
                  onClick={() => setEditingProfile('usbProfile')}
                  className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all flex items-center gap-2 ${
                    editingProfile === 'usbProfile'
                      ? 'bg-white text-indigo-600 shadow-sm dark:bg-slate-600 dark:text-white'
                      : 'text-slate-500 hover:text-slate-700 dark:text-slate-400'
                  }`}
                >
                  <Cable size={14} /> 有線設定
                </button>
                <button
                  onClick={() => setEditingProfile('wifiProfile')}
                  className={`px-4 py-1.5 rounded-md text-sm font-medium transition-all flex items-center gap-2 ${
                    editingProfile === 'wifiProfile'
                      ? 'bg-white text-indigo-600 shadow-sm dark:bg-slate-600 dark:text-white'
                      : 'text-slate-500 hover:text-slate-700 dark:text-slate-400'
                  }`}
                >
                  <Wifi size={14} /> 無線設定
                </button>
              </div>
            </div>
          )}
          <div className="grid grid-cols-1">
            <div className={getTabClass('general')}>
              <GeneralTab settings={localSettings} onChange={handleSettingsChange} isDarkMode={isDarkMode} />
            </div>
            <div className={getTabClass('video')}>
              <VideoTab settings={localSettings} onChange={handleSettingsChange} isDarkMode={isDarkMode} editingProfile={editingProfile} setEditingProfile={setEditingProfile} />
            </div>
            <div className={getTabClass('audio')}>
              <AudioTab settings={localSettings} onChange={handleSettingsChange} isDarkMode={isDarkMode} editingProfile={editingProfile} setEditingProfile={setEditingProfile} />
            </div>
            <div className={getTabClass('record')}>
              <RecordTab settings={localSettings} onChange={handleSettingsChange} onSelectSavePath={() => onSelectSavePath(device.id)} isDarkMode={isDarkMode} />
            </div>
            <div className={getTabClass('info')}>
              <InfoTab device={device} onDelete={onDelete} />
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className={`p-4 border-t flex justify-end gap-3 flex-shrink-0 ${isDarkMode ? 'border-slate-700 bg-slate-800/50' : 'border-slate-200 bg-slate-50'}`}>
          <button 
            onClick={onClose}
            className={`px-4 py-2 rounded-lg font-medium text-sm transition-colors ${isDarkMode ? 'hover:bg-slate-700 text-slate-300' : 'hover:bg-slate-200 text-slate-600'}`}
          >
            キャンセル
          </button>
          <button 
            onClick={() => onSave(device.id, localSettings)}
            className="px-6 py-2 rounded-lg font-medium text-sm bg-indigo-600 hover:bg-indigo-700 text-white shadow-lg shadow-indigo-500/30 transition-all flex items-center gap-2"
          >
            <CheckCircle size={16} />
            設定を保存
          </button>
        </div>
      </div>
    </div>
  );
};