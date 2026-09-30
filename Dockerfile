# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the entire directory structure
COPY . .

# Restore dependencies using the solution file
RUN dotnet restore TicketSystemAPI.sln

# Publish the specific project
RUN dotnet publish ./TicketSystemAPI/TicketSystemAPI.csproj -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "TicketSystemAPI.dll"]
