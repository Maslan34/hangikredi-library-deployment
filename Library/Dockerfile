# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Önce sadece .csproj dosyasını kopyala (dependency restore için)
COPY ["Library/Library.csproj", "Library/"]

# Restore işlemini yap
RUN dotnet restore "Library/Library.csproj"

# Şimdi tüm kaynak kodları kopyala
COPY Library/ Library/

# Working directory'yi değiştir ve build yap
WORKDIR /src/Library
RUN dotnet build "Library.csproj" -c Release -o /app/build

# Publish aşaması
FROM build AS publish
RUN dotnet publish "Library.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Library.dll"]