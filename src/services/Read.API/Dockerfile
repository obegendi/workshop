FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/Read.API/Read.API.csproj", "src/services/Read.API/"]
RUN dotnet restore "src/services/Read.API/Read.API.csproj"
COPY . .
WORKDIR "/src/src/services/Read.API"
RUN dotnet build "Read.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Read.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Read.API.dll", "--environment=Production"]