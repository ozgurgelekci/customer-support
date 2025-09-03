# .NET 9 SDK kullan (build için)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Project dosyasını kopyala ve restore et
COPY CustomerSupport/CustomerSupport.csproj CustomerSupport/
RUN dotnet restore CustomerSupport/CustomerSupport.csproj

# Kaynak kodları kopyala
COPY CustomerSupport/ CustomerSupport/

# Uygulamayı build et
WORKDIR /src/CustomerSupport
RUN dotnet publish CustomerSupport.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Build aşamasından dosyaları kopyala
COPY --from=build /app/publish .

# Port'u expose et
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

# SSL sertifikası geliştirme için (opsiyonel)
RUN dotnet dev-certs https --trust 2>/dev/null || true

# Uygulama başlat
ENTRYPOINT ["dotnet", "CustomerSupport.dll"]
