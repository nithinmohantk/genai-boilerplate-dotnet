# GenAI Boilerplate .NET Backend

![Docker Image Size](https://img.shields.io/docker/image-size/nithinmohantk/genai-boilerplate-backend)
![Docker Pulls](https://img.shields.io/docker/pulls/nithinmohantk/genai-boilerplate-backend)
![Docker Stars](https://img.shields.io/docker/stars/nithinmohantk/genai-boilerplate-backend)

Production-ready ASP.NET Core Web API for GenAI Chatbot Platform with multi-tenant architecture, JWT authentication, AI integration, and real-time chat capabilities.

## 🚀 Quick Start

### Basic Usage
```bash
docker run -d \
  --name genai-backend \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Database=genai_chatbot;Username=genai_user;Password=genai_password" \
  -e RedisSettings__ConnectionString="redis:6379" \
  -e JwtSettings__SecretKey="your-super-secret-key-here-change-in-production-at-least-32-characters-long" \
  nithinmohantk/genai-boilerplate-backend:latest
```

### With Docker Compose
```yaml
version: '3.8'
services:
  backend:
    image: nithinmohantk/genai-boilerplate-backend:latest
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=genai_chatbot;Username=genai_user;Password=genai_password
      - RedisSettings__ConnectionString=redis:6379
      - JwtSettings__SecretKey=your-super-secret-key-here-change-in-production
    depends_on:
      - postgres
      - redis
```

## 🔧 Configuration

### Required Environment Variables
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `RedisSettings__ConnectionString` - Redis connection string
- `JwtSettings__SecretKey` - JWT secret key (minimum 32 characters)

### Optional Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `JwtSettings__Issuer` - JWT issuer (default: GenAIBoilerplateAPI)
- `JwtSettings__Audience` - JWT audience (default: GenAIBoilerplateClient)
- `JwtSettings__ExpirationInMinutes` - JWT expiration (default: 60)
- `AIProviders__OpenAI__ApiKey` - OpenAI API key
- `AIProviders__Anthropic__ApiKey` - Anthropic API key

## 🏗️ Features

### Core Functionality
- **REST API** with comprehensive endpoints for authentication, chat, and admin
- **Multi-tenant Architecture** with complete tenant isolation
- **JWT Authentication** with refresh token support
- **Real-time Chat** with SignalR WebSocket support
- **AI Integration** supporting OpenAI, Anthropic, and custom models
- **Document Processing** with RAG capabilities

### Technical Excellence
- **Clean Architecture** with domain-driven design
- **Entity Framework Core** with PostgreSQL support
- **Redis Caching** for session management and performance
- **Health Checks** for monitoring and orchestration
- **Swagger Documentation** with authentication support
- **Security** with BCrypt password hashing and CORS protection

## 📚 API Endpoints

### Authentication
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/register` - User registration
- `POST /api/v1/auth/refresh` - Token refresh
- `GET /api/v1/auth/me` - Current user info

### Chat
- `POST /api/v1/chat/sessions` - Create chat session
- `GET /api/v1/chat/sessions` - List user sessions
- `POST /api/v1/chat/completions` - Generate AI response

### Admin
- `GET /api/v1/admin/tenants` - Manage tenants (Super Admin)
- `GET /api/v1/admin/users` - Manage users (Tenant Admin)

### Health & Monitoring
- `GET /health` - Health check endpoint
- `GET /swagger` - API documentation

## 🐳 Image Details

### Multi-architecture Support
- `linux/amd64` - Intel/AMD 64-bit
- `linux/arm64` - ARM 64-bit (Apple Silicon, ARM servers)

### Security
- Non-root user execution
- Distroless base image for minimal attack surface
- Regular security updates
- Vulnerability scanning with Trivy

### Size Optimization
- Multi-stage Docker build
- Layer caching for faster builds
- Minimal runtime dependencies

## 🔍 Health Checks

The container includes built-in health checks:
```bash
# Manual health check
curl http://localhost:8080/health

# Docker health check (automatic)
docker ps  # Shows healthy/unhealthy status
```

## 📊 Monitoring

### Metrics
- Built-in ASP.NET Core metrics
- Custom business metrics
- Performance counters

### Logging
- Structured logging with Serilog
- Configurable log levels
- JSON output for log aggregation

## 🚀 Production Deployment

### Recommended Setup
```bash
# Pull the image
docker pull nithinmohantk/genai-boilerplate-backend:latest

# Run with production settings
docker run -d \
  --name genai-backend-prod \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=your-postgres;Database=genai_chatbot;Username=user;Password=pass;SSL Mode=Require" \
  -e RedisSettings__ConnectionString="your-redis:6379" \
  -e JwtSettings__SecretKey="$JWT_SECRET_KEY" \
  -v /app/data:/app/data \
  -v /app/logs:/app/logs \
  nithinmohantk/genai-boilerplate-backend:latest
```

### Scaling
- Horizontal scaling ready
- Stateless design
- Session data stored in Redis
- Database connection pooling

## 📝 Tags

- `latest` - Latest stable release from main branch
- `develop` - Latest development build
- `v1.0.0`, `v1.1.0`, etc. - Semantic version releases
- `sha-<commit>` - Specific commit builds

## 🔗 Related Images

- [Frontend](https://hub.docker.com/r/nithinmohantk/genai-boilerplate-frontend) - React frontend application
- [PostgreSQL](https://hub.docker.com/_/postgres) - Recommended database
- [Redis](https://hub.docker.com/_/redis) - Required for caching

## 📖 Documentation

- [GitHub Repository](https://github.com/nithinmohantk/genai-boilerplate-dotnet)
- [API Documentation](https://github.com/nithinmohantk/genai-boilerplate-dotnet/blob/main/README.md#-api-documentation)
- [Docker Compose Example](https://github.com/nithinmohantk/genai-boilerplate-dotnet/blob/main/docker-compose.yml)

## 🤝 Support

- [Issues](https://github.com/nithinmohantk/genai-boilerplate-dotnet/issues)
- [Discussions](https://github.com/nithinmohantk/genai-boilerplate-dotnet/discussions)

## 📄 License

MIT License - see [LICENSE](https://github.com/nithinmohantk/genai-boilerplate-dotnet/blob/main/LICENSE) for details.
