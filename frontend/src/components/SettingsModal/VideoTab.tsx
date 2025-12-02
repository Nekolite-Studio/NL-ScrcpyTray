import React from 'react';
import { DeviceSettings, ConnectionProfile } from '../../types';
import { Video, Monitor } from 'lucide-react';
import { InputWithPresets } from '../common/InputWithPresets';
import { Toggle } from '../common/Toggle';


interface VideoTabProps {
    settings: DeviceSettings;
    onChange: (changes: Partial<DeviceSettings>) => void;
    isDarkMode: boolean;
    editingProfile: 'usbProfile' | 'wifiProfile';
    setEditingProfile: React.Dispatch<React.SetStateAction<'usbProfile' | 'wifiProfile'>>;
}

export const VideoTab = ({ settings, onChange, isDarkMode, editingProfile, setEditingProfile }: VideoTabProps) => {

    const handleProfileChange = (field: keyof ConnectionProfile, value: any) => {
        const targetProfile = editingProfile;
        
        onChange({
            [targetProfile]: {
                ...settings[targetProfile],
                [field]: value
            }
        });

        // Eğer ayarlar ayrılmamışsa, diğer profili de senkronize et
        if (!settings.separateSettings) {
            const otherProfile = targetProfile === 'usbProfile' ? 'wifiProfile' : 'usbProfile';
            onChange({
                [otherProfile]: {
                    ...settings[otherProfile],
                    [field]: value
                }
            });
        }
    };

    const currentProfileData = settings[editingProfile];

    return (
        <div className="space-y-6 max-w-2xl mx-auto">
            {!settings.separateSettings && (
                <div className="text-xs text-center opacity-50 mb-4">- 共通設定編集中 -</div>
            )}

            <div className="space-y-4">
                <div className={`flex items-center justify-between p-4 rounded-lg border ${isDarkMode ? 'border-slate-700 bg-slate-800' : 'border-slate-200 bg-slate-50'}`}>
                    <div className="flex items-center gap-3">
                        <Video size={20} className={currentProfileData.videoEnabled ? 'text-indigo-500' : 'text-slate-400'} />
                        <span className="font-medium">映像を転送する</span>
                    </div>
                    <Toggle checked={currentProfileData.videoEnabled} onChange={(v) => handleProfileChange('videoEnabled', v)} />
                </div>
                <div className={`flex items-center justify-between p-4 rounded-lg border ${isDarkMode ? 'border-slate-700 bg-slate-800' : 'border-slate-200 bg-slate-50'}`}>
                    <div className="flex items-center gap-3">
                        <Monitor size={20} className={currentProfileData.displayEnabled ? 'text-indigo-500' : 'text-slate-400'} />
                        <span className="font-medium">ウィンドウを表示する</span>
                    </div>
                    <Toggle checked={currentProfileData.displayEnabled} onChange={(v) => handleProfileChange('displayEnabled', v)} />
                </div>
            </div>
            
            <div className={`grid grid-cols-2 gap-x-8 gap-y-6 transition-all ${currentProfileData.videoEnabled ? 'opacity-100' : 'opacity-40 pointer-events-none filter grayscale'}`}>
                <div className="space-y-2">
                    <label className="text-xs font-bold uppercase opacity-60">映像ビットレート</label>
                    <InputWithPresets
                        value={currentProfileData.videoBitrate}
                        onChange={(v: any) => handleProfileChange('videoBitrate', Number(v))}
                        unit="Mbps"
                        label="映像ビットレート"
                        presets={[
                            { label: '4 Mbps (バランス)', value: 4 },
                            { label: '8 Mbps (高画質)', value: 8 },
                            { label: '16 Mbps (最高画質)', value: 16 },
                        ]}
                        isDarkMode={isDarkMode}
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-xs font-bold uppercase opacity-60">最大FPS</label>
                    <InputWithPresets
                        value={currentProfileData.maxFps}
                        onChange={(v: any) => handleProfileChange('maxFps', Number(v))}
                        unit="fps"
                        label="最大FPS"
                        presets={[
                            { label: '30 fps', value: 30 },
                            { label: '60 fps', value: 60 },
                            { label: '無制限 (0)', value: 0 },
                        ]}
                        isDarkMode={isDarkMode}
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-xs font-bold uppercase opacity-60">最大長辺解像度</label>
                    <InputWithPresets
                        value={currentProfileData.maxSize}
                        onChange={(v: any) => handleProfileChange('maxSize', Number(v))}
                        unit="px"
                        label="最大長辺解像度"
                        presets={[
                            { label: 'オリジナル (0)', value: 0 },
                            { label: '1920 px (FHD)', value: 1920 },
                            { label: '1280 px (HD)', value: 1280 },
                        ]}
                        isDarkMode={isDarkMode}
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-xs font-bold uppercase opacity-60">映像コーデック</label>
                    <select
                      title="映像コーデック"
                      value={currentProfileData.videoCodec}
                      onChange={(e) => handleProfileChange('videoCodec', e.target.value)}
                      className={`w-full p-2.5 rounded border focus:ring-2 focus:ring-indigo-500 outline-none ${isDarkMode ? 'bg-slate-700 border-slate-600' : 'bg-white border-slate-300'}`}
                    >
                        <option value="h264">H.264 (推奨)</option>
                        <option value="h265">H.265 (HEVC)</option>
                        <option value="av1">AV1</option>
                    </select>
                </div>

                <div className="space-y-2 col-span-2">
                    <label className="text-xs font-bold uppercase opacity-60">バッファ時間</label>
                    <div className="flex items-center gap-4">
                        <div className="flex-1">
                            <InputWithPresets
                                value={currentProfileData.videoBuffer}
                                onChange={(v: any) => handleProfileChange('videoBuffer', Number(v))}
                                unit="ms"
                                label="バッファ時間"
                                presets={[
                                    { label: '0 ms (低遅延)', value: 0 },
                                    { label: '50 ms (推奨)', value: 50 },
                                    { label: '200 ms (安定)', value: 200 },
                                ]}
                                isDarkMode={isDarkMode}
                            />
                        </div>
                        <div className="text-xs opacity-50 w-48">
                            値を上げると遅延が増えますが、カクつきが減ります
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};