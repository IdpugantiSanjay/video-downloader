FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VideoDownloader.Cli/VideoDownloader.Cli.csproj", "VideoDownloader.Cli/"]
RUN dotnet restore "VideoDownloader.Cli/VideoDownloader.Cli.csproj"
COPY . .
WORKDIR "/src/VideoDownloader.Cli"
RUN dotnet build "VideoDownloader.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoDownloader.Cli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoDownloader.Cli.dll"]
