# Wireless Car Charging System

## Introduction

The Wireless Car Charging System is a project aimed at providing a seamless and efficient way to charge electric vehicles wirelessly. The system includes various functionalities such as authentication, car management, charging points, charging stations, dashboard, feedback, images, payment, and user management.

## Project Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Redis
- Cloudinary account for image storage
- SMTP server for email notifications

### Installation Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/namspidey/wireless-car-charging-system.git
   cd wireless-car-charging-system
   ```

2. Set up the database:
   - Update the connection string in `API/appsettings.json` to point to your SQL Server instance.
   - Run the migrations to create the database schema:
     ```bash
     dotnet ef database update --project DataAccess
     ```

3. Set up Redis:
   - Update the Redis connection string in `API/appsettings.json`.

4. Set up Cloudinary:
   - Add your Cloudinary URL to the environment variables.

5. Set up SMTP:
   - Add your SMTP server details to the environment variables.

6. Run the application:
   ```bash
   dotnet run --project API
   ```

### Create .env file

Create a `.env` file in the root directory or use the sample `.env.example` file.

### Configure Database Connection String

Open the `API/appsettings.json` file and update the connection string to point to your SQL Server instance.

## Running Tests

To run tests, use the following command in the `TestProject` directory:
```bash
dotnet test
```

## Project Structure and Key Components

The project is structured into three main directories:

1. `API`: Contains the controllers, services, and other API-related files.
   - Controllers: Handle HTTP requests and responses.
   - Services: Contain the business logic of the application.
   - Program.cs: Configures and starts the application.

2. `DataAccess`: Contains the models, DTOs, interfaces, and repositories for data access.
   - Models: Define the database schema.
   - DTOs: Data Transfer Objects used for communication between layers.
   - Interfaces: Define the contracts for the repositories.
   - Repositories: Implement the data access logic.

3. `TestProject`: Contains unit tests for various components of the project.
   - AuthTest: Tests for authentication-related functionalities.
   - ChargingStationTest: Tests for charging station-related functionalities.
   - DashboardTest: Tests for dashboard-related functionalities.
   - FeedbackTest: Tests for feedback-related functionalities.
   - MyCarTest: Tests for car-related functionalities.
   - ThirdParty: Tests for third-party services like image and OTP services.
   - UserTest: Tests for user-related functionalities.

## Contributing

We welcome contributions to the Wireless Car Charging System project. To contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Make your changes and commit them with a clear message.
4. Push your changes to your forked repository.
5. Create a pull request to the main repository.

Please ensure that your code follows the project's coding standards and includes appropriate tests.

Thank you for contributing!
