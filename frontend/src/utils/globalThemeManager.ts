import { type BackendTheme } from '../hooks/useThemeApplication';

export interface ThemeState {
  currentThemeId: string | null;
  currentThemeData: BackendTheme | null;
  isLoading: boolean;
}

class GlobalThemeManager {
  private state: ThemeState = {
    currentThemeId: null,
    currentThemeData: null,
    isLoading: false,
  };
  
  private listeners: Array<(state: ThemeState) => void> = [];
  
  // Subscribe to theme state changes
  subscribe(listener: (state: ThemeState) => void) {
    this.listeners.push(listener);
    
    // Return unsubscribe function
    return () => {
      const index = this.listeners.indexOf(listener);
      if (index > -1) {
        this.listeners.splice(index, 1);
      }
    };
  }
  
  // Notify all listeners of state changes
  private notifyListeners() {
    this.listeners.forEach(listener => listener({ ...this.state }));
  }
  
  // Get current theme state
  getCurrentState(): ThemeState {
    return { ...this.state };
  }
  
  // Set the current theme
  setCurrentTheme(themeId: string, themeData: BackendTheme) {
    this.state.currentThemeId = themeId;
    this.state.currentThemeData = themeData;
    this.notifyListeners();
    
    // Persist to localStorage
    try {
      localStorage.setItem('global-theme-id', themeId);
      localStorage.setItem('global-theme-data', JSON.stringify(themeData));
    } catch (error) {
      console.warn('Failed to persist theme to localStorage:', error);
    }
  }
  
  // Clear the current theme
  clearCurrentTheme() {
    this.state.currentThemeId = null;
    this.state.currentThemeData = null;
    this.notifyListeners();
    
    // Clear from localStorage
    try {
      localStorage.removeItem('global-theme-id');
      localStorage.removeItem('global-theme-data');
    } catch (error) {
      console.warn('Failed to clear theme from localStorage:', error);
    }
  }
  
  // Set loading state
  setLoading(isLoading: boolean) {
    this.state.isLoading = isLoading;
    this.notifyListeners();
  }
  
  // Load theme from localStorage
  loadPersistedTheme(): { themeId: string; themeData: BackendTheme } | null {
    try {
      const themeId = localStorage.getItem('global-theme-id');
      const themeDataStr = localStorage.getItem('global-theme-data');
      
      if (themeId && themeDataStr) {
        const themeData = JSON.parse(themeDataStr);
        this.state.currentThemeId = themeId;
        this.state.currentThemeData = themeData;
        this.notifyListeners();
        
        return { themeId, themeData };
      }
    } catch (error) {
      console.warn('Failed to load persisted theme:', error);
      // Clear invalid data
      this.clearCurrentTheme();
    }
    
    return null;
  }
  
  // Get theme by category (useful for theme browsing)
  async fetchThemesByCategory(category: string): Promise<BackendTheme[]> {
    try {
      this.setLoading(true);
      const response = await fetch(`http://localhost:8000/api/v1/themes?category=${encodeURIComponent(category)}`);
      if (!response.ok) {
        throw new Error(`Failed to fetch themes for category: ${category}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch themes by category:', error);
      return [];
    } finally {
      this.setLoading(false);
    }
  }
  
  // Get all available themes
  async fetchAllThemes(): Promise<BackendTheme[]> {
    try {
      this.setLoading(true);
      const response = await fetch('http://localhost:8000/api/v1/themes');
      if (!response.ok) {
        throw new Error('Failed to fetch themes');
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch all themes:', error);
      return [];
    } finally {
      this.setLoading(false);
    }
  }
  
  // Search themes by name or description
  async searchThemes(query: string): Promise<BackendTheme[]> {
    try {
      this.setLoading(true);
      const response = await fetch(`http://localhost:8000/api/v1/themes?search=${encodeURIComponent(query)}`);
      if (!response.ok) {
        throw new Error(`Failed to search themes: ${query}`);
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to search themes:', error);
      return [];
    } finally {
      this.setLoading(false);
    }
  }
  
  // Get theme categories
  async getThemeCategories(): Promise<string[]> {
    try {
      const response = await fetch('http://localhost:8000/api/v1/themes/categories');
      if (!response.ok) {
        throw new Error('Failed to fetch theme categories');
      }
      return await response.json();
    } catch (error) {
      console.error('Failed to fetch theme categories:', error);
      return ['default', 'modern', 'nature', 'glass', 'vintage', 'cyberpunk', 'medical'];
    }
  }
}

// Export singleton instance
export const globalThemeManager = new GlobalThemeManager();

// Also export as default for flexibility
export default globalThemeManager;
