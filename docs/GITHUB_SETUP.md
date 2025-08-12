# GitHub Repository Setup Guide

This document outlines the recommended GitHub repository settings for the GenAI Boilerplate .NET project.

## 🔒 Branch Protection Rules

### Main Branch Protection
Configure the following settings for the `main` branch:

1. **Require a pull request before merging**
   - ✅ Require approvals: 2
   - ✅ Dismiss stale pull request approvals when new commits are pushed
   - ✅ Require review from code owners
   - ✅ Restrict pushes that create files larger than 100 MB

2. **Require status checks to pass before merging**
   - ✅ Require branches to be up to date before merging
   - Required status checks:
     - `Backend Tests`
     - `Frontend Tests`
     - `Docker Build Test`
     - `Code Quality`

3. **Require conversation resolution before merging**
   - ✅ Enabled

4. **Restrict pushes to matching branches**
   - Only maintainers and admins

5. **Allow force pushes**
   - ❌ Disabled

6. **Allow deletions**
   - ❌ Disabled

### Develop Branch Protection
Configure the following settings for the `develop` branch:

1. **Require a pull request before merging**
   - ✅ Require approvals: 1
   - ✅ Dismiss stale pull request approvals when new commits are pushed

2. **Require status checks to pass before merging**
   - ✅ Require branches to be up to date before merging
   - Required status checks:
     - `Backend Tests`
     - `Frontend Tests`
     - `Docker Build Test`

3. **Allow force pushes**
   - ❌ Disabled

## 🌍 Environments

### Staging Environment
- **Reviewers**: Add team members who can approve staging deployments
- **Deployment branches**: `develop`, `release/*`, `hotfix/*`
- **Environment secrets**:
  - `DATABASE_URL`
  - `REDIS_URL`
  - `OPENAI_API_KEY`
  - `JWT_SECRET`

### Production Environment
- **Reviewers**: Add senior team members/maintainers only
- **Deployment branches**: `main`
- **Protection rules**:
  - Required reviewers: 2
  - Wait timer: 5 minutes
- **Environment secrets**:
  - `DATABASE_URL`
  - `REDIS_URL`
  - `OPENAI_API_KEY`
  - `JWT_SECRET`

## 🏷️ Repository Topics/Tags

Add these topics to help with discovery:
- `dotnet`
- `aspnetcore`
- `react`
- `genai`
- `boilerplate`
- `openai`
- `postgresql`
- `redis`
- `jwt`
- `multitenant`
- `signalr`
- `docker`

## 📊 Repository Settings

### General Settings
- **Features**:
  - ✅ Wikis
  - ✅ Issues
  - ✅ Sponsorships
  - ✅ Preserve this repository
  - ✅ Discussions

- **Pull Requests**:
  - ✅ Allow merge commits
  - ✅ Allow squash merging (recommended)
  - ❌ Allow rebase merging
  - ✅ Always suggest updating pull request branches
  - ✅ Allow auto-merge
  - ✅ Automatically delete head branches

### Security
- **Code scanning**:
  - ✅ Enable CodeQL analysis
  - ✅ Enable dependency scanning

- **Secret scanning**:
  - ✅ Enable secret scanning
  - ✅ Enable push protection

## 🤖 GitHub Apps & Integrations

### Recommended Apps
1. **CodeCov** - Code coverage reporting
2. **Snyk** - Security vulnerability scanning
3. **SonarCloud** - Code quality analysis
4. **Dependabot** - Dependency updates

### Webhook Setup
Configure webhooks for:
- Deployment notifications
- Slack/Teams integration
- Project management tools

## 🔧 Actions & Secrets

### Repository Secrets
Add these secrets to your repository:

```
# Third-party services
SNYK_TOKEN=your_snyk_token
SONAR_TOKEN=your_sonar_token
CODECOV_TOKEN=your_codecov_token

# Deployment secrets (if using external hosting)
DOCKER_REGISTRY_TOKEN=your_registry_token
DEPLOY_KEY=your_deploy_key

# Notification tokens
SLACK_WEBHOOK_URL=your_slack_webhook
TEAMS_WEBHOOK_URL=your_teams_webhook
```

### Actions Permissions
- **Actions permissions**: Allow all actions and reusable workflows
- **Fork pull request workflows**: Require approval for all outside collaborators
- **Workflow permissions**: Read and write permissions

## 📋 Issue & PR Templates

The repository includes:
- Bug report template
- Feature request template
- Pull request template
- Contributing guidelines

## 🎯 Recommended Labels

Create these labels for better issue/PR management:

### Type Labels
- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `question` - Further information is requested
- `duplicate` - This issue or pull request already exists
- `invalid` - This doesn't seem right
- `wontfix` - This will not be worked on

### Priority Labels
- `priority: critical` - Critical issues
- `priority: high` - High priority
- `priority: medium` - Medium priority
- `priority: low` - Low priority

### Component Labels
- `backend` - Backend .NET changes
- `frontend` - Frontend React changes
- `docker` - Docker/containerization
- `database` - Database related
- `auth` - Authentication/authorization
- `ai` - AI/OpenAI integration
- `ci/cd` - Continuous integration/deployment

### Status Labels
- `status: needs-review` - Awaiting review
- `status: in-progress` - Currently being worked on
- `status: blocked` - Blocked by something
- `status: ready-for-test` - Ready for testing

## 🔄 Automation Rules

Set up GitHub Actions or GitHub App automations for:
- Auto-assign PRs to team members
- Auto-label PRs based on files changed
- Auto-close stale issues
- Auto-update dependencies
- Notify on security alerts
