# Multi-stage build for GenAI Boilerplate .NET Backend
# Stage 1: Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for better layer caching
COPY ["GenAIBoilerplate.sln", "./"]
COPY ["GenAIBoilerplate.API/GenAIBoilerplate.API.csproj", "GenAIBoilerplate.API/"]
COPY ["GenAIBoilerplate.Application/GenAIBoilerplate.Application.csproj", "GenAIBoilerplate.Application/"]
COPY ["GenAIBoilerplate.Infrastructure/GenAIBoilerplate.Infrastructure.csproj", "GenAIBoilerplate.Infrastructure/"]
COPY ["GenAIBoilerplate.Core/GenAIBoilerplate.Core.csproj", "GenAIBoilerplate.Core/"]

# Restore dependencies (cached if project files haven't changed)
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Stage 2: Publish application
FROM build AS publish
RUN dotnet publish "GenAIBoilerplate.API/GenAIBoilerplate.API.csproj" \
    -c Release \
    --no-build \
    -o /app/publish \
    --self-contained false \
    /p:UseAppHost=false

# Stage 3: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install security updates, curl for health checks, and clean up
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y --no-install-recommends curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set working directory
WORKDIR /app

# Create directories with proper permissions
RUN mkdir -p /app/data/{documents,embeddings,uploads} && \
    chown -R appuser:appuser /app

# Copy published application with proper ownership
COPY --from=publish --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
    DOTNET_EnableDiagnostics=0

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Add labels for metadata
LABEL org.opencontainers.image.title="GenAI Boilerplate .NET Backend" \
      org.opencontainers.image.description="ASP.NET Core Web API for GenAI Chatbot Platform" \
      org.opencontainers.image.vendor="Nithin Mohan T K" \
      org.opencontainers.image.source="https://github.com/nithinmohantk/genai-boilerplate-dotnet"

# Start application
ENTRYPOINT ["dotnet", "GenAIBoilerplate.API.dll"]
