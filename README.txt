# Toucans Todo API

A robust, feature-rich API built with .NET for managing todo lists and tasks with sharing capabilities.

## Overview

Toucans API provides a comprehensive backend solution for todo applications with:

- Todo list management
- Task tracking with status
- List sharing between users
- Pagination support
- Advanced filtering and search
- User management

## Key Features

- **User Management**
  - Email-based user accounts
  - Profile management with first/last name
  - User search and filtering

- **Todo Lists**
  - Create and manage multiple lists
  - List descriptions and metadata
  - Track creation and modification dates
  - Item count and completion statistics

- **Sharing System**
  - Share lists with other users
  - Permission-based access control
  - Track shared list relationships

- **Advanced Querying**
  - Pagination with configurable page sizes
  - Search across multiple fields
  - Date range filtering
  - Sort capabilities

## Technical Implementation

- Built on .NET isolated Azure Functions
- Entity Framework Core for data access
- SQL Server database backend
- CQRS-inspired DTO pattern
- Extension method utilities

## Roadmap

### Authentication & Authorization
- [ ] Implement Azure AD B2C integration
- [ ] Role-based access control (RBAC)
- [ ] JWT token validation
- [ ] API key management

### Logging & Monitoring
- [ ] Application Insights integration
- [ ] Structured logging
- [ ] Performance metrics
- [ ] Error tracking and alerting

### Caching
- [ ] Redis cache implementation
- [ ] Cache invalidation strategy
- [ ] Query result caching
- [ ] User session caching

### Performance
- [ ] Query optimization
- [ ] Async operations review
- [ ] Batch operations support
- [ ] Response compression

### DevOps
- [ ] CI/CD pipeline enhancement
- [ ] Infrastructure as Code
- [ ] Environment configuration
- [ ] Automated testing

## Configuration

The API uses a configuration-first approach with settings for:
- Database connectivity
- Pagination limits
- CORS policies
- Authentication settings
- Environment-specific configurations
