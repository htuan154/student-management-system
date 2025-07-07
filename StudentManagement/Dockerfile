# Base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StudentManagement/StudentManagement.csproj", "StudentManagement/"]
RUN dotnet restore "StudentManagement/StudentManagement.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/StudentManagement"
RUN dotnet build "StudentManagement.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "StudentManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentManagement.dll"]
