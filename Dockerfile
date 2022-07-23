FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Election/Election.Api.csproj", "Election.Api/"]
RUN dotnet restore "Election.Api/Election.Api.csproj"
COPY . .
WORKDIR "/src/Election"
RUN dotnet build "Election.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Election.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Election.Api.dll"]
