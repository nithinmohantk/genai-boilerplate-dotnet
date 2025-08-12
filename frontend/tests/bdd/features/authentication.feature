Feature: Frontend User Authentication
    As a user visiting the GenAI chatbot application
    I want to be able to authenticate through the web interface
    So that I can access the chatbot functionality securely

    Background:
        Given the application is loaded in the browser
        And the backend API is available

    Scenario: User can see the login form
        Given I am on the login page
        Then I should see the login form
        And I should see email input field
        And I should see password input field
        And I should see the login button
        And I should see a link to the registration page

    Scenario: Successful user login through UI
        Given I am on the login page
        And a user exists with email "user@example.com" and password "SecurePassword123!"
        When I enter "user@example.com" in the email field
        And I enter "SecurePassword123!" in the password field
        And I click the login button
        Then I should be redirected to the chat page
        And I should see a welcome message
        And the navigation should show logout option
        And my authentication token should be stored

    Scenario: Login with invalid credentials shows error
        Given I am on the login page
        When I enter "user@example.com" in the email field
        And I enter "WrongPassword" in the password field
        And I click the login button
        Then I should see an error message "Invalid credentials"
        And I should remain on the login page
        And the password field should be cleared

    Scenario: Login form validation
        Given I am on the login page
        When I click the login button without entering credentials
        Then I should see validation errors for required fields
        And the form should not be submitted

        When I enter "invalid-email" in the email field
        And I click the login button
        Then I should see an email format validation error

    Scenario: User registration through UI
        Given I am on the registration page
        When I fill out the registration form with valid data:
            | field    | value                |
            | name     | Test User            |
            | email    | test@example.com     |
            | password | SecurePassword123!   |
        And I click the register button
        Then I should see a success message
        And I should be redirected to the login page
        And I should see a notification about email verification

    Scenario: Registration form validation
        Given I am on the registration page
        When I submit the form with invalid data:
            | field    | value        | error_expected                    |
            | name     |              | Name is required                  |
            | email    | invalid      | Please enter a valid email        |
            | password | weak         | Password must be at least 8 chars |
        Then I should see the respective validation errors
        And the form should not be submitted

    Scenario: Password visibility toggle
        Given I am on the login page
        When I enter "mypassword" in the password field
        Then the password should be hidden by default
        When I click the password visibility toggle
        Then the password should be visible
        When I click the password visibility toggle again
        Then the password should be hidden again

    Scenario: Remember me functionality
        Given I am on the login page
        And I check the "Remember me" checkbox
        When I login with valid credentials
        And I close and reopen the browser
        And I visit the application
        Then I should still be logged in

    Scenario: Logout functionality
        Given I am logged in as a user
        And I am on the chat page
        When I click the logout button
        Then I should be redirected to the login page
        And my authentication token should be removed
        And I should not be able to access protected pages

    Scenario: Auto-logout on token expiration
        Given I am logged in as a user
        And my authentication token expires
        When I try to perform an authenticated action
        Then I should see a session expired message
        And I should be redirected to the login page

    Scenario: Protected route access without authentication
        Given I am not logged in
        When I try to access the chat page directly
        Then I should be redirected to the login page
        And I should see a message about authentication required

    Scenario: Accessing already authenticated state
        Given I am already logged in
        When I try to access the login page
        Then I should be redirected to the chat page

    Scenario: Social login buttons display (if configured)
        Given social login is enabled
        And I am on the login page
        Then I should see Google login button
        And I should see Microsoft login button
        And I should see GitHub login button
        And I should see Apple login button

    Scenario: Google OAuth login flow
        Given social login is enabled
        And I am on the login page
        When I click the Google login button
        Then I should be redirected to Google OAuth
        And after successful Google authentication
        Then I should be redirected back to the application
        And I should be logged in
        And I should see the chat page

    Scenario: Loading states during authentication
        Given I am on the login page
        When I enter valid credentials and submit
        Then I should see a loading spinner
        And the submit button should be disabled
        And I should see "Signing in..." text

        When the authentication completes successfully
        Then the loading state should disappear
        And I should be redirected to the chat page

    Scenario: Network error handling during login
        Given I am on the login page
        And the backend is unavailable
        When I try to login with valid credentials
        Then I should see a network error message
        And I should be able to retry the login

    Scenario: Password reset request
        Given I am on the login page
        When I click the "Forgot password?" link
        Then I should be taken to the password reset page
        When I enter my email "user@example.com"
        And I click the "Send reset link" button
        Then I should see a confirmation message
        And I should receive an email with reset instructions

    Scenario: Theme persistence across login sessions
        Given I have selected the dark theme
        And I logout and login again
        Then my theme preference should be preserved
        And the interface should display in dark theme

    Scenario: Multi-factor authentication (if enabled)
        Given MFA is enabled for my account
        And I login with correct username and password
        Then I should see the MFA verification page
        When I enter the correct MFA code
        Then I should be logged in successfully
        When I enter an incorrect MFA code
        Then I should see an error message
        And I should be able to retry

    Scenario: Account lockout notification
        Given I have made 5 failed login attempts
        When I try to login again
        Then I should see an account locked message
        And I should see information about when I can try again
        And the login form should be disabled temporarily

    Scenario: Accessibility features for authentication
        Given I am using screen reader software
        When I navigate to the login page
        Then all form fields should have proper labels
        And error messages should be announced
        And the form should be keyboard navigable
        And focus should be managed properly

    Scenario: Mobile responsive authentication
        Given I am using a mobile device
        When I access the login page
        Then the form should be mobile-friendly
        And touch targets should be appropriately sized
        And the virtual keyboard should not obscure form fields
