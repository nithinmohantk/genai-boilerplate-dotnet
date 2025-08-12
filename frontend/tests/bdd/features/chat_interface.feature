Feature: Chat Interface
    As an authenticated user
    I want to interact with the chat interface
    So that I can have conversations with AI models

    Background:
        Given I am logged in as a user
        And I am on the chat page
        And AI models are available

    Scenario: Initial chat interface display
        Given I am on an empty chat page
        Then I should see the chat input field
        And I should see a send button
        And I should see model selection dropdown
        And I should see a welcome message or placeholder
        And I should see the sidebar with chat history
        And I should see a new chat button

    Scenario: Send a message in the chat
        Given I have an active chat session
        When I type "Hello, how are you?" in the chat input
        And I click the send button
        Then my message should appear in the chat history
        And the message should be marked as from "user"
        And I should see a loading indicator for the AI response
        And after a moment, I should see the AI response
        And the AI message should be marked as from "assistant"

    Scenario: Send message with Enter key
        Given I have an active chat session
        When I type "What is the weather today?" in the chat input
        And I press the Enter key
        Then the message should be sent
        And my message should appear in the chat history

    Scenario: Send message with Shift+Enter creates new line
        Given I have an active chat session
        When I type "First line" in the chat input
        And I press Shift+Enter
        And I type "Second line"
        Then I should see both lines in the input field
        When I press Enter
        Then the multi-line message should be sent

    Scenario: Model selection and switching
        Given I am on the chat page
        Then I should see the model dropdown with options:
            | model    | display_name |
            | gpt-4    | GPT-4        |
            | gpt-3.5  | GPT-3.5      |
            | claude   | Claude       |
        When I select "Claude" from the model dropdown
        Then the selected model should be "Claude"
        And future messages should use the Claude model

    Scenario: Chat history sidebar
        Given I have multiple chat sessions:
            | title              | last_message_time | message_count |
            | Python Help        | 2 hours ago       | 15            |
            | Creative Writing   | 1 day ago         | 8             |
            | Code Review        | 3 days ago        | 12            |
        Then I should see all chat sessions in the sidebar
        And they should be ordered by most recent activity
        And each session should show the title and last activity time

    Scenario: Switch between chat sessions
        Given I have multiple chat sessions in the sidebar
        And I am currently in "Python Help" session
        When I click on "Creative Writing" in the sidebar
        Then I should switch to the Creative Writing session
        And I should see the message history for that session
        And the sidebar should highlight the active session

    Scenario: Create a new chat session
        Given I am on the chat page
        When I click the "New Chat" button
        Then a new chat session should be created
        And I should see an empty chat area
        And the session should appear in the sidebar
        And it should be titled "New Chat" or similar

    Scenario: Edit chat session title
        Given I have a chat session titled "New Chat"
        When I hover over the session title in the sidebar
        And I click the edit icon
        Then I should see an input field to edit the title
        When I change the title to "My Custom Chat"
        And I press Enter or click save
        Then the session title should be updated
        And it should show "My Custom Chat" in the sidebar

    Scenario: Delete a chat session
        Given I have a chat session in the sidebar
        When I hover over the session
        And I click the delete icon
        Then I should see a confirmation dialog
        When I confirm the deletion
        Then the session should be removed from the sidebar
        And if it was the active session, I should be switched to another session or shown empty state

    Scenario: Message timestamps and formatting
        Given I have an active chat session
        When I send several messages over time
        Then each message should show a timestamp
        And timestamps should be formatted appropriately (e.g., "2 minutes ago", "Today at 2:30 PM")
        And messages from the same time period should be grouped visually

    Scenario: Copy message content
        Given I have a chat session with AI responses
        When I hover over an AI message
        And I click the copy button
        Then the message content should be copied to clipboard
        And I should see a brief "Copied!" confirmation

    Scenario: Message markdown rendering
        Given I have an active chat session
        When the AI responds with markdown content like:
            """
            Here's some **bold text** and *italic text*.
            
            ```python
            def hello():
                print("Hello, world!")
            ```
            
            - List item 1
            - List item 2
            """
        Then the message should render with proper formatting:
            | element        | appearance              |
            | bold text      | Bold styling            |
            | italic text    | Italic styling          |
            | code block     | Syntax highlighted      |
            | list items     | Bulleted list           |

    Scenario: Responsive design on mobile
        Given I am using a mobile device
        And I am on the chat page
        Then the chat interface should be mobile-friendly
        And the sidebar should be collapsible or overlay
        And the input field should be easily accessible
        And touch targets should be appropriately sized
        And the virtual keyboard should not obscure the input

    Scenario: Auto-scroll behavior
        Given I have a chat session with many messages
        When new messages arrive
        Then the chat should auto-scroll to the bottom
        But if I have scrolled up manually
        Then new messages should not auto-scroll
        And I should see a "New messages" indicator
        When I click the "scroll to bottom" button
        Then it should scroll to the latest message

    Scenario: Message loading states
        Given I send a message to the AI
        Then I should see my message immediately
        And I should see a typing indicator from the AI
        When the AI response is ready
        Then the typing indicator should disappear
        And the AI message should appear
        
    Scenario: Error handling for failed messages
        Given the backend is temporarily unavailable
        When I try to send a message
        Then I should see an error indicator on my message
        And I should see a retry button
        When I click retry
        Then the message should be resent

    Scenario: File upload in chat
        Given I have an active chat session
        When I click the attachment button
        And I select a PDF file
        Then the file should be uploaded
        And I should see a file preview in the chat
        When I send the message with the file
        Then the AI should process the file content
        And respond with relevant information about the file

    Scenario: Voice message recording (if enabled)
        Given voice messages are enabled
        And I have an active chat session
        When I press and hold the microphone button
        Then voice recording should start
        And I should see a recording indicator
        When I release the button
        Then the voice message should be sent
        And it should be transcribed and sent to the AI

    Scenario: Theme switching in chat interface
        Given I am on the chat page
        When I switch to dark theme
        Then the chat interface should update to dark theme
        And message colors should be appropriate for dark mode
        And all UI elements should be properly themed

    Scenario: Keyboard shortcuts
        Given I am on the chat page
        When I press Ctrl+N (or Cmd+N on Mac)
        Then a new chat should be created
        When I press Ctrl+/ (or Cmd+/ on Mac)
        Then I should see a shortcuts help dialog

    Scenario: Search within chat session
        Given I have a chat session with many messages
        When I open the search function
        And I search for "Python function"
        Then I should see highlighted results
        And I should be able to navigate between matches
        And the chat should scroll to show the highlighted results

    Scenario: Export chat conversation
        Given I have a chat session with several messages
        When I click the export button
        And I select "PDF" format
        Then I should be able to download the conversation as a PDF
        With proper formatting and timestamps

    Scenario: Accessibility features for chat
        Given I am using screen reader software
        When I navigate the chat interface
        Then new messages should be announced
        And I should be able to navigate messages with keyboard
        And all interactive elements should be properly labeled
        And focus management should work correctly

    Scenario: Real-time typing indicators
        Given I have a WebSocket connection
        And I am in a chat session
        When I start typing in the input field
        Then other users (if applicable) should see a typing indicator
        When I stop typing
        Then the typing indicator should disappear after a delay

    Scenario: Message reactions (if enabled)
        Given message reactions are enabled
        And I have a chat session with AI responses
        When I hover over an AI message
        And I click the reaction button
        Then I should see reaction options (👍, 👎, ❤️, etc.)
        When I select a reaction
        Then it should be added to the message
        And provide feedback for improving AI responses
