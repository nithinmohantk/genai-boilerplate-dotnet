Feature: User Authentication
    As a user of the GenAI platform
    I want to authenticate securely
    So that I can access my personalized AI chatbot experience

Background:
    Given the GenAI platform is running
    And the database is clean

@authentication @smoke
Scenario: Successful user registration
    Given I am a new user
    When I register with valid credentials:
        | Email               | Password      | FirstName | LastName |
        | user@example.com    | SecurePass123 | John      | Doe      |
    Then I should receive a successful registration response
    And I should receive a JWT access token
    And I should receive a refresh token
    And my user account should be created in the database

@authentication @smoke
Scenario: Successful user login
    Given I have an existing user account:
        | Email            | Password      | FirstName | LastName |
        | user@example.com | SecurePass123 | John      | Doe      |
    When I login with valid credentials:
        | Email            | Password      |
        | user@example.com | SecurePass123 |
    Then I should receive a successful login response
    And I should receive a JWT access token
    And I should receive a refresh token
    And the access token should contain valid user claims

@authentication @security
Scenario: Failed login with invalid credentials
    Given I have an existing user account:
        | Email            | Password      | FirstName | LastName |
        | user@example.com | SecurePass123 | John      | Doe      |
    When I login with invalid credentials:
        | Email            | Password    |
        | user@example.com | WrongPass123 |
    Then I should receive an authentication error
    And I should not receive any tokens
    And the error message should indicate invalid credentials

@authentication @security
Scenario: Registration with duplicate email
    Given I have an existing user account:
        | Email            | Password      | FirstName | LastName |
        | user@example.com | SecurePass123 | John      | Doe      |
    When I attempt to register with the same email:
        | Email            | Password      | FirstName | LastName |
        | user@example.com | NewPass123    | Jane      | Smith    |
    Then I should receive a validation error
    And the error message should indicate email already exists
    And no new user account should be created

@authentication @validation
Scenario Outline: Registration with invalid data
    Given I am a new user
    When I register with invalid credentials:
        | Email   | Password   | FirstName   | LastName   |
        | <Email> | <Password> | <FirstName> | <LastName> |
    Then I should receive a validation error
    And the error message should contain "<ExpectedError>"

    Examples:
        | Email              | Password   | FirstName | LastName | ExpectedError          |
        | invalid-email      | SecurePass | John      | Doe      | Invalid email          |
        | user@example.com   | weak       | John      | Doe      | Password must be       |
        | user@example.com   | SecurePass |           | Doe      | First name is required |
        | user@example.com   | SecurePass | John      |          | Last name is required  |

@authentication @tokens
Scenario: Token refresh with valid refresh token
    Given I have a logged-in user with valid tokens
    When I request a token refresh using the refresh token
    Then I should receive a new access token
    And I should receive a new refresh token
    And the old refresh token should be invalidated
    And the new access token should contain valid user claims

@authentication @tokens
Scenario: Token refresh with invalid refresh token
    Given I have an invalid or expired refresh token
    When I request a token refresh using the invalid token
    Then I should receive an authentication error
    And I should not receive any new tokens
    And the error message should indicate invalid refresh token

@authentication @security
Scenario: Access protected endpoint with valid token
    Given I have a logged-in user with valid tokens
    When I access a protected endpoint with the access token
    Then I should receive a successful response
    And the response should contain user-specific data

@authentication @security
Scenario: Access protected endpoint without token
    When I access a protected endpoint without any token
    Then I should receive an unauthorized error
    And the response status should be 401

@authentication @security
Scenario: Access protected endpoint with expired token
    Given I have an expired access token
    When I access a protected endpoint with the expired token
    Then I should receive an unauthorized error
    And the response status should be 401
    And the error message should indicate token has expired

@authentication @logout
Scenario: User logout with token revocation
    Given I have a logged-in user with valid tokens
    When I logout from the system
    Then I should receive a successful logout response
    And my access token should be revoked
    And my refresh token should be revoked

@authentication @profile
Scenario: Get current user profile
    Given I have a logged-in user with valid tokens
    When I request my user profile
    Then I should receive my profile information
    And the profile should contain:
        | Field     | Value            |
        | Email     | user@example.com |
        | FirstName | John             |
        | LastName  | Doe              |
        | Role      | User             |

@authentication @profile
Scenario: Update user profile
    Given I have a logged-in user with valid tokens
    When I update my profile with new information:
        | FirstName | LastName | Phone      |
        | Johnny    | Doe      | 1234567890 |
    Then I should receive a successful update response
    And my profile should be updated in the database
    And when I request my profile again, it should show the updated information
