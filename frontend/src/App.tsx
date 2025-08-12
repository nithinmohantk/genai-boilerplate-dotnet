import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import CssBaseline from '@mui/material/CssBaseline';
import { CustomThemeProvider } from './contexts/ThemeContext';
import { ThemeApplicationProvider } from './contexts/ThemeApplicationContext';
import ErrorBoundary from './components/ErrorBoundary';
import Layout from './components/Layout';
import ChatPage from './pages/ChatPage';
import DocumentsPage from './pages/DocumentsPage';
import SettingsPage from './pages/SettingsPage';
import AdminPage from './pages/AdminPage';

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ErrorBoundary>
          <CustomThemeProvider>
            <ThemeApplicationProvider>
              <CssBaseline />
              <ErrorBoundary>
                <Router>
                  <Layout>
                    <Routes>
                      <Route path="/" element={<ChatPage />} />
                      <Route path="/chat" element={<ChatPage />} />
                      <Route path="/documents" element={<DocumentsPage />} />
                      <Route path="/settings" element={<SettingsPage />} />
                      <Route path="/admin" element={<AdminPage />} />
                    </Routes>
                  </Layout>
                </Router>
              </ErrorBoundary>
            </ThemeApplicationProvider>
          </CustomThemeProvider>
        </ErrorBoundary>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;
