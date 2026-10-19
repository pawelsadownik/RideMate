# RideMate API - Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RideMate.sln", "./"]
COPY ["src/RideMate.Domain/RideMate.Domain.csproj", "src/RideMate.Domain/"]
COPY ["src/RideMate.Application/RideMate.Application.csproj", "src/RideMate.Application/"]
COPY ["src/RideMate.Infrastructure/RideMate.Infrastructure.csproj", "src/RideMate.Infrastructure/"]
COPY ["src/RideMate.Api/RideMate.Api.csproj", "src/RideMate.Api/"]
RUN dotnet restore "src/RideMate.Api/RideMate.Api.csproj"
COPY . .
WORKDIR "/src/src/RideMate.Api"
RUN dotnet build "RideMate.Api.csproj" -c Release -o /app/build -m:1 /p:UseSharedCompilation=false

FROM build AS publish
RUN dotnet publish "RideMate.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false -m:1 /p:UseSharedCompilation=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RideMate.Api.dll"]
