# Stock Data Aggregator

# Deploying the app
To deploy the app, you can use Docker Compose. Make sure you have Docker and Docker Compose installed, then run the following command in the root of the project:

```bash
cd deploy

cp api.env.example api.env
# !!! Update api.env with your environment variables !!!

docker-compose up --build
```

This will build the Docker images and start the containers for the API, frontend, and database services.

# Development Environment Setup
Follow instructions hosted in Proton pass.
```bash
dotnet ef database update --project src/StockDataAggregator.Persistence --startup-project src/StockDataAggregator.Api
```

# Running the Application
To run the application, use the following command:
Backend:
```bash
cd api
dotnet run --project src/StockDataAggregator.Api
```

Frontend:
```bash
cd frontend
npm install
npm run dev
```
