# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Debug 1: Build context içeriği
RUN ls -la /src

# Proje dosyasını kopyala ve restore et
COPY ["Library.csproj", "./"]

# Debug 2: Kopyalanan dosya kontrolü
RUN ls -la /src

RUN dotnet restore "Library.csproj"

# Debug 3: Restore sonrası dosyalar
RUN ls -la /src

# Tüm kaynakları kopyala
COPY . .

# Debug 4: Tüm kaynaklar kopyalandıktan sonra
RUN ls -la /src

# Projeyi build et
RUN dotnet build "Library.csproj" -c Release -o /app/build

# Debug 5: Build sonrası output klasörü kontrolü
RUN ls -la /app/build

# Publish aşaması
FROM build AS publish

RUN dotnet publish "Library.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Debug 6: Publish sonrası output klasörü kontrolü
RUN ls -la /app/publish

# Final image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Debug 7: Final image içeriği
RUN ls -la /app

ENTRYPOINT ["dotnet", "Library.dll"]
