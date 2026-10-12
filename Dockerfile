# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY Devices.Api/*.csproj ./Devices.Api/

RUN dotnet restore ./Devices.Api/*.csproj

COPY . .

WORKDIR /src/Devices.Api

RUN dotnet publish ./*.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/App_Data \
    && chown -R $APP_UID:$APP_UID /app/App_Data

USER $APP_UID

ENTRYPOINT ["dotnet", "Devices.Api.dll"]
