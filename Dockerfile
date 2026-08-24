# Use the official ASP.NET runtime image as the base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything into the container
COPY . .

# Restore dependencies and publish the app
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Final stage: build runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocumentationViewer.dll"]
