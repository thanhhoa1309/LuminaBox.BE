FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
# gRPC only (HTTP/2 cleartext), reachable from the API container, not published to the host.
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LuminaBox.AiService/LuminaBox.AiService.csproj", "LuminaBox.AiService/"]
RUN dotnet restore "./LuminaBox.AiService/LuminaBox.AiService.csproj"
COPY ["Protos/", "Protos/"]
COPY ["LuminaBox.AiService/", "LuminaBox.AiService/"]
WORKDIR "/src/LuminaBox.AiService"
RUN dotnet publish "./LuminaBox.AiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LuminaBox.AiService.dll"]
