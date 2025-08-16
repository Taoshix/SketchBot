# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

WORKDIR /app
COPY . .
RUN dotnet restore SketchBot/SketchBot.csproj
RUN dotnet publish SketchBot/SketchBot.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0

# Install Java 21
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://download.oracle.com/java/21/latest/jdk-21_linux-x64_bin.deb && \
    apt-get install -y ./jdk-21_linux-x64_bin.deb && \
    rm jdk-21_linux-x64_bin.deb

WORKDIR /app

# Copy application files
COPY --from=build-env /out .
COPY SketchBot/LavaLink ./LavaLink

# Use a shell script to run both processes
COPY start.sh /app/start.sh
RUN chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]