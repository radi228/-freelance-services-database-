# Skillo - Freelance Services Platform

A full-stack web application built with ASP.NET Core (.NET 8), Entity Framework Core, and modern JavaScript/HTML/CSS featuring a modern design with authentication.

## Features

- **Responsive Frontend** - Modern UI with Skillo branding and professional design
- **User Authentication** - Secure registration and login with password hashing
- **RESTful API** - ASP.NET Core API endpoints for authentication
- **Database** - SQL Server with Entity Framework Core ORM
- **Client-Side Validation** - JavaScript form validation with error handling
- **Local Storage** - User session management

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB)
- **Language**: C#

### Frontend
- **Markup**: HTML5
- **Styling**: CSS3
- **Scripting**: Vanilla JavaScript
- **Icons**: Inline SVG

## Project Structure

```
Skillo/
├── Models/
│   ├── User.cs                 # User entity
│   └── AuthModels.cs           # DTOs for registration/login
├── Data/
│   └── ApplicationDbContext.cs  # Entity Framework context
├── Controllers/
│   └── AuthController.cs        # API endpoints
├── wwwroot/
│   ├── index.html             # Main HTML file
│   ├── css/
│   │   └── styles.css         # All styles
│   └── js/
│       └── auth.js            # Authentication logic
├── Program.cs                  # Application startup
├── appsettings.json           # Configuration
└── Skillo.csproj              # Project file
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### Installation

1. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

2. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the Application**
   - Open your browser and navigate to `https://localhost:5001`

## API Endpoints

### Authentication

**POST** `/api/auth/register`
```json
{
  "email": "user@example.com",
  "username": "username",
  "password": "password123",
  "confirmPassword": "password123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**POST** `/api/auth/login`
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

## Frontend Features

### Home Page
- Navigation bar with search
- Hero section with call-to-action
- Category showcase grid
- Featured services with seller profiles
- Footer with links

### Authentication
- Modal-based login/register forms
- Client-side validation
- Real-time error messages
- Success notifications
- Session persistence

## Security Features

- Password hashing using SHA-256
- Email and username uniqueness constraints
- CORS policy configuration
- Input validation on both client and server

## Configuration

Update `appsettings.json` for database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkilloDb;Trusted_Connection=true;"
  }
}
```

## Development Notes

- User sessions are stored in browser's localStorage
- API responses include user data and status messages
- Form validation happens on both client and server
- Responsive design works on mobile, tablet, and desktop

## Future Enhancements

- JWT token-based authentication
- Service creation and browsing
- User profiles and ratings
- Payment integration
- Real-time messaging
- Search functionality
- Advanced filtering

## License

MIT

## Author

Created as a portfolio project demonstrating full-stack web development with .NET and vanilla JavaScript using the Skillo brand.
