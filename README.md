# Stock Data Aggregator

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
