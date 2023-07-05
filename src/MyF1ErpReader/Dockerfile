FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/MyF1ErpReader/MyF1ErpReader.csproj", "src/MyF1ErpReader/"]
RUN dotnet restore "src/MyF1ErpReader/MyF1ErpReader.csproj"
COPY . .
WORKDIR "/src/src/MyF1ErpReader"
RUN dotnet build "MyF1ErpReader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyF1ErpReader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyF1ErpReader.dll"]
