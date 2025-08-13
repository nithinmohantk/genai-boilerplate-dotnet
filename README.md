# 🤖 GenAI Chatbot Boilerplate (.NET)

A production-ready, multi-tenant GenAI chatbot platform built with **ASP.NET Core**, **React**, and **PostgreSQL**. Features real-time chat, document processing, RAG capabilities, and comprehensive admin management.

## ✨ Key Features

### 🎯 **Core Functionality**
- **Real-time WebSocket Chat** with typing indicators and message broadcasting
- **Multi-tenant Architecture** with complete tenant isolation
- **Document Upload & RAG** with automatic text extraction and chunking
- **AI Model Integration** supporting OpenAI, Anthropic, Google, and custom models
- **Chat History Persistence** with full message search and session management

### 🔐 **Authentication & Security**
- **JWT Authentication** with refresh token rotation
- **OAuth Support** framework (Google, Microsoft, Apple ready)
- **Role-Based Access Control** (Super Admin, Tenant Admin, User, Viewer)
- **Multi-factor Security** with BCrypt password hashing
- **API Key Management** for tenant-specific AI provider keys

### 👑 **Admin Center**
- **Tenant Management** - Create, configure, and monitor tenants
- **User Management** - Role assignment, activation/deactivation
- **System Statistics** - Usage metrics and analytics
- **API Key Configuration** - Secure AI provider key management
- **Real-time Monitoring** - Connection stats and system health

### 🏗️ **Technical Excellence**
- **Clean Architecture** - Domain-driven design with proper layer separation
- **Entity Framework Core** - Code-first approach with PostgreSQL
- **Type Safety** - Full C# type safety and validation
- **Dependency Injection** - Built-in ASP.NET Core DI container
- **Health Checks & Metrics** - Kubernetes-ready with health endpoints

## 🔄 Development Workflow

This project uses **GitHub Flow** with a `develop` branch for ongoing development. See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed workflow guidelines.

### Quick Start for Contributors
```bash
# Clone and setup
git clone https://github.com/nithinmohantk/genai-boilerplate-dotnet.git
cd genai-boilerplate-dotnet
git checkout develop

# Create feature branch
git checkout -b feature/your-feature-name

# Make changes, commit, and push
git add .
git commit -m "feat: add your feature description"
git push -u origin feature/your-feature-name

# Create PR to develop branch on GitHub
```

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0+** and **Node.js 18+**
- **PostgreSQL 13+** and **Redis 6+**
- **Git** for cloning

### 1. Clone & Setup
```bash
git clone https://github.com/nithinmohantk/genai-boilerplate-dotnet.git
cd genai-boilerplate-dotnet
```

### 2. Backend Setup
```bash
# Navigate to backend
cd backend

# Restore packages
dotnet restore

# Update database connection string in appsettings.json
# Then run the application
dotnet run --project GenAIBoilerplate.API
```
🎉 **Backend running at https://localhost:5001**

### 3. Frontend Setup
```bash
# In a new terminal, navigate to frontend
cd frontend
npm install
npm start
```
🎉 **Frontend running at http://localhost:3000**

### 4. Database Setup
The application will automatically create the database on first run. For production, use Entity Framework migrations:

```bash
# Add migration
dotnet ef migrations add InitialCreate --project GenAIBoilerplate.Infrastructure --startup-project GenAIBoilerplate.API

# Update database
dotnet ef database update --project GenAIBoilerplate.Infrastructure --startup-project GenAIBoilerplate.API
```

## 📚 API Documentation

### Authentication Endpoints
```bash
POST /api/v1/auth/login          # User login
POST /api/v1/auth/register       # User registration  
POST /api/v1/auth/refresh        # Token refresh
GET  /api/v1/auth/me             # Current user info
PUT  /api/v1/auth/me             # Update profile
POST /api/v1/auth/logout         # User logout
```

### Chat Endpoints
```bash
POST /api/v1/chat/sessions       # Create chat session
GET  /api/v1/chat/sessions       # List user sessions
GET  /api/v1/chat/sessions/{id}  # Get session with messages
POST /api/v1/chat/completions    # Generate AI response
GET  /api/v1/chat/models         # Available AI models
GET  /api/v1/chat/search         # Search sessions
```

### Document Endpoints
```bash
POST /api/v1/documents/upload    # Upload document
GET  /api/v1/documents/          # List documents
POST /api/v1/documents/{id}/process  # Process for RAG
GET  /api/v1/documents/{id}/chunks   # View document chunks
```

### Admin Endpoints
```bash
GET  /api/v1/admin/tenants       # Manage tenants (Super Admin)
GET  /api/v1/admin/users         # Manage users (Tenant Admin)
GET  /api/v1/admin/api-keys      # Manage API keys
GET  /api/v1/admin/system/stats  # System statistics
```

### Health Check Endpoints
```bash
GET  /health                     # Application health
GET  /api/v1/health/live         # Liveness probe
GET  /api/v1/health/ready        # Readiness probe
```

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React SPA     │    │  ASP.NET Core   │    │  PostgreSQL     │
│                 │    │     Web API     │    │                 │
│ • Chat UI       │◄──►│ • REST API      │◄──►│ • Multi-tenant  │
│ • Admin Panel   │    │ • SignalR Hub   │    │ • Chat History  │
│ • Auth Flow     │    │ • Auth & RBAC   │    │ • Documents     │
└─────────────────┘    │ • Document RAG  │    │ • Users/Tenants │
                       │ • AI Integration│    └─────────────────┘
                       └─────────────────┘              │
                              │                         │
                    ┌─────────────────┐    ┌─────────────────┐
                    │      Redis      │    │   AI Providers  │
                    │                 │    │                 │
                    │ • Sessions      │    │ • OpenAI        │
                    │ • Cache         │    │ • Anthropic     │
                    │ • SignalR       │    │ • Google        │
                    └─────────────────┘    │ • Custom APIs   │
                                          └─────────────────┘
```

## 🛠️ Project Structure

```
genai-boilerplate-dotnet/
├── backend/
│   ├── GenAIBoilerplate.API/          # Web API project
│   │   ├── Controllers/               # API controllers
│   │   ├── Program.cs                 # Application entry point
│   │   └── appsettings.json          # Configuration
│   ├── GenAIBoilerplate.Core/         # Domain layer
│   │   ├── Entities/                  # Domain entities
│   │   ├── Enums/                     # Domain enums
│   │   ├── Interfaces/                # Repository interfaces
│   │   └── Common/                    # Base classes
│   ├── GenAIBoilerplate.Application/  # Application layer
│   │   ├── Services/                  # Application services
│   │   ├── DTOs/                      # Data transfer objects
│   │   └── Mappings/                  # Object mappings
│   └── GenAIBoilerplate.Infrastructure/ # Infrastructure layer
│       ├── Persistence/               # Database context & repositories
│       ├── Services/                  # External service implementations
│       └── Migrations/                # EF Core migrations
├── frontend/                          # React frontend (same as Python version)
│   ├── src/
│   │   ├── components/
│   │   ├── pages/
│   │   └── contexts/
│   └── package.json
├── data/                             # Data directories
│   ├── documents/
│   ├── embeddings/
│   └── uploads/
└── GenAIBoilerplate.sln              # Solution file
```

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=genai_chatbot;Username=genai_user;Password=genai_password"

# JWT
JwtSettings__SecretKey="your-super-secret-key-here-change-in-production"
JwtSettings__Issuer="GenAIBoilerplateAPI"
JwtSettings__Audience="GenAIBoilerplateClient"

# Redis
RedisSettings__ConnectionString="localhost:6379"

# AI Providers
AIProviders__OpenAI__ApiKey="sk-your-openai-key"
AIProviders__Anthropic__ApiKey="claude-your-key"
```

### Multi-Model Configuration
Each tenant can configure their own AI provider keys through the admin panel with support for the latest AI models:

#### **OpenAI Models**
- **GPT-4 Series**: GPT-4, GPT-4 Turbo, GPT-4o, GPT-4o Mini, GPT-4.1, GPT-4.1 Mini, GPT-4.1 Nano
- **GPT-3.5 Series**: GPT-3.5 Turbo, GPT-3.5 Turbo 16K
- **GPT-5 Series**: GPT-5, GPT-5.0 Mini, GPT-5.0 Nano ✨ **Latest Release**

#### **Anthropic Models**
- **Claude 3 Series**: Claude 3 Haiku, Claude 3 Sonnet, Claude 3 Opus
- **Claude 3.5 Series**: Claude 3.5 Sonnet, Claude 3.5 Haiku
- **Claude 4.0 Series**: Claude 4.0 Opus, Claude 4.0 Sonnet ✨ **Latest Release**

## 🎭 User Roles & Permissions

### Role Hierarchy
```
Super Admin    🏆 Full system access, manage all tenants
    │
Tenant Admin   👑 Manage tenant users, settings, API keys  
    │
Tenant User    👤 Chat, upload documents, personal settings
    │
Tenant Viewer  👁️ Read-only access to tenant resources
```

## 🐳 Docker Deployment

### Quick Docker Setup
```bash
# Build and run with docker-compose
docker-compose up --build

# Or run individual services
docker build -t genai-backend ./backend
docker build -t genai-frontend ./frontend

docker run -p 5000:8080 genai-backend
docker run -p 3000:3000 genai-frontend
```

## 🧪 Testing

### Backend Tests
```bash
cd backend

# Run tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests
```bash
cd frontend

# Run component tests
npm test

# Run E2E tests
npm run test:e2e
```

## 🔍 Monitoring & Observability

### Health Checks
- `GET /health` - Application health
- `GET /api/v1/health/live` - Liveness probe
- `GET /api/v1/health/ready` - Readiness probe

### Swagger Documentation
- Development: `https://localhost:5001/swagger`
- Comprehensive API documentation with authentication support

## 🚨 Security Features

### Authentication Security
- **JWT with configurable algorithms** (HS256, RS256)
- **Refresh Token Rotation** - Automatic token renewal
- **Session Management** - Device tracking and revocation
- **Rate Limiting** - Built-in ASP.NET Core rate limiting

### Data Protection
- **Tenant Isolation** - Complete data separation with EF Core
- **Encrypted Storage** - Passwords with BCrypt
- **Input Validation** - Model validation and sanitization
- **HTTPS Enforcement** - Production security headers

## 🤝 Contributing

### Development Setup
```bash
# Backend development
cd backend
dotnet restore
dotnet build

# Frontend development  
cd frontend
npm install
npm run dev
```

### Code Standards
- **C#**: .NET coding standards, nullable reference types
- **JavaScript**: Prettier, ESLint, TypeScript
- **Commits**: Conventional commits format
- **Testing**: Unit + integration test coverage

## 📞 Support & Documentation

- **🐛 Issues**: [GitHub Issues](https://github.com/nithinmohantk/genai-boilerplate-dotnet/issues)
- **💬 Discussions**: [GitHub Discussions](https://github.com/nithinmohantk/genai-boilerplate-dotnet/discussions)  
- **📖 Wiki**: [Project Wiki](https://github.com/nithinmohantk/genai-boilerplate-dotnet/wiki)
- **🚀 Releases**: [Release Notes](https://github.com/nithinmohantk/genai-boilerplate-dotnet/releases)

## 📄 License

This project is licensed under the **MIT License** - see [LICENSE](LICENSE) file for details.

---

## 🎯 What's Next?

This boilerplate provides a solid foundation for building production GenAI applications with .NET. Here are some potential enhancements:

### Immediate Extensions
- **SignalR Integration** for real-time chat
- **Vector Database Integration** (Pinecone, Weaviate, Chroma)
- **Advanced RAG** with semantic chunking and reranking
- **Background Jobs** with Hangfire or Quartz.NET

### Advanced Features
- **Multi-language Support** with localization
- **Custom Model Fine-tuning** workflows
- **Analytics Dashboard** with usage insights
- **gRPC Services** for high-performance APIs

### Enterprise Features
- **Azure AD / Identity Server** integration
- **Audit Logging** with compliance reports
- **Multi-database Support** (SQL Server, MySQL)
- **Microservices Architecture** with service mesh

---

**🚀 Ready to build the next generation of AI-powered applications with .NET!**

*Built with ❤️ by [Nithin Mohan](https://github.com/nithinmohantk)*
