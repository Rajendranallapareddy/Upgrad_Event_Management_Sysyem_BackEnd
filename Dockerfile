FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY EMS.DAL/EMS.DAL.csproj EMS.DAL/
COPY EMS.WEB/EMS.WEB.csproj EMS.WEB/

# Restore dependencies
RUN dotnet restore EMS.WEB/EMS.WEB.csproj

# Copy all files
COPY . .

# Publish
RUN dotnet publish EMS.WEB/EMS.WEB.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EMS.WEB.dll"]
