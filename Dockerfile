# Use the official .NET SDK image to build the app (for .NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the project file and restore dependencies
COPY . ./ 
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Publish the application to a folder in the container
RUN dotnet publish -c Release -o /app/out
RUN echo "Contents of /app after publish:" && ls -R /app
RUN echo "Contents of /app/out after publish:" && ls -R /app/out


# Check the contents of the output directory (add debugging)
RUN ls -la /app/out || echo "Directory /app/out does not exist."

# Use the official .NET runtime image to run the app (for .NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Set the working directory in the final container
WORKDIR /app

# Copy the published files from the build container
COPY --from=build /app/out ./ 

# Expose port for the application to listen
EXPOSE 8080

# Set the entry point for the app
ENTRYPOINT ["dotnet", "UABackbone-Backend.dll"]

