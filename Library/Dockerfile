# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sadece .csproj dosyasını kopyala
COPY Library/Library.csproj ./Library/

# Restore
WORKDIR /src/Library
RUN dotnet restore Library.csproj

# Geri kalan kaynak dosyaları kopyala
COPY Library/. .

# Build
RUN dotnet build Library.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish Library.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Library.dll"]
