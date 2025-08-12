using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using GenAIBoilerplate.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GenAIBoilerplate.Infrastructure.Services;

/// <summary>
/// AI service implementation that aggregates AI providers
/// </summary>
public class AIService : IAIService
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AIService> _logger;

    public AIService(
        IEnumerable<IAIProvider> providers,
        IUnitOfWork unitOfWork,
        ILogger<AIService> logger)
    {
        _providers = providers;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<AIModelDto>> GetAvailableModelsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var models = new List<AIModelDto>();
        
        foreach (var provider in _providers)
        {
            models.AddRange(provider.GetSupportedModels());
        }

        return models;
    }

    public async Task<ChatCompletionResponseDto> GenerateChatCompletionAsync(
        Guid tenantId, 
        ChatCompletionRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // Get tenant API key for the model provider
        // For now, use the first available provider
        var provider = _providers.FirstOrDefault();
        if (provider == null)
        {
            throw new InvalidOperationException("No AI providers available");
        }

        // In production, get actual API key for the tenant
        var apiKey = "dummy-key"; // This should come from tenant configuration

        return await provider.GenerateChatCompletionAsync(apiKey, request, new List<ChatMessageDto>(), cancellationToken);
    }

    public async IAsyncEnumerable<string> GenerateStreamingChatCompletionAsync(
        Guid tenantId,
        ChatCompletionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault();
        if (provider == null)
        {
            throw new InvalidOperationException("No AI providers available");
        }

        var apiKey = "dummy-key"; // This should come from tenant configuration

        await foreach (var chunk in provider.GenerateStreamingChatCompletionAsync(apiKey, request, new List<ChatMessageDto>(), cancellationToken))
        {
            yield return chunk;
        }
    }

    public async Task<int> CountTokensAsync(string text, string? modelName = null, CancellationToken cancellationToken = default)
    {
        var provider = _providers.FirstOrDefault(p => string.IsNullOrEmpty(modelName) || p.SupportsModel(modelName));
        if (provider == null)
        {
            // Fallback to rough estimation
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        return await provider.CountTokensAsync(text, modelName, cancellationToken);
    }

    public async Task<bool> IsModelAvailableAsync(Guid tenantId, string modelName, CancellationToken cancellationToken = default)
    {
        return _providers.Any(p => p.SupportsModel(modelName));
    }

    public async Task<string> GetChatResponseAsync(
        List<ChatMessageDto> messages,
        string? modelName = null,
        float? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequestDto
        {
            SessionId = Guid.Empty,
            Message = messages.LastOrDefault()?.Content ?? "",
            ModelName = modelName,
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        var response = await GenerateChatCompletionAsync(Guid.Empty, request, cancellationToken);
        return response.Content;
    }

    public async Task GetChatResponseStreamAsync(
        List<ChatMessageDto> messages,
        string? modelName = null,
        float? temperature = null,
        int? maxTokens = null,
        Func<string, Task> onChunkReceived = null!,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatCompletionRequestDto
        {
            SessionId = Guid.Empty,
            Message = messages.LastOrDefault()?.Content ?? "",
            ModelName = modelName,
            Temperature = temperature,
            MaxTokens = maxTokens
        };

        await foreach (var chunk in GenerateStreamingChatCompletionAsync(Guid.Empty, request, cancellationToken))
        {
            if (onChunkReceived != null)
            {
                await onChunkReceived(chunk);
            }
        }
    }
}
