FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["PackageReader/PackageReader.csproj", "PackageReader/"]
RUN dotnet restore "PackageReader/PackageReader.csproj"
COPY . .
WORKDIR "/src/PackageReader"
RUN dotnet build "PackageReader.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "PackageReader.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PackageReader.dll"]