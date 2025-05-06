# MovieList

MovieList is a web application for managing and reviewing movies.

## Features

- User authentication and role-based access control
- Admin panel for managing application
- Movie catalog with ratings and reviews
- Search and filter capabilities

## Technologies Used

- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- Bootstrap for responsive UI

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/KavomanyOrganization/MovieList.git
   ```

2. Navigate to the project directory:
   ```sh
   cd MovieList/MVC
   ```

3. Configure the database connection and admin password in `.env`:
   ```sh
   CONNECTION_STRING="your_connection_string"
   ADMIN_PASSWORD = "your_password"
   ```

4. Apply migrations:
   ```sh
   dotnet ef database update
   ```

5. Build the application:
   ```sh
   dotnet build
   ```

6. Seed the initial data:
   ```sh
   dotnet run seeddata
   ```

7. Run the application:
   ```sh
   dotnet watch run
   ```

## Project Structure

```
MovieList/
├── Controllers/     # MVC Controllers
├── Models/          # Data models
├── ViewModels/      # View models for UI
├── Views/           # UI templates
├── Services/        # Business logic
├── Data/            # Database context and migrations
└── wwwroot/         # Static files (CSS, JS, images)
```

## Configuration

The application uses environment variables for configuration. Create an `.env` file in the root directory with the following variables:

- `CONNECTION_STRING`: PostgreSQL connection string
- `ADMIN_PASSWORD`: Password for the admin account

## Development

### Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 14 or later

### Working with Database

To create a new migration:

```sh
dotnet ef migrations add MigrationName
```

To apply migrations:

```sh
dotnet ef database update
```
