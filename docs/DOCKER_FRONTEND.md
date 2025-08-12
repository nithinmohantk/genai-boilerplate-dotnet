# GenAI Boilerplate .NET Frontend

![Docker Image Size](https://img.shields.io/docker/image-size/nithinmohantk/genai-boilerplate-frontend)
![Docker Pulls](https://img.shields.io/docker/pulls/nithinmohantk/genai-boilerplate-frontend)
![Docker Stars](https://img.shields.io/docker/stars/nithinmohantk/genai-boilerplate-frontend)

Production-ready React frontend for GenAI Chatbot Platform with modern UI, dark/light themes, real-time chat, and comprehensive admin interface.

## 🚀 Quick Start

### Basic Usage
```bash
docker run -d \
  --name genai-frontend \
  -p 3000:8080 \
  -e VITE_API_BASE_URL="http://localhost:8000" \
  -e VITE_WS_URL="ws://localhost:8000" \
  nithinmohantk/genai-boilerplate-frontend:latest
```

### With Docker Compose
```yaml
version: '3.8'
services:
  frontend:
    image: nithinmohantk/genai-boilerplate-frontend:latest
    ports:
      - "3000:8080"
    environment:
      - VITE_API_BASE_URL=http://localhost:8000
      - VITE_WS_URL=ws://localhost:8000
      - NODE_ENV=production
    depends_on:
      - backend
```

## 🔧 Configuration

### Environment Variables
- `VITE_API_BASE_URL` - Backend API base URL (e.g., http://localhost:8000)
- `VITE_WS_URL` - WebSocket URL for real-time chat (e.g., ws://localhost:8000)
- `NODE_ENV` - Environment (development/production)

## 🏗️ Features

### User Interface
- **Modern React SPA** with TypeScript
- **Responsive Design** optimized for desktop and mobile
- **Dark/Light Theme** with system preference detection
- **Real-time Chat** with WebSocket support
- **Rich Text Editor** with markdown support
- **File Upload** with drag-and-drop support

### Chat Experience
- **Conversational UI** with message bubbles
- **Typing Indicators** for real-time feedback
- **Message History** with infinite scroll
- **Chat Sessions** with session management
- **AI Model Selection** with provider switching

### Admin Interface
- **Tenant Management** for multi-tenant operations
- **User Management** with role-based access
- **System Analytics** with usage metrics
- **API Key Management** for AI providers
- **Settings Configuration** with real-time updates

### Technical Features
- **Progressive Web App** (PWA) support
- **Service Worker** for offline capabilities
- **Code Splitting** for optimized loading
- **Hot Module Replacement** for development
- **Build Optimization** with Vite

## 🎨 UI Components

### Core Components
- Authentication forms (Login/Register)
- Chat interface with message history
- Document upload with preview
- Settings panels with form validation
- Admin dashboards with data tables

### Theme System
- Material Design inspired
- CSS-in-JS with styled-components
- Customizable color schemes
- Responsive breakpoints
- Accessibility compliant

## 🐳 Image Details

### Multi-architecture Support
- `linux/amd64` - Intel/AMD 64-bit
- `linux/arm64` - ARM 64-bit (Apple Silicon, ARM servers)

### Security
- **Nginx-based** production server
- **Non-root user** execution
- **Security headers** for XSS/CSRF protection
- **Content Security Policy** implementation
- **HTTPS redirect** ready

### Performance
- **Static file serving** with Nginx
- **Gzip compression** enabled
- **Browser caching** optimization
- **Asset minification** and bundling
- **Tree shaking** for minimal bundle size

## 🔍 Health Checks

The container includes built-in health checks:
```bash
# Manual health check
curl http://localhost:8080/health

# Docker health check (automatic)
docker ps  # Shows healthy/unhealthy status
```

## 📊 Monitoring

### Performance Metrics
- Bundle size analysis
- Runtime performance monitoring
- Network request tracking
- Error boundary reporting

### Logging
- Browser console logging
- Error tracking integration
- Performance monitoring
- User analytics (optional)

## 🚀 Production Deployment

### Recommended Setup
```bash
# Pull the image
docker pull nithinmohantk/genai-boilerplate-frontend:latest

# Run with production settings
docker run -d \
  --name genai-frontend-prod \
  --restart unless-stopped \
  -p 3000:8080 \
  -e VITE_API_BASE_URL="https://api.yourdomain.com" \
  -e VITE_WS_URL="wss://api.yourdomain.com" \
  -e NODE_ENV=production \
  nithinmohantk/genai-boilerplate-frontend:latest
```

### Reverse Proxy Integration
```nginx
# Nginx configuration example
location / {
    proxy_pass http://frontend:8080;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}

# Static asset caching
location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

## 🔧 Development

### Local Development
```bash
# Clone the repository
git clone https://github.com/nithinmohantk/genai-boilerplate-dotnet.git
cd genai-boilerplate-dotnet/frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

### Build Configuration
- **Vite** for fast development and building
- **TypeScript** for type safety
- **ESLint** for code linting
- **Prettier** for code formatting
- **Tailwind CSS** for styling

## 🧪 Testing

### Test Suite
- **Unit Tests** with Jest and React Testing Library
- **Integration Tests** for component interactions
- **E2E Tests** with Playwright (planned)
- **Visual Regression Tests** (planned)

### Running Tests
```bash
# Run unit tests
npm test

# Run tests with coverage
npm run test:coverage

# Run E2E tests
npm run test:e2e
```

## 📝 Tags

- `latest` - Latest stable release from main branch
- `develop` - Latest development build
- `v1.0.0`, `v1.1.0`, etc. - Semantic version releases
- `sha-<commit>` - Specific commit builds

## 🔗 Related Images

- [Backend](https://hub.docker.com/r/nithinmohantk/genai-boilerplate-backend) - ASP.NET Core backend API
- [Nginx](https://hub.docker.com/_/nginx) - Used as production web server

## 📖 Documentation

- [GitHub Repository](https://github.com/nithinmohantk/genai-boilerplate-dotnet)
- [Frontend Documentation](https://github.com/nithinmohantk/genai-boilerplate-dotnet/blob/main/frontend/README.md)
- [Component Storybook](https://genai-boilerplate-storybook.netlify.app) (planned)

## 🤝 Support

- [Issues](https://github.com/nithinmohantk/genai-boilerplate-dotnet/issues)
- [Discussions](https://github.com/nithinmohantk/genai-boilerplate-dotnet/discussions)
- [Frontend Specific Issues](https://github.com/nithinmohantk/genai-boilerplate-dotnet/issues?q=is%3Aissue+is%3Aopen+label%3Afrontend)

## 📄 License

MIT License - see [LICENSE](https://github.com/nithinmohantk/genai-boilerplate-dotnet/blob/main/LICENSE) for details.

---

**Built with ❤️ using React, TypeScript, Vite, and Tailwind CSS**
