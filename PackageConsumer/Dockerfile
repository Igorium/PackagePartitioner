FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY PackageConsumer/PackageConsumer.csproj PackageConsumer/
RUN dotnet restore PackageConsumer/PackageConsumer.csproj
COPY . .
WORKDIR /src/PackageConsumer
RUN dotnet build PackageConsumer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PackageConsumer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PackageConsumer.dll"]
