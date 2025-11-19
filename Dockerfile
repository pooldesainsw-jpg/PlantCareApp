FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first to optimize restore caching
COPY PlantCareApp/PlantCareApp.csproj ./PlantCareApp/
RUN dotnet restore PlantCareApp/PlantCareApp.csproj

# Copy the rest of the source
COPY . .

# Publish in Release mode
RUN dotnet publish PlantCareApp/PlantCareApp.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Container listens on port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PlantCareApp.dll"]
