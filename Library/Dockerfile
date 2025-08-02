# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["./Library.csproj", "./"]
RUN dotnet restore "./Library.csproj"

# Tüm kaynakları kopyala
COPY . .

# Projeyi build et
RUN dotnet build "./Library.csproj" -c Release -o /app/build

# Publish aşaması
FROM build AS publish
RUN dotnet publish "./Library.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "./Library.dll"]
