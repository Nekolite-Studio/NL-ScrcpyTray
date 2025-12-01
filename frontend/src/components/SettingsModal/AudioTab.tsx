import React from 'react';
import { DeviceSettings, ConnectionProfile } from '../../types';
import { Mic } from 'lucide-react';
import { InputWithPresets } from '../common/InputWithPresets';
import { Toggle } from '../common/Toggle';

interface AudioTabProps {
    settings: DeviceSettings;
    onChange: (changes: Partial<DeviceSettings>) => void;
    isDarkMode: boolean;
    editingProfile: 'usbProfile' | 'wifiProfile';
    setEditingProfile: React.Dispatch<React.SetStateAction<'usbProfile' | 'wifiProfile'>>;
}

export const AudioTab = ({ settings, onChange, isDarkMode, editingProfile, setEditingProfile }: AudioTabProps) => {
    
    const handleProfileChange = (field: keyof ConnectionProfile, value: any) => {
        const targetProfile = editingProfile;
        
        onChange({
            [targetProfile]: {
                ...settings[targetProfile],
                [field]: value
            }
        });

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

            <div className={`flex items-center justify-between p-4 rounded-lg border ${isDarkMode ? 'border-slate-700 bg-slate-800' : 'border-slate-200 bg-slate-50'}`}>
                <div className="flex items-center gap-3">
                    <Mic size={20} className={currentProfileData.audioEnabled ? 'text-indigo-500' : 'text-slate-400'} />
                    <span className="font-medium">音声を転送する</span>
                </div>
                <Toggle checked={currentProfileData.audioEnabled} onChange={(v) => handleProfileChange('audioEnabled', v)} />
            </div>

            <div className={`space-y-6 transition-all ${currentProfileData.audioEnabled ? 'opacity-100' : 'opacity-40 pointer-events-none filter grayscale'}`}>
                <div className="grid grid-cols-2 gap-x-8 gap-y-6">
                    <div className="space-y-2">
                        <label className="text-xs font-bold uppercase opacity-60">音声ビットレート</label>
                        <InputWithPresets
                            value={currentProfileData.audioBitrate}
                            onChange={(v: any) => handleProfileChange('audioBitrate', Number(v))}
                            unit="Kbps"
                            label="音声ビットレート"
                            presets={[
                                { label: '128 Kbps (標準)', value: 128 },
                                { label: '256 Kbps (高音質)', value: 256 },
                            ]}
                            isDarkMode={isDarkMode}
                        />
                    </div>
                    <div className="space-y-2">
                        <label className="text-xs font-bold uppercase opacity-60">音声コーデック</label>
                        <select
                          title="音声コーデック"
                          value={currentProfileData.audioCodec}
                          onChange={(e) => handleProfileChange('audioCodec', e.target.value)}
                          className={`w-full p-2.5 rounded border focus:ring-2 focus:ring-indigo-500 outline-none ${isDarkMode ? 'bg-slate-700 border-slate-600' : 'bg-white border-slate-300'}`}
                        >
                            <option value="opus">Opus (推奨)</option>
                            <option value="aac">AAC</option>
                            <option value="raw">RAW</option>
                        </select>
                    </div>
                    <div className="space-y-2 col-span-2">
                        <label className="text-xs font-bold uppercase opacity-60">音声バッファ時間</label>
                        <div className="flex items-center gap-4">
                            <div className="flex-1">
                                <InputWithPresets
                                    value={currentProfileData.audioBuffer}
                                    onChange={(v: any) => handleProfileChange('audioBuffer', Number(v))}
                                    unit="ms"
                                    label="音声バッファ時間"
                                    presets={[
                                        { label: '50 ms (推奨)', value: 50 },
                                        { label: '100 ms', value: 100 },
                                        { label: '200 ms (安定)', value: 200 },
                                    ]}
                                    isDarkMode={isDarkMode}
                                />
                            </div>
                            <div className="text-xs opacity-50 w-48">
                                値を上げると遅延が増えますが、音飛びが減ります
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};