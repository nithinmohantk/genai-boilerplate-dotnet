import React, { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Card,
  CardContent,
  CardActions,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  LinearProgress,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Description as DocumentIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  GetApp as DownloadIcon,
} from '@mui/icons-material';

interface Document {
  id: string;
  name: string;
  type: string;
  size: number;
  uploadDate: Date;
  status: 'processing' | 'ready' | 'error';
  chunks?: number;
}

const DocumentsPage: React.FC = () => {
  const [documents, setDocuments] = useState<Document[]>([
    {
      id: '1',
      name: 'Company Overview.pdf',
      type: 'pdf',
      size: 1024000,
      uploadDate: new Date('2024-01-15'),
      status: 'ready',
      chunks: 12,
    },
    {
      id: '2',
      name: 'Product Manual.docx',
      type: 'docx',
      size: 2048000,
      uploadDate: new Date('2024-01-14'),
      status: 'ready',
      chunks: 28,
    },
    {
      id: '3',
      name: 'Technical Specifications.md',
      type: 'md',
      size: 512000,
      uploadDate: new Date('2024-01-13'),
      status: 'processing',
    },
  ]);

  const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getStatusColor = (status: Document['status']) => {
    switch (status) {
      case 'ready': return 'success';
      case 'processing': return 'warning';
      case 'error': return 'error';
      default: return 'default';
    }
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setUploadDialogOpen(true);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setIsUploading(true);
    
    // Simulate upload process
    setTimeout(() => {
      const newDocument: Document = {
        id: Date.now().toString(),
        name: selectedFile.name,
        type: selectedFile.name.split('.').pop() || 'unknown',
        size: selectedFile.size,
        uploadDate: new Date(),
        status: 'processing',
      };

      setDocuments(prev => [...prev, newDocument]);
      setIsUploading(false);
      setUploadDialogOpen(false);
      setSelectedFile(null);

      // Simulate processing completion
      setTimeout(() => {
        setDocuments(prev => prev.map(doc => 
          doc.id === newDocument.id 
            ? { ...doc, status: 'ready', chunks: Math.floor(Math.random() * 30) + 5 }
            : doc
        ));
      }, 3000);
    }, 2000);
  };

  const handleDelete = (documentId: string) => {
    setDocuments(prev => prev.filter(doc => doc.id !== documentId));
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Typography variant="h4" component="h1">
            Document Management
          </Typography>
          <Button
            variant="contained"
            startIcon={<UploadIcon />}
            component="label"
          >
            Upload Document
            <input
              type="file"
              hidden
              accept=".pdf,.doc,.docx,.txt,.md,.xlsx,.csv"
              onChange={handleFileSelect}
            />
          </Button>
        </Box>
        <Typography variant="body1" color="text.secondary">
          Upload and manage documents for AI-powered conversations. Supported formats: PDF, Word, Text, Markdown, Excel, CSV
        </Typography>
        <Box sx={{ mt: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Total Documents: {documents.length} | 
            Ready: {documents.filter(d => d.status === 'ready').length} |
            Processing: {documents.filter(d => d.status === 'processing').length}
          </Typography>
        </Box>
      </Paper>

      {/* Documents Grid */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm: 'repeat(2, 1fr)',
            md: 'repeat(3, 1fr)',
          },
          gap: 2,
        }}
      >
        {documents.map((document) => (
          <Box key={document.id}>
            <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
              <CardContent sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <DocumentIcon sx={{ mr: 1, color: 'text.secondary' }} />
                  <Typography variant="h6" component="h3" noWrap sx={{ flexGrow: 1 }}>
                    {document.name}
                  </Typography>
                </Box>
                
                <Box sx={{ mb: 2 }}>
                  <Chip 
                    label={document.status.toUpperCase()}
                    color={getStatusColor(document.status)}
                    size="small"
                    sx={{ mr: 1 }}
                  />
                  <Chip 
                    label={document.type.toUpperCase()}
                    variant="outlined"
                    size="small"
                  />
                </Box>

                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Size: {formatFileSize(document.size)}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  Uploaded: {document.uploadDate.toLocaleDateString()}
                </Typography>
                {document.chunks && (
                  <Typography variant="body2" color="text.secondary">
                    Chunks: {document.chunks}
                  </Typography>
                )}

                {document.status === 'processing' && (
                  <Box sx={{ mt: 2 }}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Processing...
                    </Typography>
                    <LinearProgress />
                  </Box>
                )}
              </CardContent>

              <CardActions>
                <IconButton size="small" disabled={document.status !== 'ready'}>
                  <ViewIcon />
                </IconButton>
                <IconButton size="small" disabled={document.status !== 'ready'}>
                  <DownloadIcon />
                </IconButton>
                <IconButton 
                  size="small" 
                  color="error"
                  onClick={() => handleDelete(document.id)}
                >
                  <DeleteIcon />
                </IconButton>
              </CardActions>
            </Card>
          </Box>
        ))}
      </Box>

      {/* Upload Dialog */}
      <Dialog open={uploadDialogOpen} onClose={() => setUploadDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Upload Document</DialogTitle>
        <DialogContent>
          {selectedFile && (
            <Box>
              <Typography variant="body1" gutterBottom>
                <strong>File:</strong> {selectedFile.name}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <strong>Size:</strong> {formatFileSize(selectedFile.size)}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <strong>Type:</strong> {selectedFile.type || 'Unknown'}
              </Typography>
              
              {isUploading && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" gutterBottom>
                    Uploading and processing document...
                  </Typography>
                  <LinearProgress />
                </Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setUploadDialogOpen(false)}
            disabled={isUploading}
          >
            Cancel
          </Button>
          <Button 
            onClick={handleUpload}
            variant="contained"
            disabled={!selectedFile || isUploading}
          >
            Upload
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DocumentsPage;
