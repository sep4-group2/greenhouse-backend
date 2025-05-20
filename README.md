# 🌱 Greenhouse Backend System

This repository contains the backend services for the greenhouse monitoring and control system. The system consists of several components that work together to collect, process, and serve greenhouse sensor data.

## 📦 Project Components

- **🖥️ API Service**: REST API that serves greenhouse data to frontend applications
- **📊 Data Consumer**: Service that subscribes to MQTT topics, processes incoming sensor data, and stores it in the database
- **📡 MQTT Broker**: Mosquitto broker for message handling between IoT devices and backend services
- **🗄️ SQL Server Database**: Stores greenhouse data, sensor readings, and configuration

## 🚀 Running the Project

There are three main ways to run this project:

### Option 1: Full Docker Deployment 🐳

This option runs all components in Docker containers:

1. Make sure you have Docker and Docker Compose installed
2. Clone the repository
3. Run the following command from the repository root:

```sh
docker compose --profile full up
```

This will start:
- 🗄️ SQL Server database on port 1433
- 📡 Mosquitto MQTT broker on port 1883
- 🖥️ API service on port 5050 (http://localhost:5050)
- 📊 Data Consumer service

### Option 2: Full Docker Deployment with Simulator 🌿

This option runs all components including a simulator for frontend development:

1. Make sure you have Docker and Docker Compose installed
2. Clone the repository
3. Run the following command from the repository root:

```sh
docker compose --profile full-simulator up -d --build
```

This will start all services from Option 1 plus:
- 🔄 Greenhouse simulator that generates realistic sensor data
- All services run in detached mode with freshly built images

For frontend development, a default user is seeded in the database:
- 👤 Email: bob@smartgrow.nothing
- 🔑 Password: password

This user account is always available but is especially useful with the simulator option as it provides access to generated greenhouse data.

### Option 3: Infrastructure in Docker + Services in IDE 💻

This option runs the infrastructure (database and MQTT) in Docker while running the API and Data Consumer in your IDE for development:

1. Start the infrastructure services:

```sh
docker compose up
```

2. Open the solution in JetBrains Rider
3. Set the run configuration to "Development"
4. Start the API and Data Consumer projects separately:
   - For the API project, run it directly from Rider
   - For the Data Consumer, run it from Rider with the "Development" configuration

When running in development mode:
- 🖥️ API typically runs at: https://localhost:7001 and http://localhost:5000
- 📡 MQTT broker is available at: localhost:1883
- 🗄️ SQL Server is available at: localhost:1433

## ⚙️ Configuration

The project uses different configuration files for different environments:

- **appsettings.json**: Base configuration with production defaults
- **appsettings.Development.json**: Local development configuration
- **appsettings.LocalDocker.json**: Configuration for services running in Docker

Environment-specific settings like connection strings and MQTT broker addresses are configured in these files and can be overridden using environment variables.

## 🔧 Environment Variables

Key environment variables:

- **DOTNET_ENVIRONMENT**: Set to "Development", "LocalDocker", or "Production"
- **MQTT__Host**: MQTT broker hostname (defaults to "mqtt" in Docker, "localhost" in development)
- **MQTT__Port**: MQTT broker port (defaults to 1883)

## ☁️ Azure Integration

In production, the system integrates with:
- Azure Key Vault for secrets management
- Azure SQL Database for data storage
- Azure Container Apps for service hosting

The Azure integration is automatically enabled when the environment is "Production" or "Release".

## 📂 Project Structure

- **Api/**: ASP.NET Core Web API project
- **Data/**: Shared data access library with Entity Framework Core
- **DataConsumer/**: MQTT client service for processing sensor data
- **Tests/**: Unit and integration tests
- **mosquitto/**: Mosquitto configuration

## 🔄 Development Workflow

1. Make your code changes in Rider using Option 3 (Development Configuration)
2. Test locally using the Docker infrastructure
3. When ready, commit changes and push to trigger CI/CD pipeline
4. The pipeline will build, test, and deploy the services to Azure