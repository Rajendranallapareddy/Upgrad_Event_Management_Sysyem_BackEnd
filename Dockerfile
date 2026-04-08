FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files
COPY EMS.DAL/*.csproj EMS.DAL/
COPY EMS.WEB/*.csproj EMS.WEB/

# Restore dependencies
RUN dotnet restore EMS.WEB/EMS.Web.csproj

# Copy all source code
COPY . .

# Publish the application
RUN dotnet publish EMS.WEB/EMS.Web.csproj -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .

# Install PostgreSQL dependencies (if needed)
RUN apt-get update && apt-get install -y libpq-dev

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EMS.Web.dll"]