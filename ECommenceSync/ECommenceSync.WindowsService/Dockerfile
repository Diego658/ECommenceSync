#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["ECommenceSync.WindowsService/ECommenceSync.WindowsService.csproj", "ECommenceSync.WindowsService/"]
RUN dotnet restore "ECommenceSync.WindowsService/ECommenceSync.WindowsService.csproj"
COPY . .
WORKDIR "/src/ECommenceSync.WindowsService"
RUN dotnet build "ECommenceSync.WindowsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ECommenceSync.WindowsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommenceSync.WindowsService.dll"]