# Real-time Chat Application

A modern real-time chat application built with Angular and .NET Core, featuring SignalR for real-time communication.

## Features

- 🔐 Authentication (Register/Login)
- 💬 Real-time messaging
- 👥 Group chat support
- 🤝 Private chat between users
- 🎨 Clean and responsive UI
- 🔄 Real-time updateso
- 🛡️ JWT authentication
- 🏗️ Clean Architecture

## Screenshots

### Authentication

#### Login Page
![alt text](image.png)
*Login interface with email and password authentication*

#### Register Page
![alt text](image-1.png)
*User registration form with required fields*

### Chat Features

#### Chat List
![alt text](image-2.png)
*Overview of all chats (private and group) with last message preview*

#### Create Group Chat
![alt text](image-3.png)
*Dialog for creating new group chats*

## Technical Architecture

### Frontend (Angular)

- **Components**
  - Chat components for message display and input
  - Authentication forms
  - Group chat creation dialog
  - Chat list for navigation

- **Services**
  - Authentication service
  - Chat service
  - SignalR service for real-time communication
  - User service

### Backend (.NET Core)

- **Clean Architecture**
  - Domain Layer: Core entities and business rules
  - Application Layer: Business logic and interfaces
  - Infrastructure Layer: External concerns and implementations
  - API Layer: REST endpoints and SignalR hub

- **Features**
  - CQRS pattern with MediatR
  - JWT authentication
  - Entity Framework Core
  - SignalR for real-time updates

## Getting Started

1. Clone the repository
```bash
git clone [repository-url]
```

2. Setup Backend
```bash
cd src/Real-time-Chat.WebApi
dotnet restore
# Update database with latest migrations
cd ../Real-time-Chat.Infrastructure
dotnet ef database update
cd ../Real-time-Chat.WebApi
dotnet run
```

3. Setup Frontend
```bash
cd frontend
npm install
ng serve
```

4. Navigate to `http://localhost:4200` in your browser

## Environment Configuration

### Frontend
Create `environment.ts` based on `environment.prod.ts.example`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5065/api',
  hubUrl: 'http://localhost:5065/api/chat'
};
```

### Backend
Update `appsettings.json` with your database connection string and JWT settings.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
