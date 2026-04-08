FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files
COPY EMS.DAL/*.csproj EMS.DAL/
COPY EMS.WEB/*.csproj EMS.WEB/

# Restore
RUN dotnet restore EMS.WEB/EMS.Web.csproj

# Copy all code
COPY . .

# Publish
RUN dotnet publish EMS.WEB/EMS.Web.csproj -c Release -o /publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EMS.Web.dll"]