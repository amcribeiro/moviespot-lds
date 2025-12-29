# syntax=docker/dockerfile:1

# --- Build (SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["MovieSpot/MovieSpot.csproj", "MovieSpot/"]
RUN dotnet restore "MovieSpot/MovieSpot.csproj"
COPY . .
WORKDIR /src/MovieSpot
RUN dotnet publish "MovieSpot.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime (ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Kestrel a escutar em 8080 dentro do container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MovieSpot.dll"]
