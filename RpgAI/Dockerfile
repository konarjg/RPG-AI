FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ARG APP_UID=1001
RUN useradd --create-home --uid $APP_UID appuser
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RpgAI/RpgAI.csproj", "RpgAI/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "RpgAI/RpgAI.csproj"
COPY . .
WORKDIR "/src/RpgAI"
RUN dotnet build "./RpgAI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RpgAI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .

RUN chown -R $APP_UID:$APP_UID .

USER $APP_UID

ENTRYPOINT ["dotnet", "RpgAI.dll"]