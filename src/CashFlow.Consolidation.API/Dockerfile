FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/CashFlow.Shared/CashFlow.Shared.csproj               src/CashFlow.Shared/
COPY src/CashFlow.Consolidation.Domain/CashFlow.Consolidation.Domain.csproj  src/CashFlow.Consolidation.Domain/
COPY src/CashFlow.Consolidation.Application/CashFlow.Consolidation.Application.csproj src/CashFlow.Consolidation.Application/
COPY src/CashFlow.Consolidation.Infrastructure/CashFlow.Consolidation.Infrastructure.csproj src/CashFlow.Consolidation.Infrastructure/
COPY src/CashFlow.Consolidation.API/CashFlow.Consolidation.API.csproj src/CashFlow.Consolidation.API/

RUN dotnet restore src/CashFlow.Consolidation.API/CashFlow.Consolidation.API.csproj

COPY src/ src/
RUN dotnet publish src/CashFlow.Consolidation.API/CashFlow.Consolidation.API.csproj \
    -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
VOLUME ["/data"]
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CashFlow.Consolidation.API.dll"]
