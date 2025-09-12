# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# copy everything and restore
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Use official ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Render expects your app to bind to PORT env var
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "api-raspi-web.dll"]
