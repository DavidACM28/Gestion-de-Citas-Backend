# Etapa 1: compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Gestion.Citas.API/Gestion.Citas.API.csproj", "Gestion.Citas.API/"]
COPY ["Gestion.Citas.Business/Gestion.Citas.Business.csproj", "Gestion.Citas.Business/"]
COPY ["Gestion.Citas.Repositories/Gestion.Citas.Repositories.csproj", "Gestion.Citas.Repositories/"]
COPY ["Gestion.Citas.DataAccess/Gestion.Citas.DataAccess.csproj", "Gestion.Citas.DataAccess/"]
COPY ["Gestion.Citas.Common/Gestion.Citas.Common.csproj", "Gestion.Citas.Common/"]

RUN dotnet restore "Gestion.Citas.API/Gestion.Citas.API.csproj"

COPY . .

RUN dotnet publish "Gestion.Citas.API/Gestion.Citas.API.csproj" \
    -c Release \
    -o /app

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Gestion.Citas.API.dll"]