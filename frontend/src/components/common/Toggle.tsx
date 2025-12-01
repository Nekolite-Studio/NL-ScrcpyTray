import React from 'react';

export const Toggle = ({ 
  checked, 
  onChange, 
  disabled = false,
  label = ''
}: { 
  checked: boolean, 
  onChange: (checked: boolean) => void, 
  disabled?: boolean,
  label?: string
}) => (
  <div 
    className={`flex items-center gap-3 ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}`}
    onClick={() => !disabled && onChange(!checked)}
  >
    <div className={`
      relative w-11 h-6 rounded-full transition-colors duration-200 ease-in-out flex items-center
      ${checked ? 'bg-indigo-600' : 'bg-slate-300 dark:bg-slate-600'}
    `}>
      <span className={`
        block w-4 h-4 bg-white rounded-full shadow-sm transform transition-transform duration-200 ease-in-out ml-1
        ${checked ? 'translate-x-5' : 'translate-x-0'}
      `} />
    </div>
    {label && <span className="text-sm font-medium">{label}</span>}
  </div>
);