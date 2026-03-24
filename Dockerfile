# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /app

# Copy solution and project files
COPY src/DgiiSaas.slnx ./src/
COPY src/DgiiSaas.Api/*.csproj ./src/DgiiSaas.Api/
COPY src/DgiiSaas.Application/*.csproj ./src/DgiiSaas.Application/
COPY src/DgiiSaas.Domain/*.csproj ./src/DgiiSaas.Domain/
COPY src/DgiiSaas.Infrastructure/*.csproj ./src/DgiiSaas.Infrastructure/
COPY src/DgiiSaas.Infrastructure.Hazelcast/*.csproj ./src/DgiiSaas.Infrastructure.Hazelcast/
COPY src/DgiiSaas.Shared/*.csproj ./src/DgiiSaas.Shared/
COPY src/DgiiSaas.Workers/*.csproj ./src/DgiiSaas.Workers/

# Restore dependencies
RUN dotnet restore src/DgiiSaas.Api/DgiiSaas.Api.csproj

# Copy everything else and build
COPY src/. ./src/
WORKDIR /app/src/DgiiSaas.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 5040
COPY --from=build /app/publish .

# Environment variables for the container
ENV ASPNETCORE_URLS=http://+:5040
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DgiiSaas.Api.dll"]
