import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Switch,
  FormControlLabel,
  Button,
  Alert,
  Slider,
  Card,
  CardContent,
  Chip,
} from '@mui/material';
import {
  Save as SaveIcon,
  RestoreFromTrash as ResetIcon,
  LightMode as LightModeIcon,
  DarkMode as DarkModeIcon,
  SettingsBrightness as AutoModeIcon,
} from '@mui/icons-material';
import { useTheme as useCustomTheme } from '../contexts/useTheme';
import DarkModeToggle from '../components/DarkModeToggle';
import ThemeSelector from '../components/ThemeSelector';

interface Settings {
  // AI Model Settings
  aiProvider: string;
  model: string;
  temperature: number;
  maxTokens: number;
  
  // RAG Settings
  ragEnabled: boolean;
  retrievalType: string;
  documentsToRetrieve: number;
  scoreThreshold: number;
  
  // Chat Settings
  maxChatHistory: number;
  systemMessage: string;
  
  // UI Settings
  theme: string;
  autoScroll: boolean;
  showTimestamps: boolean;
}

const SettingsPage: React.FC = () => {
  const { mode: currentTheme, setTheme, isDark } = useCustomTheme();
  
  const [settings, setSettings] = useState<Settings>({
    aiProvider: 'openai',
    model: 'gpt-3.5-turbo',
    temperature: 0.7,
    maxTokens: 1000,
    
    ragEnabled: true,
    retrievalType: 'similarity',
    documentsToRetrieve: 5,
    scoreThreshold: 0.7,
    
    maxChatHistory: 10,
    systemMessage: 'You are a helpful AI assistant. Use the provided context to answer questions accurately and helpfully.',
    
    theme: 'light',
    autoScroll: true,
    showTimestamps: true,
  });

  const [saveSuccess, setSaveSuccess] = useState(false);

  const handleInputChange = (field: keyof Settings, value: string | number | boolean) => {
    setSettings(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = () => {
    // Simulate save operation
    console.log('Saving settings:', settings);
    setSaveSuccess(true);
    setTimeout(() => setSaveSuccess(false), 3000);
  };

  const handleReset = () => {
    setSettings({
      aiProvider: 'openai',
      model: 'gpt-3.5-turbo',
      temperature: 0.7,
      maxTokens: 1000,
      
      ragEnabled: true,
      retrievalType: 'similarity',
      documentsToRetrieve: 5,
      scoreThreshold: 0.7,
      
      maxChatHistory: 10,
      systemMessage: 'You are a helpful AI assistant. Use the provided context to answer questions accurately and helpfully.',
      
      theme: 'light',
      autoScroll: true,
      showTimestamps: true,
    });
  };

  const aiProviders = [
    { value: 'openai', label: 'OpenAI' },
    { value: 'anthropic', label: 'Anthropic' },
    { value: 'google', label: 'Google (Gemini)' },
    { value: 'azure', label: 'Azure OpenAI' },
    { value: 'huggingface', label: 'Hugging Face' },
  ];

  const modelsByProvider: Record<string, string[]> = {
    openai: [
      // GPT-4 Models
      'gpt-4',
      'gpt-4-turbo', 
      'gpt-4o',
      'gpt-4o-mini',
      'gpt-4.1',
      'gpt-4.1-mini',
      'gpt-4.1-nano',
      // GPT-3.5 Models
      'gpt-3.5-turbo',
      'gpt-3.5-turbo-16k',
      // GPT-5 Models ✨ Latest Release
      'gpt-5',
      'gpt-5.0-mini',
      'gpt-5.0-nano',
      // Open Source
      'gpt-oss-20b'
    ],
    anthropic: [
      // Claude 3 Models
      'claude-3-haiku-20240307',
      'claude-3-sonnet-20240229', 
      'claude-3-opus-20240229',
      // Claude 3.5 Models
      'claude-3-5-sonnet-20241022',
      'claude-3-5-haiku-20241022',
      // Claude 3.7 Models ✨ Latest Release
      'claude-3-7-sonnet',
      // Claude 4.0 Models ✨ Latest Release
      'claude-4-0-opus',
      'claude-4-0-sonnet'
    ],
    google: [
      // Gemini 1.5 Models
      'gemini-1.5-flash',
      'gemini-1.5-pro',
      // Gemini 2.5 Models (Latest)
      'gemini-2.5-flash', 
      'gemini-2.5-pro',
      // Legacy Models
      'gemini-pro',
      'gemini-pro-vision'
    ],
    azure: [
      'gpt-4',
      'gpt-4-turbo',
      'gpt-35-turbo'
    ],
    huggingface: [
      'microsoft/DialoGPT-medium',
      'microsoft/DialoGPT-large',
      'meta-llama/Llama-2-7b-chat-hf',
      'meta-llama/Llama-2-13b-chat-hf',
      'meta-llama/Llama-2-70b-chat-hf',
      'mistralai/Mistral-7B-Instruct-v0.1'
    ],
  };

  return (
    <Box sx={{ p: 3, maxWidth: 1200, mx: 'auto' }}>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Settings
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Configure AI models, RAG parameters, and application preferences
        </Typography>
      </Paper>

      {/* Save Success Alert */}
      {saveSuccess && (
        <Alert severity="success" sx={{ mb: 3 }}>
          Settings saved successfully!
        </Alert>
      )}

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 3 }}>
        {/* AI Model Settings */}
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                AI Model Configuration
              </Typography>
              
              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>AI Provider</InputLabel>
                <Select
                  value={settings.aiProvider}
                  onChange={(e) => handleInputChange('aiProvider', e.target.value)}
                  label="AI Provider"
                >
                  {aiProviders.map(provider => (
                    <MenuItem key={provider.value} value={provider.value}>
                      {provider.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl fullWidth sx={{ mb: 2 }}>
                <InputLabel>Model</InputLabel>
                <Select
                  value={settings.model}
                  onChange={(e) => handleInputChange('model', e.target.value)}
                  label="Model"
                >
                  {(modelsByProvider[settings.aiProvider] || []).map(model => (
                    <MenuItem key={model} value={model}>
                      {model}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <Box sx={{ mb: 2 }}>
                <Typography gutterBottom>
                  Temperature: {settings.temperature}
                </Typography>
                <Slider
                  value={settings.temperature}
                  onChange={(_, value) => handleInputChange('temperature', value)}
                  min={0}
                  max={2}
                  step={0.1}
                  marks={[
                    { value: 0, label: '0' },
                    { value: 1, label: '1' },
                    { value: 2, label: '2' },
                  ]}
                  valueLabelDisplay="auto"
                />
              </Box>

              <TextField
                fullWidth
                label="Max Tokens"
                type="number"
                value={settings.maxTokens}
                onChange={(e) => handleInputChange('maxTokens', parseInt(e.target.value))}
                sx={{ mb: 2 }}
              />
            </CardContent>
          </Card>
        </Box>

        {/* RAG Settings */}
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                RAG (Retrieval Augmented Generation)
              </Typography>

              <FormControlLabel
                control={
                  <Switch
                    checked={settings.ragEnabled}
                    onChange={(e) => handleInputChange('ragEnabled', e.target.checked)}
                  />
                }
                label="Enable RAG"
                sx={{ mb: 2, display: 'block' }}
              />

              <FormControl fullWidth sx={{ mb: 2 }} disabled={!settings.ragEnabled}>
                <InputLabel>Retrieval Type</InputLabel>
                <Select
                  value={settings.retrievalType}
                  onChange={(e) => handleInputChange('retrievalType', e.target.value)}
                  label="Retrieval Type"
                >
                  <MenuItem value="similarity">Similarity Search</MenuItem>
                  <MenuItem value="mmr">Maximum Marginal Relevance</MenuItem>
                  <MenuItem value="similarity_score_threshold">Similarity with Threshold</MenuItem>
                </Select>
              </FormControl>

              <Box sx={{ mb: 2 }}>
                <Typography gutterBottom>
                  Documents to Retrieve: {settings.documentsToRetrieve}
                </Typography>
                <Slider
                  value={settings.documentsToRetrieve}
                  onChange={(_, value) => handleInputChange('documentsToRetrieve', value)}
                  min={1}
                  max={20}
                  step={1}
                  marks={[
                    { value: 1, label: '1' },
                    { value: 10, label: '10' },
                    { value: 20, label: '20' },
                  ]}
                  valueLabelDisplay="auto"
                  disabled={!settings.ragEnabled}
                />
              </Box>

              <Box sx={{ mb: 2 }}>
                <Typography gutterBottom>
                  Score Threshold: {settings.scoreThreshold}
                </Typography>
                <Slider
                  value={settings.scoreThreshold}
                  onChange={(_, value) => handleInputChange('scoreThreshold', value)}
                  min={0}
                  max={1}
                  step={0.1}
                  marks={[
                    { value: 0, label: '0' },
                    { value: 0.5, label: '0.5' },
                    { value: 1, label: '1' },
                  ]}
                  valueLabelDisplay="auto"
                  disabled={!settings.ragEnabled}
                />
              </Box>
            </CardContent>
          </Card>
        </Box>

        {/* Chat Settings */}
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Chat Configuration
              </Typography>

              <TextField
                fullWidth
                label="Max Chat History"
                type="number"
                value={settings.maxChatHistory}
                onChange={(e) => handleInputChange('maxChatHistory', parseInt(e.target.value))}
                sx={{ mb: 2 }}
                helperText="Number of previous messages to keep in context"
              />

              <TextField
                fullWidth
                label="System Message"
                multiline
                rows={4}
                value={settings.systemMessage}
                onChange={(e) => handleInputChange('systemMessage', e.target.value)}
                sx={{ mb: 2 }}
                helperText="Instructions for the AI assistant"
              />
            </CardContent>
          </Card>
        </Box>

        {/* Theme & UI Settings */}
        <Box>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                🌙 Theme & Interface
              </Typography>
              
              {/* Professional Theme Selector */}
              <Box sx={{ mb: 3 }}>
                <ThemeSelector onThemeChange={(themeId) => console.log('Selected theme:', themeId)} />
              </Box>
              
              {/* Theme Mode Selection */}
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" gutterBottom>
                  Theme Mode
                </Typography>
                <Box sx={{ display: 'flex', gap: 1, mb: 1 }}>
                  <Chip
                    icon={<LightModeIcon />}
                    label="Light"
                    clickable
                    color={currentTheme === 'light' ? 'primary' : 'default'}
                    onClick={() => setTheme('light')}
                    sx={{ transition: 'all 0.2s ease-in-out' }}
                  />
                  <Chip
                    icon={<DarkModeIcon />}
                    label="Dark"
                    clickable
                    color={currentTheme === 'dark' ? 'primary' : 'default'}
                    onClick={() => setTheme('dark')}
                    sx={{ transition: 'all 0.2s ease-in-out' }}
                  />
                  <Chip
                    icon={<AutoModeIcon />}
                    label="Auto"
                    clickable
                    color={currentTheme === 'auto' ? 'primary' : 'default'}
                    onClick={() => setTheme('auto')}
                    sx={{ transition: 'all 0.2s ease-in-out' }}
                  />
                </Box>
                <Typography variant="caption" color="text.secondary">
                  {currentTheme === 'auto' 
                    ? `Currently using ${isDark ? 'dark' : 'light'} based on system preference`
                    : `Using ${currentTheme} theme`
                  }
                </Typography>
              </Box>

              {/* Quick Theme Toggle */}
              <Box sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography variant="subtitle2">
                  Quick Toggle
                </Typography>
                <DarkModeToggle size="large" showTooltip={false} />
              </Box>

              {/* Interface Options */}
              <Typography variant="subtitle2" gutterBottom sx={{ mt: 2 }}>
                Interface Options
              </Typography>
              
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.autoScroll}
                    onChange={(e) => handleInputChange('autoScroll', e.target.checked)}
                  />
                }
                label="Auto-scroll to latest message"
                sx={{ mb: 1, display: 'block' }}
              />

              <FormControlLabel
                control={
                  <Switch
                    checked={settings.showTimestamps}
                    onChange={(e) => handleInputChange('showTimestamps', e.target.checked)}
                  />
                }
                label="Show message timestamps"
                sx={{ mb: 1, display: 'block' }}
              />
              
              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                Theme preference is saved automatically and syncs across all your devices.
              </Typography>
            </CardContent>
          </Card>
        </Box>
      </Box>

      {/* Action Buttons */}
      <Box sx={{ display: 'flex', gap: 2, mt: 3 }}>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          size="large"
        >
          Save Settings
        </Button>
        <Button
          variant="outlined"
          startIcon={<ResetIcon />}
          onClick={handleReset}
          size="large"
        >
          Reset to Defaults
        </Button>
      </Box>
    </Box>
  );
};

export default SettingsPage;
