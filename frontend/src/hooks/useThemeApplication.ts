import { useState, useCallback, useEffect } from 'react';
import { createTheme, type Theme } from '@mui/material/styles';
import { useTheme as useThemeContext } from '../contexts/useTheme';
import { globalThemeManager } from '../utils/globalThemeManager';

// Types for backend theme data
export interface BackendTheme {
  id: string;
  name: string;
  display_name: string;
  description: string;
  category: string;
  supports_dark_mode: boolean;
  accessibility_features?: Record<string, any>;
  css_variables?: Record<string, any>;
  color_scheme?: {
    light: {
      primary: string;
      secondary: string;
      accent?: string;
      background: string;
      surface?: string;
      surface_variant?: string;
      text: string;
      text_secondary?: string;
      text_muted?: string;
      border?: string;
      success?: string;
      warning?: string;
      error?: string;
      info?: string;
      // Special theme-specific properties
      elevation_1?: string;
      elevation_2?: string;
      elevation_3?: string;
      glass_overlay?: string;
      backdrop_filter?: string;
      chrome_accent?: string;
      contrast_surface?: string;
      contrast_text?: string;
      nature_accent_1?: string;
      nature_accent_2?: string;
      creative_accent_1?: string;
      creative_accent_2?: string;
      medical_urgent?: string;
      medical_normal?: string;
      profit?: string;
      loss?: string;
      neutral?: string;
      forest_green?: string;
      lime_green?: string;
      mint_green?: string;
      sage_green?: string;
      emerald_green?: string;
      code_bg?: string;
      code_border?: string;
      // Add any other color properties as optional
      [key: string]: string | undefined;
    };
    dark: {
      primary: string;
      secondary: string;
      accent?: string;
      background: string;
      surface?: string;
      surface_variant?: string;
      text: string;
      text_secondary?: string;
      text_muted?: string;
      border?: string;
      success?: string;
      warning?: string;
      error?: string;
      info?: string;
      // Special theme-specific properties
      elevation_1?: string;
      elevation_2?: string;
      elevation_3?: string;
      glass_overlay?: string;
      backdrop_filter?: string;
      chrome_accent?: string;
      contrast_surface?: string;
      contrast_text?: string;
      nature_accent_1?: string;
      nature_accent_2?: string;
      creative_accent_1?: string;
      creative_accent_2?: string;
      medical_urgent?: string;
      medical_normal?: string;
      profit?: string;
      loss?: string;
      neutral?: string;
      forest_green?: string;
      lime_green?: string;
      mint_green?: string;
      sage_green?: string;
      emerald_green?: string;
      code_bg?: string;
      code_border?: string;
      // Add any other color properties as optional
      [key: string]: string | undefined;
    };
  };
}

// Custom hook for theme application
export const useThemeApplication = () => {
  const [isApplying, setIsApplying] = useState(false);
  const [appliedTheme, setAppliedTheme] = useState<string | null>(null);
  const [previewTheme, setPreviewTheme] = useState<Theme | null>(null);
  const [currentThemeData, setCurrentThemeData] = useState<BackendTheme | null>(null);
  const themeContext = useThemeContext();

  // Fetch theme details from backend
  const fetchThemeDetails = useCallback(async (themeId: string): Promise<BackendTheme> => {
    const response = await fetch(`http://localhost:8000/api/v1/themes/${themeId}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch theme: ${themeId}`);
    }
    return response.json();
  }, []);

  // Convert backend theme to MUI theme
  const createMUITheme = useCallback((backendTheme: BackendTheme, isDark: boolean): Theme => {
    const colorScheme = backendTheme.color_scheme;
    if (!colorScheme) {
      // Fallback to default theme if no color scheme
      return createTheme({ palette: { mode: isDark ? 'dark' : 'light' } });
    }

    const scheme = isDark ? colorScheme.dark : colorScheme.light;
    const cssVariables = backendTheme.css_variables || {};
    
    return createTheme({
      palette: {
        mode: isDark ? 'dark' : 'light',
        primary: {
          main: scheme.primary,
          light: scheme.accent || (isDark ? '#8b5cf6' : '#60a5fa'),
          dark: isDark ? '#4f46e5' : '#2563eb',
        },
        secondary: {
          main: scheme.secondary,
          light: isDark ? '#c084fc' : '#a78bfa',
          dark: isDark ? '#9333ea' : '#7c3aed',
        },
        background: {
          default: scheme.background,
          paper: scheme.surface || (isDark ? '#1e293b' : '#ffffff'),
        },
        text: {
          primary: scheme.text,
          secondary: scheme.text_secondary || (isDark ? '#94a3b8' : '#64748b'),
        },
        divider: scheme.border || (isDark ? '#334155' : '#e2e8f0'),
        success: {
          main: scheme.success || '#4caf50',
        },
        warning: {
          main: scheme.warning || '#ff9800',
        },
        error: {
          main: scheme.error || '#f44336',
        },
        info: {
          main: scheme.info || '#2196f3',
        },
        action: {
          hover: isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.04)',
          selected: isDark ? 'rgba(255, 255, 255, 0.12)' : 'rgba(0, 0, 0, 0.08)',
        },
      },
      typography: {
        fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      },
      shape: {
        borderRadius: parseInt(cssVariables['border-radius']?.replace('px', '')) || 12,
      },
      components: {
        MuiCssBaseline: {
          styleOverrides: {
            body: {
              backgroundColor: scheme.background,
              color: scheme.text,
              transition: 'background-color 0.3s ease-in-out, color 0.3s ease-in-out',
              // Add glass morphism backdrop filter for glass themes
              ...(scheme.backdrop_filter && {
                backdropFilter: scheme.backdrop_filter,
              }),
            },
            '#root': {
              minHeight: '100vh',
              backgroundColor: scheme.background,
              color: scheme.text,
            },
          },
        },
        MuiButton: {
          styleOverrides: {
            root: {
              textTransform: 'none',
              fontWeight: 500,
              borderRadius: cssVariables['border-radius'] || '8px',
              transition: 'all 0.2s ease-in-out',
              boxShadow: cssVariables.shadow || '0 1px 3px rgba(0, 0, 0, 0.1)',
              '&:hover': {
                boxShadow: cssVariables['shadow-lg'] || '0 4px 12px rgba(0, 0, 0, 0.15)',
              },
            },
            contained: {
              // Add glossy overlay for glossy themes
              ...(cssVariables['glossy-overlay'] && {
                backgroundImage: cssVariables['glossy-overlay'],
              }),
            },
          },
        },
        MuiPaper: {
          styleOverrides: {
            root: {
              backgroundColor: scheme.surface || (isDark ? '#1e293b' : '#ffffff'),
              transition: 'background-color 0.3s ease-in-out, box-shadow 0.3s ease-in-out',
              backgroundImage: 'none',
              boxShadow: cssVariables.shadow || '0 1px 3px rgba(0, 0, 0, 0.1)',
              // Add glass effects for glass themes
              ...(cssVariables['backdrop-filter'] && {
                backdropFilter: cssVariables['backdrop-filter'],
                backgroundColor: scheme.glass_overlay || scheme.surface,
                border: cssVariables['glass-border'],
              }),
            },
            elevation1: {
              backgroundColor: scheme.elevation_1 ? `rgba(${scheme.elevation_1})` : undefined,
            },
            elevation2: {
              backgroundColor: scheme.elevation_2 ? `rgba(${scheme.elevation_2})` : undefined,
            },
            elevation3: {
              backgroundColor: scheme.elevation_3 ? `rgba(${scheme.elevation_3})` : undefined,
            },
          },
        },
        MuiCard: {
          styleOverrides: {
            root: {
              backgroundColor: scheme.surface_variant || scheme.surface || (isDark ? '#1e293b' : '#ffffff'),
              transition: 'all 0.3s ease-in-out',
              borderRadius: cssVariables['border-radius'] || '12px',
              boxShadow: cssVariables.shadow || '0 2px 8px rgba(0, 0, 0, 0.1)',
              '&:hover': {
                boxShadow: cssVariables['shadow-lg'] || '0 8px 24px rgba(0, 0, 0, 0.15)',
              },
              // Add glossy effects for modern themes
              ...(cssVariables['glossy-overlay'] && {
                '&::before': {
                  content: '""',
                  position: 'absolute',
                  top: 0,
                  left: 0,
                  right: 0,
                  bottom: 0,
                  background: cssVariables['glossy-overlay'],
                  borderRadius: cssVariables['border-radius'] || '12px',
                  pointerEvents: 'none',
                  zIndex: 1,
                },
              }),
            },
          },
        },
        MuiAppBar: {
          styleOverrides: {
            root: {
              backgroundColor: scheme.surface || scheme.primary,
              color: scheme.contrast_text || scheme.text,
              boxShadow: cssVariables.shadow || '0 1px 3px rgba(0, 0, 0, 0.1)',
            },
          },
        },
        MuiChip: {
          styleOverrides: {
            root: {
              backgroundColor: scheme.surface_variant || (isDark ? '#374151' : '#f3f4f6'),
              color: scheme.text_secondary || scheme.text,
            },
          },
        },
      },
    });
  }, []);

  // Recreate current backend theme when base theme mode changes
  const recreateCurrentTheme = useCallback(async (newIsDark: boolean) => {
    const appliedThemeId = localStorage.getItem('applied-theme');
    const previewThemeId = localStorage.getItem('preview-theme');
    const currentThemeId = previewThemeId || appliedThemeId;
    
    if (currentThemeId) {
      try {
        // If we have currentThemeData, use it, otherwise fetch it
        let themeData = currentThemeData;
        if (!themeData) {
          themeData = await fetchThemeDetails(currentThemeId);
          setCurrentThemeData(themeData);
        }
        
        const muiTheme = createMUITheme(themeData, newIsDark);
        setPreviewTheme(muiTheme);
        
        // Dispatch event to update the main theme provider
        window.dispatchEvent(new CustomEvent('backend-theme-change', {
          detail: { theme: muiTheme, themeId: currentThemeId }
        }));
        
        console.log(`Recreated ${themeData.display_name} for ${newIsDark ? 'dark' : 'light'} mode`);
      } catch (error) {
        console.error('Failed to recreate theme for new mode:', error);
        // If recreation fails, clear the backend theme
        setPreviewTheme(null);
        setCurrentThemeData(null);
        window.dispatchEvent(new CustomEvent('backend-theme-clear'));
      }
    }
  }, [currentThemeData, createMUITheme, fetchThemeDetails]);

  // Listen for base theme mode changes
  useEffect(() => {
    const handleBaseModeChange = (event: CustomEvent) => {
      console.log(`Received base mode change event: ${event.detail.isDark ? 'dark' : 'light'}`);
      recreateCurrentTheme(event.detail.isDark);
    };

    window.addEventListener('base-theme-mode-change', handleBaseModeChange as EventListener);
    console.log('Added base-theme-mode-change listener');
    
    return () => {
      window.removeEventListener('base-theme-mode-change', handleBaseModeChange as EventListener);
      console.log('Removed base-theme-mode-change listener');
    };
  }, [recreateCurrentTheme]);

  // Preview a theme temporarily
  const previewThemeById = useCallback(async (themeId: string) => {
    try {
      setIsApplying(true);
      const themeData = await fetchThemeDetails(themeId);
      const muiTheme = createMUITheme(themeData, themeContext.isDark);
      setPreviewTheme(muiTheme);
      setCurrentThemeData(themeData); // Store theme data for mode switching
      
      // Notify global theme manager
      globalThemeManager.setCurrentTheme(themeId, themeData);
      
      // Don't set as applied theme - this is just a preview
      // setAppliedTheme(themeId);
      
      // Store the preview in localStorage for persistence
      localStorage.setItem('preview-theme', themeId);
      localStorage.removeItem('applied-theme'); // Remove applied theme when previewing
      
      // Dispatch custom event to update the main theme provider
      window.dispatchEvent(new CustomEvent('backend-theme-change', {
        detail: { theme: muiTheme, themeId: themeId }
      }));
      
      console.log(`Previewing theme: ${themeData.display_name}`);
    } catch (error) {
      console.error('Failed to preview theme:', error);
    } finally {
      setIsApplying(false);
    }
  }, [fetchThemeDetails, createMUITheme, themeContext.isDark]);

  // Apply a theme permanently
  const applyTheme = useCallback(async (themeId: string) => {
    try {
      setIsApplying(true);
      const themeData = await fetchThemeDetails(themeId);
      const muiTheme = createMUITheme(themeData, themeContext.isDark);
      setPreviewTheme(muiTheme);
      setCurrentThemeData(themeData); // Store theme data for mode switching
      setAppliedTheme(themeId);
      
      // Notify global theme manager
      globalThemeManager.setCurrentTheme(themeId, themeData);
      
      // Store the applied theme in localStorage
      localStorage.setItem('applied-theme', themeId);
      localStorage.removeItem('preview-theme');
      
      // Dispatch custom event to update the main theme provider
      window.dispatchEvent(new CustomEvent('backend-theme-change', {
        detail: { theme: muiTheme, themeId: themeId }
      }));
      
      console.log(`Applied theme: ${themeData.display_name}`);
    } catch (error) {
      console.error('Failed to apply theme:', error);
    } finally {
      setIsApplying(false);
    }
  }, [fetchThemeDetails, createMUITheme, themeContext.isDark]);

  // Clear preview and revert to default
  const clearPreview = useCallback(() => {
    setPreviewTheme(null);
    setCurrentThemeData(null);
    setAppliedTheme(null);
    localStorage.removeItem('preview-theme');
    localStorage.removeItem('applied-theme');
    
    // Notify global theme manager
    globalThemeManager.clearCurrentTheme();
    
    // Dispatch custom event to clear the theme
    window.dispatchEvent(new CustomEvent('backend-theme-clear'));
    
    console.log('Cleared theme preview');
  }, []);

  // Load saved theme on initialization
  const loadSavedTheme = useCallback(async () => {
    const appliedThemeId = localStorage.getItem('applied-theme');
    const previewThemeId = localStorage.getItem('preview-theme');
    
    const themeToLoad = previewThemeId || appliedThemeId;
    if (themeToLoad) {
      try {
        const themeData = await fetchThemeDetails(themeToLoad);
        const muiTheme = createMUITheme(themeData, themeContext.isDark);
        setPreviewTheme(muiTheme);
        setCurrentThemeData(themeData); // Store theme data for mode switching
        setAppliedTheme(themeToLoad);
        
        // Notify global theme manager
        globalThemeManager.setCurrentTheme(themeToLoad, themeData);
        
        // Dispatch event to update the main theme provider
        window.dispatchEvent(new CustomEvent('backend-theme-change', {
          detail: { theme: muiTheme, themeId: themeToLoad }
        }));
      } catch (error) {
        console.error('Failed to load saved theme:', error);
        // Clear invalid theme from storage
        localStorage.removeItem('applied-theme');
        localStorage.removeItem('preview-theme');
      }
    }
  }, [fetchThemeDetails, createMUITheme, themeContext.isDark]);

  return {
    isApplying,
    appliedTheme,
    previewTheme,
    previewThemeById,
    applyTheme,
    clearPreview,
    loadSavedTheme,
  };
};
