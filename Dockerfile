FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["SqlDetective.Api/SqlDetective.Api.csproj", "SqlDetective.Api/"]
COPY ["SqlDetective.Domain/SqlDetective.Domain.csproj", "SqlDetective.Domain/"]
COPY ["SqlDetective.Data/SqlDetective.Data.csproj", "SqlDetective.Data/"]

RUN dotnet restore "SqlDetective.Api/SqlDetective.Api.csproj"


COPY . .
WORKDIR "/src/SqlDetective.Api"

RUN dotnet publish "SqlDetective.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SqlDetective.Api.dll"]
