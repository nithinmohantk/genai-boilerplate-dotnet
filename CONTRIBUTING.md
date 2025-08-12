# Contributing to GenAI Boilerplate .NET

Thank you for your interest in contributing to this project! This document outlines our development workflow and contribution guidelines.

## 🔄 GitHub Flow Workflow

We use a simplified GitHub Flow with a `develop` branch for ongoing development:

### Branch Structure
- **`main`** - Production-ready, stable code
- **`develop`** - Integration branch for ongoing development
- **`feature/*`** - Feature branches for new functionality
- **`bugfix/*`** - Bug fix branches
- **`hotfix/*`** - Emergency fixes for production

### Development Workflow

#### 1. For New Features
```bash
# Start from develop
git checkout develop
git pull origin develop

# Create feature branch
git checkout -b feature/your-feature-name

# Make your changes and commit
git add .
git commit -m "feat: add your feature description"

# Push and create PR to develop
git push -u origin feature/your-feature-name
```

#### 2. For Bug Fixes
```bash
# Start from develop
git checkout develop
git pull origin develop

# Create bugfix branch
git checkout -b bugfix/issue-description

# Make your changes and commit
git add .
git commit -m "fix: resolve issue description"

# Push and create PR to develop
git push -u origin bugfix/issue-description
```

#### 3. For Hotfixes (Critical Production Issues)
```bash
# Start from main
git checkout main
git pull origin main

# Create hotfix branch
git checkout -b hotfix/critical-issue-name

# Make your changes and commit
git add .
git commit -m "hotfix: resolve critical issue"

# Push and create PR to main (and merge to develop after)
git push -u origin hotfix/critical-issue-name
```

### Pull Request Process

1. **Create PR** to appropriate branch (`develop` for features/bugs, `main` for hotfixes)
2. **Fill PR template** with description, changes, and testing notes
3. **Request review** from maintainers
4. **Address feedback** and update PR as needed
5. **Squash and merge** once approved

### Release Process

1. Create release branch from `develop`: `release/v1.x.x`
2. Final testing and bug fixes on release branch
3. Merge release branch to `main`
4. Tag release: `git tag v1.x.x`
5. Merge back to `develop`

## 📝 Commit Message Convention

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `ci`: CI/CD changes

### Examples:
```
feat(auth): add OAuth2 integration
fix(api): resolve chat session creation bug
docs: update installation instructions
test(services): add unit tests for AIService
```

## 🧪 Testing Requirements

Before submitting a PR, ensure:

- [ ] All existing tests pass: `dotnet test`
- [ ] New features include appropriate tests
- [ ] Code builds successfully: `dotnet build`
- [ ] Docker setup works: `docker-compose up`
- [ ] API endpoints tested with Swagger
- [ ] Frontend integration tested

## 📋 Code Style Guidelines

### Backend (.NET)
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and single-purpose
- Use dependency injection appropriately

### Frontend (React)
- Follow existing code style and patterns
- Use TypeScript for type safety
- Keep components focused and reusable
- Add PropTypes or TypeScript interfaces

### General
- Update README.md if adding new features
- Update API documentation if changing endpoints
- Add appropriate error handling
- Follow security best practices

## 🚀 Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/nithinmohantk/genai-boilerplate-dotnet.git
cd genai-boilerplate-dotnet
```

2. **Switch to develop branch**
```bash
git checkout develop
```

3. **Set up environment**
```bash
# Copy environment template
cp .env.template .env

# Edit .env with your configuration
```

4. **Run with Docker**
```bash
docker-compose up -d
```

5. **Or run manually**
```bash
# Start dependencies
docker-compose up -d postgres redis

# Run backend
cd backend
dotnet run --project GenAIBoilerplate.API

# Run frontend
cd ../frontend
npm install
npm start
```

## 📞 Getting Help

- **Issues**: Open a GitHub issue for bugs or feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Security**: Email security issues privately to maintainers

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.
