# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Debug 1: Build context içeriği
RUN ls -la /src

# Proje dosyasını kopyala (kök dizinden Library klasörü içindeki .csproj)
COPY ["Library/Library.csproj", "Library/"]

# Debug 2: Library.csproj'nin kopyalanıp kopyalanmadığını kontrol et
RUN ls -la /src/Library/

# Working directory'yi Library klasörüne değiştir
WORKDIR /src/Library
RUN dotnet restore "Library.csproj"

# Debug 3: Restore sonrası dosya kontrolü
RUN ls -la /src/Library/

# Tüm Library kaynaklarını kopyala
COPY Library/ .

# Debug 4: Tüm kaynaklar kopyalandıktan sonra içeriği gör
RUN ls -la /src/Library/

# Projeyi build et
RUN dotnet build "Library.csproj" -c Release -o /app/build

# Debug 5: Build sonrası çıktı klasörü kontrolü
RUN ls -la /app/build

# Publish aşaması
FROM build AS publish
RUN dotnet publish "Library.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Debug 6: Publish sonrası çıktı klasörü kontrolü
RUN ls -la /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Debug 7: Final image içeriği kontrolü
RUN ls -la /app

ENTRYPOINT ["dotnet", "Library.dll"]