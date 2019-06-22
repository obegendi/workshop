FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["src/services/Product.API/Product.API.csproj", "src/services/Product.API/"]
RUN dotnet restore "src/services/Product.API/Product.API.csproj"
COPY . .
WORKDIR "/src/src/services/Product.API"
RUN dotnet build "Product.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Product.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Product.API.dll", "--environment=Production"]