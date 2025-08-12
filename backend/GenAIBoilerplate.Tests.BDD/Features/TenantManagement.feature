Feature: Tenant Management
    As a system administrator
    I want to manage multiple tenants
    So that I can provide isolated environments for different organizations

Background:
    Given the GenAI platform is running
    And the database is clean

@tenant @admin @smoke
Scenario: Create a new tenant as super admin
    Given I am logged in as a super admin
    When I create a new tenant:
        | Name           | Description                    | Status |
        | Acme Corp      | Leading technology company     | Active |
    Then I should receive a successful tenant creation response
    And the tenant should be created in the database
    And the tenant should have the correct details:
        | Field       | Value                          |
        | Name        | Acme Corp                      |
        | Description | Leading technology company     |
        | Status      | Active                         |
    And a default API key should be generated for the tenant

@tenant @admin
Scenario: Get all tenants as super admin
    Given I am logged in as a super admin
    And I have created multiple tenants:
        | Name          | Description                | Status   |
        | Acme Corp     | Technology company         | Active   |
        | Beta Inc      | Startup company            | Active   |
        | Gamma LLC     | Consulting firm            | Inactive |
    When I request all tenants
    Then I should receive all tenants
    And the tenants should include system metadata:
        | Field          | Required |
        | Id             | true     |
        | Name           | true     |
        | Status         | true     |
        | CreatedAt      | true     |
        | UpdatedAt      | true     |
        | UserCount      | true     |

@tenant @admin
Scenario: Update tenant information
    Given I am logged in as a super admin
    And I have created a tenant:
        | Name      | Description           | Status |
        | Acme Corp | Technology company    | Active |
    When I update the tenant information:
        | Name              | Description                     | Status   |
        | Acme Corporation  | Leading technology enterprise   | Active   |
    Then the tenant should be updated successfully
    And the updated information should be saved to the database
    And when I retrieve the tenant, it should show the new details

@tenant @admin
Scenario: Deactivate a tenant
    Given I am logged in as a super admin
    And I have an active tenant with users and data
    When I deactivate the tenant
    Then the tenant status should be changed to Inactive
    And existing users should no longer be able to login
    And existing sessions should be invalidated
    But tenant data should be preserved for reactivation

@tenant @admin
Scenario: Reactivate a tenant
    Given I am logged in as a super admin
    And I have an inactive tenant
    When I reactivate the tenant
    Then the tenant status should be changed to Active
    And users should be able to login again
    And the tenant data should be accessible

@tenant @api-keys @smoke
Scenario: Generate API key for tenant
    Given I am logged in as a super admin
    And I have an active tenant
    When I generate a new API key for the tenant:
        | KeyName          | Provider  | Description                    |
        | OpenAI Primary   | OpenAI    | Main API key for OpenAI        |
    Then I should receive a successful API key creation response
    And the API key should be created in the database
    And the API key should be associated with the tenant
    And the API key should have the correct details:
        | Field       | Value              |
        | KeyName     | OpenAI Primary     |
        | Provider    | OpenAI             |
        | IsActive    | true               |

@tenant @api-keys
Scenario: List tenant API keys
    Given I am logged in as a super admin
    And I have a tenant with multiple API keys:
        | KeyName          | Provider    | Status   |
        | OpenAI Primary   | OpenAI      | Active   |
        | OpenAI Secondary | OpenAI      | Active   |
        | Anthropic Main   | Anthropic   | Inactive |
    When I request the tenant's API keys
    Then I should receive all API keys for the tenant
    And each API key should include metadata:
        | Field       | Required |
        | Id          | true     |
        | KeyName     | true     |
        | Provider    | true     |
        | IsActive    | true     |
        | CreatedAt   | true     |
        | LastUsed    | false    |

@tenant @api-keys
Scenario: Update API key status
    Given I am logged in as a super admin
    And I have a tenant with an active API key
    When I deactivate the API key
    Then the API key status should be changed to inactive
    And the API key should no longer be usable for AI requests
    And existing AI requests should fail with authentication errors

@tenant @api-keys
Scenario: Delete API key
    Given I am logged in as a super admin
    And I have a tenant with an API key
    When I delete the API key
    Then the API key should be removed from the database
    And the API key should no longer be usable
    And any cached references should be invalidated

@tenant @isolation @security
Scenario: Tenant data isolation verification
    Given I have two different tenants with users and chat sessions:
        | TenantName | UserEmail          | SessionTitle    |
        | Acme Corp  | user1@acme.com    | Acme Chat 1     |
        | Beta Inc   | user2@beta.com    | Beta Discussion |
    When I login as user1@acme.com
    And I request my chat sessions
    Then I should only see chat sessions from Acme Corp
    And I should not see any data from Beta Inc
    And the session isolation should be enforced at the database level

@tenant @permissions
Scenario: Tenant admin manages tenant users
    Given I am logged in as a tenant admin for "Acme Corp"
    And I have users in my tenant:
        | Email               | Role  | Status |
        | user1@acme.com      | User  | Active |
        | user2@acme.com      | User  | Active |
    When I request my tenant's users
    Then I should receive all users in my tenant
    And I should not see users from other tenants
    And each user should include tenant-specific information

@tenant @permissions
Scenario: Tenant admin cannot access other tenant data
    Given I am logged in as a tenant admin for "Acme Corp"
    And there are other tenants in the system
    When I attempt to access another tenant's information
    Then I should receive a forbidden error
    And I should not see any data from other tenants

@tenant @permissions
Scenario: Regular user cannot access tenant management
    Given I am logged in as a regular user
    When I attempt to access tenant management endpoints
    Then I should receive a forbidden error
    And I should not be able to view any tenant information

@tenant @settings
Scenario: Configure tenant-specific settings
    Given I am logged in as a tenant admin
    When I update tenant settings:
        | Setting              | Value                    |
        | MaxUsersPerTenant    | 100                      |
        | AllowedAIProviders   | OpenAI,Anthropic         |
        | DefaultModel         | gpt-4                    |
        | RateLimitPerUser     | 1000                     |
    Then the settings should be updated successfully
    And the settings should be applied to all users in the tenant
    And the settings should be enforced in API requests

@tenant @analytics @admin
Scenario: View tenant usage analytics
    Given I am logged in as a super admin
    And I have tenants with usage data:
        | TenantName | Users | Sessions | Messages | APIRequests |
        | Acme Corp  | 25    | 150      | 3000     | 2500        |
        | Beta Inc   | 10    | 75       | 1500     | 1200        |
    When I request tenant analytics
    Then I should receive usage statistics for all tenants
    And the analytics should include:
        | Metric              | Required |
        | UserCount           | true     |
        | ActiveSessions      | true     |
        | MessageCount        | true     |
        | APIRequestCount     | true     |
        | StorageUsage        | true     |
        | LastActivityDate    | true     |

@tenant @billing
Scenario: Calculate tenant usage for billing
    Given I am logged in as a super admin
    And I have a tenant with usage data over a billing period
    When I request billing information for the tenant
    Then I should receive detailed usage metrics
    And the billing data should include:
        | Metric                | Required |
        | TotalAPIRequests      | true     |
        | TotalMessages         | true     |
        | StorageUsedGB         | true     |
        | ActiveUserCount       | true     |
        | BillingPeriodStart    | true     |
        | BillingPeriodEnd      | true     |
        | EstimatedCost         | true     |

@tenant @validation
Scenario: Prevent duplicate tenant names
    Given I am logged in as a super admin
    And I have created a tenant named "Acme Corp"
    When I attempt to create another tenant with the same name:
        | Name      | Description        | Status |
        | Acme Corp | Different company  | Active |
    Then I should receive a validation error
    And the error message should indicate name already exists
    And no duplicate tenant should be created

@tenant @validation
Scenario: Validate required tenant fields
    Given I am logged in as a super admin
    When I attempt to create a tenant with missing required fields:
        | Name | Description | Status |
        |      | Test        | Active |
    Then I should receive a validation error
    And the error message should indicate required fields are missing
    And no tenant should be created

@tenant @data-retention
Scenario: Tenant data cleanup on deletion
    Given I am logged in as a super admin
    And I have a tenant with users, sessions, and messages
    When I delete the tenant with data cleanup option
    Then the tenant should be removed from the database
    And all associated user accounts should be deleted
    And all associated chat sessions should be deleted
    And all associated messages should be deleted
    And all API keys should be revoked and deleted

@tenant @migration
Scenario: Export tenant data
    Given I am logged in as a super admin
    And I have a tenant with complete data
    When I request to export the tenant data
    Then I should receive a complete data export
    And the export should include:
        | DataType     | Required |
        | Users        | true     |
        | ChatSessions | true     |
        | Messages     | true     |
        | APIKeys      | true     |
        | Settings     | true     |
    And the export should be in a standard format
    And sensitive data should be properly handled
