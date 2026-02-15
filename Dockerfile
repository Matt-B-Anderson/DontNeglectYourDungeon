# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/Data

# Expose port (Render uses PORT environment variable)
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "DontNeglectYourDungeon.dll"]
