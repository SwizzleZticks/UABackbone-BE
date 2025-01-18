# Use the official .NET SDK image to build the app (for .NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./ 
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Publish the application to a folder in the container
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the app (for .NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set the working directory in the final container
WORKDIR /app

# Copy the published files from the build container
COPY --from=build /app/out . 

# Set the entry point for the app
ENTRYPOINT ["dotnet", "UABackbone-Backend.dll"]
