FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8042

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-ltsc2022 AS build
WORKDIR /src
COPY ["Returns/Returns.csproj", "Returns/"]
RUN dotnet restore "Returns/Returns.csproj"
COPY . .
WORKDIR "/src/Returns"
RUN dotnet build "Returns.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "Returns.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8042
#ENV Variable for Where Documents are stored in the Host
ENV HOST_STORAGE_PATH="C:\\inetpub\\wwwroot\\RBSS\\Uploads"
ENTRYPOINT ["dotnet", "Returns.dll"]