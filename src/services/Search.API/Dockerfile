FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/Search.API/Search.API.csproj", "src/services/Search.API/"]
RUN dotnet restore "src/services/Search.API/Search.API.csproj"
COPY . .
WORKDIR "/src/src/services/Search.API"
RUN dotnet build "Search.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Search.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Search.API.dll", "--environment=Production"]