# Use .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files first
COPY *.sln ./
COPY backend.Core/*.csproj backend.Core/
COPY backend.Infrastructure/*.csproj backend.Infrastructure/
COPY backend.Api/*.csproj backend.Api/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish all projects into a single "published" folder
RUN dotnet publish -c Release -o /app/published

# Use lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/published .

EXPOSE 80
EXPOSE 443

# Run the application
ENTRYPOINT ["dotnet", "backend.Api.dll"]
