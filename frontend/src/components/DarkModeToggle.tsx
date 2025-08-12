import React from 'react';
import { IconButton, Tooltip, Box } from '@mui/material';
import {
  LightMode as LightModeIcon,
  DarkMode as DarkModeIcon,
  SettingsBrightness as AutoModeIcon,
} from '@mui/icons-material';
import { useTheme } from '../contexts/useTheme';

interface DarkModeToggleProps {
  size?: 'small' | 'medium' | 'large';
  showTooltip?: boolean;
}

const DarkModeToggle: React.FC<DarkModeToggleProps> = ({ 
  size = 'medium', 
  showTooltip = true 
}) => {
  const { mode, toggleTheme } = useTheme();

  const getIcon = () => {
    switch (mode) {
      case 'light':
        return <LightModeIcon />;
      case 'dark':
        return <DarkModeIcon />;
      case 'auto':
        return <AutoModeIcon />;
      default:
        return <LightModeIcon />;
    }
  };

  const getTooltipText = () => {
    switch (mode) {
      case 'light':
        return 'Switch to dark mode';
      case 'dark':
        return 'Switch to auto mode';
      case 'auto':
        return 'Switch to light mode';
      default:
        return 'Toggle theme';
    }
  };

  const iconButton = (
    <IconButton
      onClick={toggleTheme}
      color="inherit"
      size={size}
      sx={{
        transition: 'all 0.3s ease-in-out',
        '&:hover': {
          transform: 'rotate(180deg) scale(1.1)',
          bgcolor: 'action.hover',
        },
        '& .MuiSvgIcon-root': {
          transition: 'all 0.3s ease-in-out',
        },
      }}
      aria-label="Toggle theme"
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          transform: mode === 'dark' ? 'rotate(180deg)' : 'rotate(0deg)',
          transition: 'transform 0.3s ease-in-out',
        }}
      >
        {getIcon()}
      </Box>
    </IconButton>
  );

  if (!showTooltip) {
    return iconButton;
  }

  return (
    <Tooltip title={getTooltipText()} arrow>
      {iconButton}
    </Tooltip>
  );
};

export default DarkModeToggle;
