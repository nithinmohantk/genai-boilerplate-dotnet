using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using GenAIBoilerplate.Application.DTOs;
using GenAIBoilerplate.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GenAIBoilerplate.Infrastructure.AI;

/// <summary>
/// OpenAI API provider implementation
/// </summary>
public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIProvider> _logger;

    public OpenAIProvider(HttpClient httpClient, ILogger<OpenAIProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public string ProviderName => "openai";

    public List<AIModelDto> GetSupportedModels()
    {
        return new List<AIModelDto>
        {
            new() { Id = "gpt-4", Name = "GPT-4", Provider = ProviderName, Description = "Most capable GPT-4 model", MaxTokens = 8192, SupportsStreaming = true, IsAvailable = true },
            new() { Id = "gpt-4-turbo", Name = "GPT-4 Turbo", Provider = ProviderName, Description = "GPT-4 Turbo with improved speed and cost", MaxTokens = 128000, SupportsStreaming = true, IsAvailable = true },
            new() { Id = "gpt-4o", Name = "GPT-4o", Provider = ProviderName, Description = "GPT-4 Omni model", MaxTokens = 128000, SupportsStreaming = true, IsAvailable = true },
            new() { Id = "gpt-4o-mini", Name = "GPT-4o Mini", Provider = ProviderName, Description = "Faster and cheaper GPT-4o", MaxTokens = 128000, SupportsStreaming = true, IsAvailable = true },
            new() { Id = "gpt-3.5-turbo", Name = "GPT-3.5 Turbo", Provider = ProviderName, Description = "Fast and cost-effective model", MaxTokens = 16385, SupportsStreaming = true, IsAvailable = true },
            new() { Id = "gpt-3.5-turbo-16k", Name = "GPT-3.5 Turbo 16K", Provider = ProviderName, Description = "GPT-3.5 with extended context", MaxTokens = 16385, SupportsStreaming = true, IsAvailable = true }
        };
    }

    public async Task<ChatCompletionResponseDto> GenerateChatCompletionAsync(
        string apiKey,
        ChatCompletionRequestDto request,
        List<ChatMessageDto> chatHistory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = ConvertToOpenAIMessages(chatHistory, request.Message);
            
            var requestBody = new
            {
                model = request.ModelName ?? "gpt-3.5-turbo",
                messages = messages,
                max_tokens = request.MaxTokens ?? 1000,
                temperature = request.Temperature ?? 0.7,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, error);
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

            if (result?.Choices?.Count > 0)
            {
                return new ChatCompletionResponseDto
                {
                    MessageId = Guid.NewGuid(),
                    Content = result.Choices[0].Message?.Content ?? "",
                    ModelName = request.ModelName,
                    TokenCount = result.Usage?.TotalTokens,
                    CreatedAt = DateTime.UtcNow,
                    Metadata = request.Metadata
                };
            }

            throw new InvalidOperationException("No response from OpenAI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OpenAI completion");
            throw;
        }
    }

    public async IAsyncEnumerable<string> GenerateStreamingChatCompletionAsync(
        string apiKey,
        ChatCompletionRequestDto request,
        List<ChatMessageDto> chatHistory,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = ConvertToOpenAIMessages(chatHistory, request.Message);
        
        var requestBody = new
        {
            model = request.ModelName ?? "gpt-3.5-turbo",
            messages = messages,
            max_tokens = request.MaxTokens ?? 1000,
            temperature = request.Temperature ?? 0.7,
            stream = true
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = content
        };
        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, error);
            throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..];
            if (data == "[DONE]")
                break;

            OpenAIStreamResponse? streamResponse;
            try
            {
                streamResponse = JsonSerializer.Deserialize<OpenAIStreamResponse>(data);
            }
            catch (JsonException)
            {
                // Skip malformed JSON
                continue;
            }
            
            var delta = streamResponse?.Choices?[0]?.Delta?.Content;
            if (!string.IsNullOrEmpty(delta))
            {
                yield return delta;
            }
        }
    }

    public Task<int> CountTokensAsync(string text, string? modelName = null, CancellationToken cancellationToken = default)
    {
        // Rough estimation: 1 token ≈ 4 characters for English text
        // This is a simplification - in production, use a proper tokenizer
        var estimatedTokens = (int)Math.Ceiling(text.Length / 4.0);
        return Task.FromResult(estimatedTokens);
    }

    public bool SupportsModel(string modelName)
    {
        return GetSupportedModels().Any(m => m.Id.Equals(modelName, StringComparison.OrdinalIgnoreCase));
    }

    private static List<object> ConvertToOpenAIMessages(List<ChatMessageDto> chatHistory, string newMessage)
    {
        var messages = new List<object>();

        // Add chat history
        foreach (var msg in chatHistory.Where(m => !m.IsDeleted).OrderBy(m => m.CreatedAt))
        {
            var role = msg.Role.ToLowerInvariant() switch
            {
                "user" => "user",
                "assistant" => "assistant",
                "system" => "system",
                _ => "user"
            };

            messages.Add(new { role, content = msg.Content });
        }

        // Add new message
        messages.Add(new { role = "user", content = newMessage });

        return messages;
    }
}

// OpenAI API response models
public class OpenAIResponse
{
    public List<OpenAIChoice>? Choices { get; set; }
    public OpenAIUsage? Usage { get; set; }
}

public class OpenAIChoice
{
    public OpenAIMessage? Message { get; set; }
    public string? FinishReason { get; set; }
}

public class OpenAIMessage
{
    public string? Role { get; set; }
    public string? Content { get; set; }
}

public class OpenAIUsage
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}

public class OpenAIStreamResponse
{
    public List<OpenAIStreamChoice>? Choices { get; set; }
}

public class OpenAIStreamChoice
{
    public OpenAIDelta? Delta { get; set; }
}

public class OpenAIDelta
{
    public string? Content { get; set; }
}
