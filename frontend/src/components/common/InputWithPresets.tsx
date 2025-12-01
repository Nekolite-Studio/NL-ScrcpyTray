import React, { useState, useRef, useEffect } from 'react';
import { ChevronDown } from 'lucide-react';

export const InputWithPresets = ({
  value,
  onChange,
  presets,
  unit = "",
  type = "number",
  isDarkMode,
  label
}: {
  value: string | number,
  onChange: (val: any) => void,
  presets: { label: string, value: any }[],
  unit?: string,
  type?: string,
  isDarkMode: boolean,
  label?: string
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div className="relative w-full" ref={containerRef}>
      <div className="flex">
        <input
          type={type}
          value={value}
          title={label}
          onChange={(e) => onChange(e.target.value)}
          className={`flex-1 min-w-0 p-2.5 rounded-l border focus:ring-2 focus:ring-indigo-500 outline-none ${isDarkMode ? 'bg-slate-700 border-slate-600 text-white' : 'bg-white border-slate-300 text-slate-900'}`}
        />
        {unit && (
           <span className={`flex items-center px-3 border-y ${isDarkMode ? 'bg-slate-800 border-slate-600 text-slate-400' : 'bg-slate-100 border-slate-300 text-slate-500'} text-xs`}>
             {unit}
           </span>
        )}
        <button
          onClick={() => setIsOpen(!isOpen)}
          title={label ? `${label}のプリセットを表示` : "プリセットを表示"}
          className={`px-2 border rounded-r hover:bg-opacity-80 transition-colors ${isDarkMode ? 'bg-slate-700 border-slate-600 hover:bg-slate-600 text-white' : 'bg-slate-100 border-slate-300 hover:bg-slate-200 text-slate-600'}`}
        >
          <ChevronDown size={16} />
        </button>
      </div>
      
      {isOpen && (
        <div className={`absolute z-10 w-full mt-1 rounded shadow-lg max-h-60 overflow-auto border ${isDarkMode ? 'bg-slate-700 border-slate-600' : 'bg-white border-slate-200'}`}>
          {presets.map((preset, idx) => (
            <button
              key={idx}
              onClick={() => {
                onChange(preset.value);
                setIsOpen(false);
              }}
              className={`w-full text-left px-4 py-2 text-sm hover:bg-indigo-50 dark:hover:bg-slate-600 transition-colors ${isDarkMode ? 'text-slate-200' : 'text-slate-700'}`}
            >
              <span className="font-medium">{preset.label}</span>
              {String(preset.value) !== preset.label && <span className="ml-2 text-xs opacity-50">({preset.value})</span>}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};