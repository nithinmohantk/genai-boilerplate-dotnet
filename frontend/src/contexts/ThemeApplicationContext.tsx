import React, { createContext, useEffect } from 'react';
import type { ReactNode } from 'react';
import { useThemeApplication } from '../hooks/useThemeApplication';

interface ThemeApplicationContextType {
  previewTheme: (themeId: string) => Promise<void>;
  applyTheme: (themeId: string) => Promise<void>;
  clearPreview: () => void;
  isApplying: boolean;
  appliedTheme: string | null;
}

// Create the context
const ThemeApplicationContext = createContext<ThemeApplicationContextType | undefined>(undefined);

interface ThemeApplicationProviderProps {
  children: ReactNode;
}

export const ThemeApplicationProvider: React.FC<ThemeApplicationProviderProps> = ({ children }) => {
  const {
    isApplying,
    appliedTheme,
    previewThemeById,
    applyTheme: applyThemeHook,
    clearPreview,
    loadSavedTheme,
  } = useThemeApplication();

  // Load saved theme on mount
  useEffect(() => {
    loadSavedTheme();
  }, [loadSavedTheme]);

  const contextValue: ThemeApplicationContextType = {
    previewTheme: previewThemeById,
    applyTheme: applyThemeHook,
    clearPreview,
    isApplying,
    appliedTheme,
  };

  return (
    <ThemeApplicationContext.Provider value={contextValue}>
      {children}
    </ThemeApplicationContext.Provider>
  );
};

export default ThemeApplicationContext;
