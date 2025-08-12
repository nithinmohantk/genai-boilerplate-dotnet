# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["GenAIBoilerplate.API/GenAIBoilerplate.API.csproj", "GenAIBoilerplate.API/"]
COPY ["GenAIBoilerplate.Application/GenAIBoilerplate.Application.csproj", "GenAIBoilerplate.Application/"]
COPY ["GenAIBoilerplate.Infrastructure/GenAIBoilerplate.Infrastructure.csproj", "GenAIBoilerplate.Infrastructure/"]
COPY ["GenAIBoilerplate.Core/GenAIBoilerplate.Core.csproj", "GenAIBoilerplate.Core/"]
RUN dotnet restore "GenAIBoilerplate.API/GenAIBoilerplate.API.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/GenAIBoilerplate.API"
RUN dotnet build "GenAIBoilerplate.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GenAIBoilerplate.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create data directories
RUN mkdir -p /app/data/documents /app/data/embeddings /app/data/uploads

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "GenAIBoilerplate.API.dll"]
