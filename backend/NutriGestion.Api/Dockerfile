# Dockerfile multi-stage para NutriGestión API (.NET 8)
# Imagen final pequeña: solo runtime + binarios publicados.

# ─── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de Docker en restore
COPY NutriGestion.Api.csproj ./
RUN dotnet restore

# Copiar el resto y publicar en modo Release
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ─── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Render/Railway inyectan PORT — Kestrel debe escuchar ahí.
# Si no viene PORT, usa 8080 por defecto.
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "NutriGestion.Api.dll"]
