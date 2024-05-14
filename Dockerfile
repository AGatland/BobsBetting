# Use the official Microsoft .NET 8 SDK image for building the project.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BobsBetting/BobsBetting.csproj", "./"]
RUN dotnet restore "BobsBetting.csproj"

# Copy the rest of your project files and build the project.
COPY . .
RUN dotnet build "BobsBetting.csproj" -c Release -o /app/build

# Publish the project using the release configuration.
FROM build AS publish
RUN dotnet publish "BobsBetting.csproj" -c Release -o /app/publish

# Final stage/image based on the runtime base image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BobsBetting.dll"]