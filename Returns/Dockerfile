FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8042

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-ltsc2022 AS build
# Set environment variables to speed up .NET build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_XMLDOC_MODE=skip \
    # Enable package caching
    NUGET_PACKAGES=/nuget/packages \
    # Disable sending diagnostic data
    DOTNET_NOLOGO=true

WORKDIR /src
# Copy only the project file first to leverage layer caching
COPY ["Returns/Returns.csproj", "Returns/"]
# Restore with minimal verbosity and parallel downloads
RUN dotnet restore "Returns/Returns.csproj" --verbosity minimal /p:RestoreUseSkipNonexistentTargets=true --disable-parallel=false

# Now copy everything else
COPY . .
WORKDIR /src/Returns
# Build with optimizations and parallel builds
RUN dotnet build "Returns.csproj" -c Release -o /app/build --verbosity minimal --no-restore /p:UseSharedCompilation=false /maxcpucount:8 /p:RuntimeIdentifier=win-x64

FROM build AS publish
# Publish without rebuilding since we already built
RUN dotnet publish "Returns.csproj" -c Release -o /app/publish --no-build --verbosity minimal /p:RuntimeIdentifier=win-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8042
# Set development environment
ENV ASPNETCORE_ENVIRONMENT=Development
#ENV Variable for Where Documents are stored in the Host
ENV HOST_STORAGE_PATH="C:\\inetpub\\wwwroot\\RBSS\\Uploads"
ENTRYPOINT ["dotnet", "Returns.dll"]