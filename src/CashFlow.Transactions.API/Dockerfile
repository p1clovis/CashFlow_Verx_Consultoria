FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/CashFlow.Shared/CashFlow.Shared.csproj               src/CashFlow.Shared/
COPY src/CashFlow.Transactions.Domain/CashFlow.Transactions.Domain.csproj   src/CashFlow.Transactions.Domain/
COPY src/CashFlow.Transactions.Application/CashFlow.Transactions.Application.csproj src/CashFlow.Transactions.Application/
COPY src/CashFlow.Transactions.Infrastructure/CashFlow.Transactions.Infrastructure.csproj src/CashFlow.Transactions.Infrastructure/
COPY src/CashFlow.Transactions.API/CashFlow.Transactions.API.csproj src/CashFlow.Transactions.API/

RUN dotnet restore src/CashFlow.Transactions.API/CashFlow.Transactions.API.csproj

COPY src/ src/
RUN dotnet publish src/CashFlow.Transactions.API/CashFlow.Transactions.API.csproj \
    -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
VOLUME ["/data"]
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CashFlow.Transactions.API.dll"]
