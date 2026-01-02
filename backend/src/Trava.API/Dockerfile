FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["backend/src/Trava.API/Trava.API.csproj", "backend/src/Trava.API/"]
COPY ["backend/src/Trava.Application/Trava.Application.csproj", "backend/src/Trava.Application/"]
COPY ["backend/src/Trava.Domain/Trava.Domain.csproj", "backend/src/Trava.Domain/"]
COPY ["backend/src/Trava.Infrastructure/Trava.Infrastructure.csproj", "backend/src/Trava.Infrastructure/"]
COPY ["backend/src/Trava.Shared/Trava.Shared.csproj", "backend/src/Trava.Shared/"]

RUN dotnet restore "backend/src/Trava.API/Trava.API.csproj"

COPY . .

WORKDIR "/src/backend/src/Trava.API"
RUN dotnet build "Trava.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Trava.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Trava.API.dll"]