import React, { useState, useEffect } from 'react';
import {
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  type SelectChangeEvent,
  Box,
  Typography,
  Chip,
  CircularProgress,
  Alert,
  Button,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { useThemeApplication_Context } from '../contexts/ThemeApplicationContext';

// Types for theme data
interface Theme {
  id: string;
  name: string;
  display_name: string;
  description: string;
  category: string;
}

interface ThemeSelectorProps {
  onThemeChange?: (themeId: string) => void;
}

// API functions
const fetchThemes = async (): Promise<Theme[]> => {
  const response = await fetch('http://localhost:8000/api/v1/themes/');
  if (!response.ok) {
    throw new Error('Failed to fetch themes');
  }
  return response.json();
};

const fetchCategories = async (): Promise<string[]> => {
  const response = await fetch('http://localhost:8000/api/v1/themes/categories/');
  if (!response.ok) {
    throw new Error('Failed to fetch categories');
  }
  return response.json();
};

const ThemeSelector: React.FC<ThemeSelectorProps> = ({ onThemeChange }) => {
  const [selectedTheme, setSelectedTheme] = useState<string>('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  
  // Use theme application context
  const { 
    previewTheme, 
    applyTheme, 
    isApplying, 
    appliedTheme 
  } = useThemeApplication_Context();

  // Fetch themes and categories
  const { 
    data: themes = [], 
    isLoading: themesLoading, 
    error: themesError 
  } = useQuery({
    queryKey: ['themes'],
    queryFn: fetchThemes,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const { 
    data: categories = [], 
    isLoading: categoriesLoading 
  } = useQuery({
    queryKey: ['theme-categories'],
    queryFn: fetchCategories,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });

  // Filter themes by category for display

  const handleThemeChange = (event: SelectChangeEvent) => {
    const themeId = event.target.value;
    setSelectedTheme(themeId);
    onThemeChange?.(themeId);
    
    // Store selected theme in localStorage
    localStorage.setItem('selected-theme', themeId);
  };

  const handleCategoryChange = (event: SelectChangeEvent) => {
    setSelectedCategory(event.target.value);
  };

  // Load saved theme from localStorage on mount
  useEffect(() => {
    const savedTheme = localStorage.getItem('selected-theme');
    if (savedTheme && themes.some(theme => theme.id === savedTheme)) {
      setSelectedTheme(savedTheme);
      onThemeChange?.(savedTheme);
    }
  }, [themes, onThemeChange]);

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      professional: 'primary',
      creative: 'secondary', 
      industry: 'success',
      accessibility: 'warning',
    };
    return colors[category] || 'default';
  };

  if (themesLoading || categoriesLoading) {
    return (
      <Box display="flex" alignItems="center" gap={1}>
        <CircularProgress size={20} />
        <Typography variant="body2">Loading themes...</Typography>
      </Box>
    );
  }

  if (themesError) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        Failed to load themes. Please try refreshing the page.
      </Alert>
    );
  }

  return (
    <Box sx={{ minWidth: 300 }}>
      <Typography variant="h6" gutterBottom>
        Theme Selection
      </Typography>
      
      {/* Category Filter */}
      <FormControl fullWidth margin="normal" size="small">
        <InputLabel>Category</InputLabel>
        <Select
          value={selectedCategory}
          label="Category"
          onChange={handleCategoryChange}
        >
          <MenuItem value="all">All Categories</MenuItem>
          {categories.map(category => (
            <MenuItem key={category} value={category}>
              {category.charAt(0).toUpperCase() + category.slice(1)}
            </MenuItem>
          ))}
        </Select>
      </FormControl>

      {/* Theme Selector */}
      <FormControl fullWidth margin="normal">
        <InputLabel>Professional Theme</InputLabel>
        <Select
          value={selectedTheme}
          label="Professional Theme"
          onChange={handleThemeChange}
          MenuProps={{
            PaperProps: {
              style: {
                maxHeight: 400,
              },
            },
          }}
        >
          {/* Get filtered themes based on category */}
          {(() => {
            const filteredThemes = selectedCategory === 'all' 
              ? themes 
              : themes.filter(theme => theme.category === selectedCategory);
            
            return filteredThemes.map(theme => (
              <MenuItem key={theme.id} value={theme.id}>
                {theme.display_name} - {theme.description.substring(0, 40)}...
              </MenuItem>
            ));
          })()}
        </Select>
      </FormControl>

      {/* Theme Info */}
      {selectedTheme && (
        <Box sx={{ mt: 2, p: 2, bgcolor: 'background.paper', borderRadius: 1 }}>
          {(() => {
            const theme = themes.find(t => t.id === selectedTheme);
            return theme ? (
              <Box>
                <Box display="flex" alignItems="center" gap={1} mb={1}>
                  <Typography variant="subtitle1">
                    {theme.display_name}
                  </Typography>
                  <Chip 
                    label={theme.category} 
                    size="small" 
                    color={getCategoryColor(theme.category) as any}
                  />
                </Box>
                <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                  {theme.description}
                </Typography>
                
                {/* Theme Actions */}
                <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
                  <Button 
                    variant="outlined" 
                    size="small"
                    onClick={() => previewTheme(selectedTheme)}
                  >
                    Preview
                  </Button>
                  <Button 
                    variant="contained" 
                    size="small"
                    disabled={isApplying}
                    onClick={() => applyTheme(selectedTheme)}
                  >
                    {appliedTheme === selectedTheme ? 'Applied' : 'Apply'}
                  </Button>
                </Box>
              </Box>
            ) : null;
          })()}
        </Box>
      )}

      <Typography variant="caption" color="textSecondary" sx={{ mt: 1, display: 'block' }}>
        Choose from {themes.length} professional themes across {categories.length} categories
      </Typography>
    </Box>
  );
};

export default ThemeSelector;
