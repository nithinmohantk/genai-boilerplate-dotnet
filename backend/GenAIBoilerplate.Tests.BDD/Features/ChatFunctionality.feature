Feature: Chat Functionality
    As an authenticated user
    I want to have AI-powered conversations
    So that I can get assistance and information through natural language interaction

Background:
    Given the GenAI platform is running
    And the database is clean
    And I have a logged-in user with valid tokens

@chat @smoke
Scenario: Create a new chat session
    When I create a new chat session:
        | Title        | ModelName  | Description                    |
        | My First Chat| gpt-4      | A conversation about AI        |
    Then I should receive a successful session creation response
    And the session should be created in the database
    And the session should have the correct details:
        | Field       | Value                          |
        | Title       | My First Chat                  |
        | ModelName   | gpt-4                         |
        | Description | A conversation about AI        |
        | IsActive    | true                          |

@chat @smoke
Scenario: Get user's chat sessions
    Given I have created multiple chat sessions:
        | Title           | ModelName  | Description                |
        | AI Discussions  | gpt-4      | General AI conversations   |
        | Code Help       | gpt-3.5    | Programming assistance     |
        | Creative Writing| gpt-4      | Story and content creation |
    When I request my chat sessions
    Then I should receive all my chat sessions
    And the sessions should be ordered by creation date descending
    And each session should contain the correct metadata

@chat @messaging @smoke
Scenario: Send a message and receive AI response
    Given I have an active chat session with model "gpt-4"
    When I send a message:
        | Content                                    | Role |
        | Hello! Can you help me learn about AI?    | User |
    Then I should receive a successful message response
    And the message should be saved to the session
    And I should receive an AI response
    And the AI response should be saved to the session
    And both messages should have the correct metadata:
        | Field     | UserMessage | AIMessage   |
        | Role      | User        | Assistant   |
        | ModelName | gpt-4       | gpt-4       |

@chat @messaging
Scenario: Send multiple messages in a conversation
    Given I have an active chat session with model "gpt-4"
    And I have sent a previous message "Hello! Tell me about machine learning."
    And I received an AI response about machine learning
    When I send a follow-up message:
        | Content                                        | Role |
        | Can you explain neural networks specifically?  | User |
    Then I should receive a successful message response
    And the AI should respond contextually to the conversation history
    And the conversation history should be maintained in order
    And all messages should be associated with the correct session

@chat @models
Scenario: Switch AI model during conversation
    Given I have an active chat session with model "gpt-3.5"
    And I have sent some messages in the conversation
    When I update the session to use model "gpt-4":
        | ModelName | Description                    |
        | gpt-4     | Switched to more advanced model|
    Then the session should be updated successfully
    And subsequent messages should use the new model
    And the model change should be reflected in message metadata

@chat @streaming
Scenario: Receive streaming AI response
    Given I have an active chat session with streaming enabled
    When I send a message requesting a long response:
        | Content                                             | Role |
        | Write a detailed explanation of quantum computing   | User |
    Then I should receive a streaming response
    And the response should be delivered in chunks
    And each chunk should be valid
    And the complete response should be assembled correctly
    And the final response should be saved to the session

@chat @session-management
Scenario: Update chat session details
    Given I have an active chat session:
        | Title        | ModelName | Description           |
        | Original Title| gpt-4    | Original description  |
    When I update the session details:
        | Title         | Description            |
        | Updated Title | Updated description    |
    Then the session should be updated successfully
    And the updated details should be saved to the database
    And when I retrieve the session, it should show the new details

@chat @session-management
Scenario: Archive a chat session
    Given I have an active chat session with messages
    When I archive the chat session
    Then the session should be marked as inactive
    And the session should still be accessible for reading
    And I should not be able to send new messages to the archived session
    But the conversation history should remain intact

@chat @validation
Scenario: Send empty message
    Given I have an active chat session
    When I attempt to send an empty message
    Then I should receive a validation error
    And the error message should indicate message content is required
    And no message should be saved to the session

@chat @validation
Scenario: Send message to non-existent session
    When I attempt to send a message to a non-existent session:
        | SessionId                            | Content      |
        | 00000000-0000-0000-0000-000000000000 | Test message |
    Then I should receive a not found error
    And the error message should indicate session not found
    And no message should be saved

@chat @permissions
Scenario: Access another user's chat session
    Given another user has a chat session
    When I attempt to access their chat session
    Then I should receive a forbidden error
    And I should not be able to view their session details
    And I should not be able to send messages to their session

@chat @search
Scenario: Search chat sessions by title
    Given I have multiple chat sessions:
        | Title               | ModelName | Description                |
        | AI Research Project | gpt-4     | Academic research topics   |
        | Code Review Help    | gpt-3.5   | Programming assistance     |
        | Creative Writing    | gpt-4     | Story creation             |
    When I search for sessions with title containing "AI"
    Then I should receive matching sessions
    And the results should contain "AI Research Project"
    And the results should not contain unmatched sessions

@chat @search
Scenario: Search messages within sessions
    Given I have a chat session with multiple messages:
        | Content                              | Role      |
        | Tell me about machine learning       | User      |
        | Machine learning is a subset of AI  | Assistant |
        | What about deep learning?            | User      |
        | Deep learning uses neural networks   | Assistant |
    When I search for messages containing "neural networks"
    Then I should receive matching messages
    And the results should contain the relevant message
    And the context should show the surrounding conversation

@chat @error-handling
Scenario: Handle AI service unavailability
    Given the AI service is temporarily unavailable
    And I have an active chat session
    When I send a message:
        | Content                    | Role |
        | Can you help me with this? | User |
    Then my message should be saved successfully
    But I should receive a service unavailable error for the AI response
    And the error should indicate the AI service is temporarily down
    And I should be able to retry the request later

@chat @rate-limiting
Scenario: Handle rate limiting
    Given I have sent many messages in quick succession
    And I have reached the rate limit
    When I attempt to send another message:
        | Content            | Role |
        | One more message   | User |
    Then I should receive a rate limit error
    And the error should indicate I need to wait
    And the error should include when I can try again

@chat @export
Scenario: Export chat session conversation
    Given I have a chat session with multiple messages
    When I request to export the conversation
    Then I should receive the complete conversation history
    And the export should include all messages in chronological order
    And the export should include metadata for each message:
        | Field     | Required |
        | Timestamp | true     |
        | Role      | true     |
        | Content   | true     |
        | ModelName | true     |
