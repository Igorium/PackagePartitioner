FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY PackagePartitioner/PackagePartitioner.csproj PackagePartitioner/
RUN dotnet restore PackagePartitioner/PackagePartitioner.csproj
COPY . .
WORKDIR /src/PackagePartitioner
RUN dotnet build PackagePartitioner.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PackagePartitioner.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PackagePartitioner.dll"]
