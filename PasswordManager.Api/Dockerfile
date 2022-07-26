#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PasswordManager.Api/PasswordManager.Api.csproj", "PasswordManager.Api/"]
COPY ["PasswordManager.Application/PasswordManager.Application.csproj", "PasswordManager.Application/"]
COPY ["PasswordManager.Domain/PasswordManager.Domain.csproj", "PasswordManager.Domain/"]
COPY ["PasswordManager.Infrastructure/PasswordManager.Infrastructure.csproj", "PasswordManager.Infrastructure/"]
RUN dotnet restore "PasswordManager.Api/PasswordManager.Api.csproj"
COPY . .
WORKDIR "/src/PasswordManager.Api"
RUN dotnet build "PasswordManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PasswordManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PasswordManager.Api.dll"]
