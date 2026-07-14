# syntax=docker/dockerfile:1

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first so `dotnet restore` layer is cached until deps change.
COPY src/BuddyScript.Domain/BuddyScript.Domain.csproj src/BuddyScript.Domain/
COPY src/BuddyScript.Application/BuddyScript.Application.csproj src/BuddyScript.Application/
COPY src/BuddyScript.Infrastructure/BuddyScript.Infrastructure.csproj src/BuddyScript.Infrastructure/
COPY src/BuddyScript.Api/BuddyScript.Api.csproj src/BuddyScript.Api/
RUN dotnet restore src/BuddyScript.Api/BuddyScript.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/BuddyScript.Api/BuddyScript.Api.csproj -c Release -o /app --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render terminates TLS at the edge and forwards HTTP; bind plain HTTP on 8080.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Non-root user provided by the .NET aspnet image.
USER $APP_UID

ENTRYPOINT ["dotnet", "BuddyScript.Api.dll"]
