using GenAIBoilerplate.Application.DTOs;

namespace GenAIBoilerplate.Application.Interfaces;

/// <summary>
/// AI service interface for chat completions
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Get available AI models for tenant
    /// </summary>
    Task<List<AIModelDto>> GetAvailableModelsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate chat completion
    /// </summary>
    Task<ChatCompletionResponseDto> GenerateChatCompletionAsync(
        Guid tenantId,
        ChatCompletionRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate streaming chat completion
    /// </summary>
    IAsyncEnumerable<string> GenerateStreamingChatCompletionAsync(
        Guid tenantId,
        ChatCompletionRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count tokens in text
    /// </summary>
    Task<int> CountTokensAsync(string text, string? modelName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if model is available for tenant
    /// </summary>
    Task<bool> IsModelAvailableAsync(Guid tenantId, string modelName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get chat response (backward compatibility method)
    /// </summary>
    Task<string> GetChatResponseAsync(
        List<ChatMessageDto> messages,
        string? modelName = null,
        float? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get streaming chat response (backward compatibility method)
    /// </summary>
    Task GetChatResponseStreamAsync(
        List<ChatMessageDto> messages,
        string? modelName = null,
        float? temperature = null,
        int? maxTokens = null,
        Func<string, Task> onChunkReceived = null!,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// AI provider interface
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Provider name (e.g., "openai", "anthropic")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Get supported models
    /// </summary>
    List<AIModelDto> GetSupportedModels();

    /// <summary>
    /// Generate chat completion
    /// </summary>
    Task<ChatCompletionResponseDto> GenerateChatCompletionAsync(
        string apiKey,
        ChatCompletionRequestDto request,
        List<ChatMessageDto> chatHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate streaming chat completion
    /// </summary>
    IAsyncEnumerable<string> GenerateStreamingChatCompletionAsync(
        string apiKey,
        ChatCompletionRequestDto request,
        List<ChatMessageDto> chatHistory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count tokens
    /// </summary>
    Task<int> CountTokensAsync(string text, string? modelName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if model is supported
    /// </summary>
    bool SupportsModel(string modelName);
}
