FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Copy everything
COPY . .

# Restore and publish
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run the app
CMD ["dotnet", "out/EMS.Web.dll"]
