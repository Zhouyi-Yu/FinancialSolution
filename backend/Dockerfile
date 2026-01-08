# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and project files
COPY FinancialSolution.sln ./
COPY FinanceApi/FinanceApi.csproj ./FinanceApi/
COPY FinanceApi.Tests/FinanceApi.Tests.csproj ./FinanceApi.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish FinanceApi/FinanceApi.csproj -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port and start the app
EXPOSE 80
ENTRYPOINT ["dotnet", "FinanceApi.dll"]
