# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Publish the application to a folder in the container
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o out

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final

# Set the working directory in the final container
WORKDIR /app

# Copy the published files from the build container
COPY --from=build /app/out ./

# Expose port for the application to listen
EXPOSE 8080

# Set the entry point for the app
ENTRYPOINT ["dotnet", "UABackbone-Backend.dll"]

