# build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish ./src/StockDataAggregator.Api/StockDataAggregator.Api.csproj -c Release -o /out

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out ./
ENV ASPNETCORE_URLS=http://0.0.0.0:5077
EXPOSE 5077
ENTRYPOINT ["dotnet", "StockDataAggregator.Api.dll"]
