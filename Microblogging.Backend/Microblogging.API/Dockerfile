# Use latest .NET 9 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy the full solution and restore
COPY . ./
RUN dotnet restore Microblogging.Backend.sln

# Build and publish
RUN dotnet publish Microblogging.API/Microblogging.API.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Microblogging.API.dll"]
