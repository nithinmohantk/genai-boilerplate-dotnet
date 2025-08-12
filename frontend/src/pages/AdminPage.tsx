import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Tabs,
  Tab,
  Card,
  CardContent,
  Chip,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Alert,
} from '@mui/material';
import {
  AdminPanelSettings as AdminIcon,
  Palette as ThemeIcon,
  Analytics as AnalyticsIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import ThemeSelector from '../components/ThemeSelector';
import { useThemeApplication_Context } from '../contexts/ThemeApplicationContext';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`admin-tabpanel-${index}`}
      aria-labelledby={`admin-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
    </div>
  );
}

// Types
interface Theme {
  id: string;
  name: string;
  display_name: string;
  description: string;
  category: string;
}

// API functions
const fetchThemes = async (): Promise<Theme[]> => {
  const response = await fetch('http://localhost:8000/api/v1/themes/');
  if (!response.ok) throw new Error('Failed to fetch themes');
  return response.json();
};

const fetchCategories = async (): Promise<string[]> => {
  const response = await fetch('http://localhost:8000/api/v1/themes/categories/');
  if (!response.ok) throw new Error('Failed to fetch categories');
  return response.json();
};

const AdminPage: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [selectedTheme, setSelectedTheme] = useState<string>('');
  
  // Use theme application context
  const { 
    previewTheme, 
    applyTheme, 
    clearPreview, 
    isApplying, 
    appliedTheme 
  } = useThemeApplication_Context();

  // Fetch data
  const { data: themes = [], error: themesError } = useQuery({
    queryKey: ['admin-themes'],
    queryFn: fetchThemes,
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['admin-categories'],
    queryFn: fetchCategories,
  });

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, any> = {
      professional: 'primary',
      creative: 'secondary', 
      industry: 'success',
      accessibility: 'warning',
    };
    return colors[category] || 'default';
  };

  // Theme statistics
  const themeStats = {
    total: themes.length,
    byCategory: categories.reduce((acc, cat) => {
      acc[cat] = themes.filter(t => t.category === cat).length;
      return acc;
    }, {} as Record<string, number>),
  };

  if (themesError) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">Failed to load admin data. Please try refreshing.</Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3, maxWidth: 1400, mx: 'auto' }}>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box display="flex" alignItems="center" gap={2}>
          <AdminIcon color="primary" sx={{ fontSize: 40 }} />
          <Box>
            <Typography variant="h4" component="h1">
              Admin Center
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Manage themes, monitor analytics, and configure system settings
            </Typography>
          </Box>
        </Box>
      </Paper>

      {/* Navigation Tabs */}
      <Paper sx={{ mb: 3 }}>
        <Tabs 
          value={tabValue} 
          onChange={handleTabChange}
          aria-label="admin tabs"
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab 
            icon={<ThemeIcon />} 
            label="Theme Management" 
            id="admin-tab-0"
            aria-controls="admin-tabpanel-0"
          />
          <Tab 
            icon={<AnalyticsIcon />} 
            label="Analytics" 
            id="admin-tab-1"
            aria-controls="admin-tabpanel-1"
          />
          <Tab 
            icon={<SettingsIcon />} 
            label="System Settings" 
            id="admin-tab-2"
            aria-controls="admin-tabpanel-2"
          />
        </Tabs>
      </Paper>

      {/* Theme Management Tab */}
      <TabPanel value={tabValue} index={0}>
        <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
          {/* Theme Selector */}
          <Box sx={{ flex: '1 1 400px', minWidth: 400 }}>
            <Card>
              <CardContent>
                <ThemeSelector 
                  onThemeChange={setSelectedTheme}
                />
                
                {/* Theme Actions */}
                <Box sx={{ mt: 3, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                  <Button 
                    variant="outlined" 
                    color="secondary"
                    onClick={clearPreview}
                  >
                    Clear Preview
                  </Button>
                  {selectedTheme && (
                    <Button 
                      variant="contained" 
                      disabled={isApplying}
                      onClick={() => applyTheme(selectedTheme)}
                    >
                      Apply Selected Theme
                    </Button>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Box>

          {/* Theme Statistics */}
          <Box sx={{ flex: '2 1 600px' }}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  📊 Theme Statistics
                </Typography>
                
                <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
                  <Box sx={{ flex: '1 1 150px' }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <Typography variant="h4" color="primary">
                        {themeStats.total}
                      </Typography>
                      <Typography variant="caption">
                        Total Themes
                      </Typography>
                    </Paper>
                  </Box>
                  <Box sx={{ flex: '1 1 150px' }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <Typography variant="h4" color="secondary">
                        {categories.length}
                      </Typography>
                      <Typography variant="caption">
                        Categories
                      </Typography>
                    </Paper>
                  </Box>
                  <Box sx={{ flex: '1 1 150px' }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <Typography variant="h4" color="success.main">
                        {themeStats.byCategory.professional || 0}
                      </Typography>
                      <Typography variant="caption">
                        Professional
                      </Typography>
                    </Paper>
                  </Box>
                  <Box sx={{ flex: '1 1 150px' }}>
                    <Paper sx={{ p: 2, textAlign: 'center' }}>
                      <Typography variant="h4" color="warning.main">
                        {themeStats.byCategory.accessibility || 0}
                      </Typography>
                      <Typography variant="caption">
                        Accessibility
                      </Typography>
                    </Paper>
                  </Box>
                </Box>

                <Typography variant="h6" gutterBottom sx={{ mt: 3 }}>
                  🎨 Available Themes
                </Typography>
                
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Name</TableCell>
                        <TableCell>Category</TableCell>
                        <TableCell>Description</TableCell>
                        <TableCell>Actions</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {themes.map((theme) => (
                        <TableRow 
                          key={theme.id}
                          selected={selectedTheme === theme.id}
                          hover
                        >
                          <TableCell>
                            <Typography variant="body2" fontWeight="medium">
                              {theme.display_name}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={theme.category}
                              size="small"
                              color={getCategoryColor(theme.category)}
                            />
                          </TableCell>
                          <TableCell>
                            <Typography variant="caption" color="text.secondary">
                              {theme.description.substring(0, 50)}...
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', gap: 1 }}>
                              <Button 
                                size="small" 
                                variant="outlined"
                                onClick={() => {
                                  setSelectedTheme(theme.id);
                                  previewTheme(theme.id);
                                }}
                              >
                                Preview
                              </Button>
                              <Button 
                                size="small" 
                                variant="contained"
                                disabled={isApplying}
                                onClick={() => applyTheme(theme.id)}
                              >
                                {appliedTheme === theme.id ? 'Applied' : 'Apply'}
                              </Button>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Box>
        </Box>
      </TabPanel>

      {/* Analytics Tab */}
      <TabPanel value={tabValue} index={1}>
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                📈 Theme Usage Analytics
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Analytics integration coming soon. This will show:
              </Typography>
              <Box component="ul" sx={{ mt: 2 }}>
                <li>Most popular themes</li>
                <li>Theme adoption rates</li>
                <li>User preferences by category</li>
                <li>Performance metrics</li>
                <li>Accessibility compliance scores</li>
              </Box>
            </CardContent>
          </Card>
        </Box>
      </TabPanel>

      {/* System Settings Tab */}
      <TabPanel value={tabValue} index={2}>
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                ⚙️ System Configuration
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Advanced system settings and configuration options.
              </Typography>
              
              <Alert severity="info" sx={{ mt: 2 }}>
                System settings panel will include:
                <ul>
                  <li>Default theme selection</li>
                  <li>Theme caching settings</li>
                  <li>User permission management</li>
                  <li>API rate limiting</li>
                  <li>Database maintenance tools</li>
                </ul>
              </Alert>
            </CardContent>
          </Card>
        </Box>
      </TabPanel>
    </Box>
  );
};

export default AdminPage;
