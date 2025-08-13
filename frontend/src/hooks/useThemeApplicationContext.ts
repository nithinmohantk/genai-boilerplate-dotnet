import { useContext } from 'react';
import ThemeApplicationContext from '../contexts/ThemeApplicationContext';

interface ThemeApplicationContextType {
  previewTheme: (themeId: string) => Promise<void>;
  applyTheme: (themeId: string) => Promise<void>;
  clearPreview: () => void;
  isApplying: boolean;
  appliedTheme: string | null;
}

// Custom hook to use the theme application context
export const useThemeApplication_Context = (): ThemeApplicationContextType => {
  const context = useContext(ThemeApplicationContext);
  if (!context) {
    throw new Error('useThemeApplication must be used within a ThemeApplicationProvider');
  }
  return context;
};
