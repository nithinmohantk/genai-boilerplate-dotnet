import { createTheme } from '@mui/material/styles';
import type { Theme, PaletteMode } from '@mui/material';

// Define theme mode type
export type ThemeMode = 'light' | 'dark' | 'auto';

// Define custom theme configurations
export const getThemeConfig = (mode: PaletteMode): Theme => {
  const isDark = mode === 'dark';

  return createTheme({
    palette: {
      mode,
      primary: {
        main: isDark ? '#6366f1' : '#3b82f6',
        light: isDark ? '#8b5cf6' : '#60a5fa',
        dark: isDark ? '#4f46e5' : '#2563eb',
      },
      secondary: {
        main: isDark ? '#a855f7' : '#8b5cf6',
        light: isDark ? '#c084fc' : '#a78bfa',
        dark: isDark ? '#9333ea' : '#7c3aed',
      },
      background: {
        default: isDark ? '#0f172a' : '#f8fafc',
        paper: isDark ? '#1e293b' : '#ffffff',
      },
      text: {
        primary: isDark ? '#f1f5f9' : '#1e293b',
        secondary: isDark ? '#94a3b8' : '#64748b',
      },
      divider: isDark ? '#334155' : '#e2e8f0',
      action: {
        hover: isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.04)',
        selected: isDark ? 'rgba(255, 255, 255, 0.12)' : 'rgba(0, 0, 0, 0.08)',
      },
      error: {
        main: isDark ? '#f87171' : '#ef4444',
        light: isDark ? '#fca5a5' : '#f87171',
        dark: isDark ? '#dc2626' : '#dc2626',
      },
      warning: {
        main: isDark ? '#fbbf24' : '#f59e0b',
        light: isDark ? '#fcd34d' : '#fbbf24',
        dark: isDark ? '#d97706' : '#d97706',
      },
      info: {
        main: isDark ? '#60a5fa' : '#3b82f6',
        light: isDark ? '#93c5fd' : '#60a5fa',
        dark: isDark ? '#2563eb' : '#1d4ed8',
      },
      success: {
        main: isDark ? '#34d399' : '#10b981',
        light: isDark ? '#6ee7b7' : '#34d399',
        dark: isDark ? '#059669' : '#047857',
      },
    },
    typography: {
      fontFamily: 'Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
      h1: {
        fontWeight: 700,
        fontSize: '2.25rem',
        lineHeight: 1.2,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      h2: {
        fontWeight: 600,
        fontSize: '1.875rem',
        lineHeight: 1.3,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      h3: {
        fontWeight: 600,
        fontSize: '1.5rem',
        lineHeight: 1.4,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      h4: {
        fontWeight: 600,
        fontSize: '1.25rem',
        lineHeight: 1.4,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      h5: {
        fontWeight: 600,
        fontSize: '1.125rem',
        lineHeight: 1.4,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      h6: {
        fontWeight: 600,
        fontSize: '1rem',
        lineHeight: 1.4,
        color: isDark ? '#f1f5f9' : '#1e293b',
      },
      body1: {
        fontSize: '1rem',
        lineHeight: 1.6,
        color: isDark ? '#e2e8f0' : '#334155',
      },
      body2: {
        fontSize: '0.875rem',
        lineHeight: 1.6,
        color: isDark ? '#cbd5e1' : '#475569',
      },
    },
    shape: {
      borderRadius: 12,
    },
    // Use default Material-UI shadows to avoid array index issues
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundColor: isDark ? '#0f172a' : '#f8fafc',
            color: isDark ? '#f1f5f9' : '#1e293b',
            transition: 'background-color 0.3s ease-in-out, color 0.3s ease-in-out',
            scrollbarWidth: 'thin',
            scrollbarColor: isDark ? '#475569 #1e293b' : '#cbd5e1 #f8fafc',
            '&::-webkit-scrollbar': {
              width: '8px',
            },
            '&::-webkit-scrollbar-track': {
              backgroundColor: isDark ? '#1e293b' : '#f8fafc',
            },
            '&::-webkit-scrollbar-thumb': {
              backgroundColor: isDark ? '#475569' : '#cbd5e1',
              borderRadius: '4px',
              '&:hover': {
                backgroundColor: isDark ? '#64748b' : '#94a3b8',
              },
            },
          },
          '#root': {
            minHeight: '100vh',
            backgroundColor: isDark ? '#0f172a' : '#f8fafc',
            color: isDark ? '#f1f5f9' : '#1e293b',
          },
        },
      },
      MuiButton: {
        styleOverrides: {
          root: {
            textTransform: 'none',
            fontWeight: 500,
            borderRadius: '8px',
            transition: 'all 0.2s ease-in-out',
            '&:hover': {
              transform: 'translateY(-1px)',
              boxShadow: isDark
                ? '0 4px 12px rgba(0, 0, 0, 0.3)'
                : '0 4px 12px rgba(0, 0, 0, 0.1)',
            },
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            transition: 'background-color 0.3s ease-in-out, box-shadow 0.3s ease-in-out',
            backgroundImage: 'none',
          },
        },
      },
      MuiAppBar: {
        styleOverrides: {
          root: {
            transition: 'background-color 0.3s ease-in-out, color 0.3s ease-in-out',
          },
        },
      },
      MuiDrawer: {
        styleOverrides: {
          paper: {
            transition: 'background-color 0.3s ease-in-out',
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            transition: 'all 0.3s ease-in-out',
            '&:hover': {
              transform: 'translateY(-2px)',
              boxShadow: isDark
                ? '0 8px 25px rgba(0, 0, 0, 0.4)'
                : '0 8px 25px rgba(0, 0, 0, 0.1)',
            },
          },
        },
      },
      MuiTextField: {
        styleOverrides: {
          root: {
            '& .MuiOutlinedInput-root': {
              transition: 'all 0.2s ease-in-out',
              '&:hover .MuiOutlinedInput-notchedOutline': {
                borderColor: isDark ? '#64748b' : '#94a3b8',
              },
            },
          },
        },
      },
      MuiListItem: {
        styleOverrides: {
          root: {
            transition: 'all 0.2s ease-in-out',
            '&:hover': {
              transform: 'translateX(4px)',
            },
          },
        },
      },
    },
  });
};

// Utility function to get system theme preference
export const getSystemTheme = (): PaletteMode => {
  if (typeof window !== 'undefined' && window.matchMedia) {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
  return 'light';
};

// Utility function to get the effective palette mode
export const getEffectiveMode = (mode: ThemeMode): PaletteMode => {
  if (mode === 'auto') {
    return getSystemTheme();
  }
  return mode as PaletteMode;
};
