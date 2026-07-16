FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Lofasi.API/Lofasi.API.csproj src/Lofasi.API/
COPY src/Lofasi.Application/Lofasi.Application.csproj src/Lofasi.Application/
COPY src/Lofasi.Domain/Lofasi.Domain.csproj src/Lofasi.Domain/
COPY src/Lofasi.Infrastructure/Lofasi.Infrastructure.csproj src/Lofasi.Infrastructure/

RUN dotnet restore src/Lofasi.API/Lofasi.API.csproj

COPY . .
RUN dotnet publish src/Lofasi.API/Lofasi.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__Database="Data Source=/data/lofasi.db"
ENV Database__ApplyMigrationsOnStartup=true

COPY --from=build /app/publish .

VOLUME ["/data"]
EXPOSE 8080
ENTRYPOINT ["dotnet", "Lofasi.API.dll"]
