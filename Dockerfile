# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

WORKDIR /app
COPY . . 
RUN dotnet restore SketchBot/SketchBot.csproj
RUN dotnet publish SketchBot/SketchBot.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Copy application files
COPY --from=build-env /out .

ENTRYPOINT ["dotnet", "SketchBot.dll"]