FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App
EXPOSE 8080

COPY . .
RUN dotnet restore
RUN dotnet publish -o /App/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as runtime
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "BobsBetting.dll"]