FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["REA.API/REA.API.csproj", "REA.API/"]
COPY ["REA.Models/REA.Models.csproj", "REA.Models/"]
RUN dotnet restore "REA.API/REA.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/REA.API"
RUN dotnet build "REA.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "REA.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
ENTRYPOINT ["dotnet", "REA.API.dll"]