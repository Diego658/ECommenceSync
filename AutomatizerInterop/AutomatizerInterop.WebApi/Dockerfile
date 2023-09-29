#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf
COPY ["AutomatizerInterop.WebApi/AutomatizerInterop.WebApi.csproj", "AutomatizerInterop.WebApi/"]
RUN dotnet restore "AutomatizerInterop.WebApi/AutomatizerInterop.WebApi.csproj"
COPY . .
WORKDIR "/src/AutomatizerInterop.WebApi"
RUN dotnet build "AutomatizerInterop.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutomatizerInterop.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutomatizerInterop.WebApi.dll"]