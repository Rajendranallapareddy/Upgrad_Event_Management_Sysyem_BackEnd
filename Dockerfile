# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY EMS.DAL/*.csproj EMS.DAL/
COPY EMS.Web/*.csproj EMS.Web/

# Restore dependencies
RUN dotnet restore EMS.Web/EMS.Web.csproj

# Copy all source code
COPY . .

# Publish the application
RUN dotnet publish EMS.Web/EMS.Web.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "EMS.Web.dll"]