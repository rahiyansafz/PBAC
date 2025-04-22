# Dockerfile for the AccessManagementAPI
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["AccessManagementAPI/AccessManagementAPI.csproj", "AccessManagementAPI/"]

# Restore dependencies
RUN dotnet restore "AccessManagementAPI/AccessManagementAPI.csproj"

# Copy the remaining source code
COPY . .

# Install EF CLI tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Build the application
WORKDIR "/src/AccessManagementAPI"
RUN dotnet build "AccessManagementAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccessManagementAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install utilities
RUN apt-get update && apt-get install -y netcat-openbsd bash

# Copy wait-for-it script
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Copy the published application
COPY --from=publish /app/publish .

# Copy EF CLI tools from the build image
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create a directory for logs
RUN mkdir -p /app/logs

# Set the entry point for the container
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["dotnet", "AccessManagementAPI.dll"]