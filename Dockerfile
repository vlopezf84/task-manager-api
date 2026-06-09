# ─── Etapa 1: Compilación ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia los archivos de proyecto para restaurar dependencias
COPY ["TaskManagerAPI/TaskManagerAPI.csproj", "TaskManagerAPI/"]
COPY ["TaskManagerAPI.Domain/TaskManagerAPI.Domain.csproj", "TaskManagerAPI.Domain/"]
COPY ["TaskManagerAPI.Application/TaskManagerAPI.Application.csproj", "TaskManagerAPI.Application/"]
COPY ["TaskManagerAPI.Infrastructure/TaskManagerAPI.Infrastructure.csproj", "TaskManagerAPI.Infrastructure/"]

# Restaura las dependencias NuGet
RUN dotnet restore "TaskManagerAPI/TaskManagerAPI.csproj"

# Copia el resto del código fuente
COPY . .

# Compila y publica en modo Release
WORKDIR "/src/TaskManagerAPI"
RUN dotnet publish "TaskManagerAPI.csproj" -c Release -o /app/publish

# ─── Etapa 2: Runtime ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copia solo los binarios publicados de la etapa anterior
COPY --from=build /app/publish .

# Expone el puerto en el que corre la API
EXPOSE 8080

# Comando de arranque
ENTRYPOINT ["dotnet", "TaskManagerAPI.dll"]