# Base image: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image: SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["Library.csproj", "./"]
RUN dotnet restore "./Library.csproj"

# Tüm kaynakları kopyala
COPY . .

# Projeyi build et
RUN dotnet build "./Library.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish aşaması
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Library.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Library.dll"]
