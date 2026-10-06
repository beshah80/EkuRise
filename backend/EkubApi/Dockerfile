FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["backend/EkubApi/EkubApi.csproj", "backend/EkubApi/"]
RUN dotnet restore "backend/EkubApi/EkubApi.csproj"
COPY backend/EkubApi/ backend/EkubApi/
WORKDIR "/src/backend/EkubApi"
RUN dotnet publish "EkubApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EkubApi.dll"]
