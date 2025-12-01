import React from 'react';
import { DeviceViewModel } from '../../types';
import { Trash2 } from 'lucide-react';

interface InfoTabProps {
    device: DeviceViewModel;
    onDelete: (id: string) => void;
}

export const InfoTab = ({ device, onDelete }: InfoTabProps) => {
    return (
        <div className="space-y-8 max-w-2xl mx-auto">
            <div className="space-y-4 p-6 rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50">
                <div className="grid grid-cols-[120px_1fr] gap-y-4 text-sm">
                    <div className="opacity-60 dark:text-slate-400">デバイス名</div>
                    <div className="font-mono font-bold text-slate-900 dark:text-white">{device.name}</div>
                    
                    <div className="opacity-60 dark:text-slate-400">モデル</div>
                    <div className="text-slate-700 dark:text-slate-200">{device.model}</div>

                    <div className="opacity-60 dark:text-slate-400">シリアルID</div>
                    <div className="font-mono text-slate-700 dark:text-slate-200">{device.serial}</div>

                    <div className="opacity-60 dark:text-slate-400">IPアドレス</div>
                    <div className="font-mono text-slate-700 dark:text-slate-200">{device.ipAddress || '(未接続)'}</div>
                    
                    <div className="opacity-60 dark:text-slate-400">接続状態</div>
                    <div className="flex items-center gap-2 font-bold text-slate-900 dark:text-white">
                        {device.status}
                    </div>
                </div>
            </div>

            <div className="pt-6 border-t border-red-200 dark:border-red-900/50">
                <h3 className="text-red-600 font-bold mb-2 flex items-center gap-2">
                    <Trash2 size={18} /> Danger Zone
                </h3>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-4">このデバイスをリストから削除します。すべての設定は破棄されます。</p>
                <button
                    onClick={() => onDelete(device.id)}
                    className="w-full py-3 bg-red-50 text-red-600 border border-red-200 rounded-lg hover:bg-red-100 dark:bg-red-900/20 dark:text-red-400 dark:border-red-900/50 dark:hover:bg-red-900/40 transition-colors font-bold"
                >
                    デバイスを削除
                </button>
            </div>
        </div>
    );
};