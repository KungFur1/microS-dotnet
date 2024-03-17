FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
# Create a directory in docker
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use of the same copy command 
# for every dockerfile in the project to take advantage of docker caching.

# Copying the solution file into the docker image
COPY microS-dotnet.sln microS-dotnet.sln
# Copying all .csproj files into the docker image
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj

# Restore package dependencies (This will look at all .csproj files and install the required packages)
RUN dotnet restore microS-dotnet.sln

# Copy the app folders over
COPY src/GatewayService src/GatewayService
WORKDIR /app/src/GatewayService
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out . 
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]