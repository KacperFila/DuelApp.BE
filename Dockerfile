# -------- Base runtime image --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Set working directory inside container
WORKDIR /src

# Copy csproj files only (for cache)
COPY ["src/Bootstrapper/DuelApp.Bootstrapper/DuelApp.Bootstrapper.csproj", "Bootstrapper/DuelApp.Bootstrapper/"]
COPY ["src/Modules/Users/DuelApp.Modules.Users.Api/DuelApp.Modules.Users.Api.csproj", "Modules/Users/DuelApp.Modules.Users.Api/"]
COPY ["src/Modules/Users/DuelApp.Modules.Users.Core/DuelApp.Modules.Users.Core.csproj", "Modules/Users/DuelApp.Modules.Users.Core/"]
COPY ["src/Modules/Questions/DuelApp.Modules.Questions.Api/DuelApp.Modules.Questions.Api.csproj", "Modules/Questions/DuelApp.Modules.Questions.Api/"]
COPY ["src/Modules/Questions/DuelApp.Modules.Questions.Application/DuelApp.Modules.Questions.Application.csproj", "Modules/Questions/DuelApp.Modules.Questions.Application/"]
COPY ["src/Modules/Questions/DuelApp.Modules.Questions.Api/DuelApp.Modules.Questions.Domain.csproj", "Modules/Questions/DuelApp.Modules.Questions.Domain/"]
COPY ["src/Modules/Questions/DuelApp.Modules.Questions.Api/DuelApp.Modules.Questions.Infrastructure.csproj", "Modules/Questions/DuelApp.Modules.Questions.Infrastructure/"]
COPY ["src/Modules/Questions/DuelApp.Modules.Questions.Api/DuelApp.Modules.Questions.Shared.csproj", "Modules/Questions/DuelApp.Modules.Questions.Shared/"]
COPY ["src/Shared/DuelApp.Shared.Abstractions/DuelApp.Shared.Abstractions.csproj", "Shared/DuelApp.Shared.Abstractions/"]
COPY ["src/Shared/DuelApp.Shared.Infrastructure/DuelApp.Shared.Infrastructure.csproj", "Shared/DuelApp.Shared.Infrastructure/"]

# Restore project dependencies
RUN dotnet restore "Bootstrapper/DuelApp.Bootstrapper/DuelApp.Bootstrapper.csproj"

# Copy all source files
COPY src/ .

# Build
WORKDIR "/src/Bootstrapper/DuelApp.Bootstrapper"
RUN dotnet build "DuelApp.Bootstrapper.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish artifacts
FROM build AS publish
RUN dotnet publish "DuelApp.Bootstrapper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------- Final image --------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DuelApp.Bootstrapper.dll"]
