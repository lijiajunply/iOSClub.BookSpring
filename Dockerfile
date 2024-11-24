FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookSpring.WebApp/BookSpring.WebApp.csproj", "BookSpring.WebApp/"]
RUN dotnet restore "BookSpring.WebApp/BookSpring.WebApp.csproj"
COPY . .
WORKDIR "/src/BookSpring.WebApp"
RUN dotnet build "BookSpring.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BookSpring.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookSpring.WebApp.dll"]
