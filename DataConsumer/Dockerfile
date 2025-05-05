FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DataConsumer/DataConsumer.csproj", "DataConsumer/"]
COPY ["Data/Data.csproj", "Data/"]
RUN dotnet restore "DataConsumer/DataConsumer.csproj"
COPY . .
WORKDIR "/src/DataConsumer"
RUN dotnet build "./DataConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DataConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataConsumer.dll"]
