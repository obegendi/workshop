FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/Write.API/Write.API.csproj", "src/services/Write.API/"]
RUN dotnet restore "src/services/Write.API/Write.API.csproj"
COPY . .
WORKDIR "/src/src/services/Write.API"
RUN dotnet build "Write.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Write.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Write.API.dll", "--environment=Production"]