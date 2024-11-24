FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookSpring.WebApi/BookSpring.WebApi.csproj", "BookSpring.WebApi/"]
COPY ["BookSpring.DataLib/BookSpring.DataLib.csproj", "BookSpring.DataLib/"]
RUN dotnet restore "BookSpring.WebApi/BookSpring.WebApi.csproj"
COPY . .
WORKDIR "/src/BookSpring.WebApi"
RUN dotnet build "BookSpring.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BookSpring.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookSpring.WebApi.dll"]