FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SGR_API.csproj", "./"]
RUN dotnet restore "SGR_API.csproj"
COPY . .
RUN dotnet publish "SGR_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SGR_API.dll"]
