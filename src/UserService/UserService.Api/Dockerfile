# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UserService/UserService.Api/UserService.Api.csproj", "src/UserService/UserService.Api/"]
COPY ["src/BuildingBlocks/Shared.Web/Shared.Web.csproj", "src/BuildingBlocks/Shared.Web/"]
COPY ["src/UserService/UserService.Application/UserService.Application.csproj", "src/UserService/UserService.Application/"]
COPY ["src/UserService/UserService.Domain/UserService.Domain.csproj", "src/UserService/UserService.Domain/"]
COPY ["src/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "src/UserService/UserService.Infrastructure/"]
RUN dotnet restore "./src/UserService/UserService.Api/UserService.Api.csproj"
COPY . .
WORKDIR "/src/src/UserService/UserService.Api"
RUN dotnet build "./UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]