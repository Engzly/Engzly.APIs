FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Engzly.Notification/Engzly.Notification.csproj", "src/Engzly.Notification/"]
RUN dotnet restore "src/Engzly.Notification/Engzly.Notification.csproj"
COPY . .
WORKDIR "/src/src/Engzly.Notification"
RUN dotnet build "./Engzly.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Engzly.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Engzly.Notification.dll"]
