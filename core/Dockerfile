# Use the official .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory inside the container
WORKDIR /app

# Copy the project files to the container
COPY . ./

# Restore project dependencies
RUN dotnet restore

# Build the project
RUN dotnet build --configuration Release

# Publish the application
RUN dotnet publish --configuration Release --output out

# Use the official .NET Runtime image as the base image for the final image
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Set the working directory inside the container
WORKDIR /app

# Copy the published output from the build image to the final image
COPY --from=build-env /app/out .

# Expose port 15000 to the outside world
EXPOSE 15000


# Set the entry point for the application
ENTRYPOINT ["dotnet", "core.dll"]
