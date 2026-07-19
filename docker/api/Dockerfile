FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY PayFlow.sln ./
COPY src/PayFlow.Domain/PayFlow.Domain.csproj src/PayFlow.Domain/
COPY src/PayFlow.Shared/PayFlow.Shared.csproj src/PayFlow.Shared/
COPY src/PayFlow.Application/PayFlow.Application.csproj src/PayFlow.Application/
COPY src/PayFlow.Infrastructure/PayFlow.Infrastructure.csproj src/PayFlow.Infrastructure/
COPY src/PayFlow.Api/PayFlow.Api.csproj src/PayFlow.Api/

RUN dotnet restore src/PayFlow.Api/PayFlow.Api.csproj

COPY src/ ./src/

RUN dotnet publish src/PayFlow.Api/PayFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayFlow.Api.dll"]
