FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app


COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/out

# Use the .NET Runtime image to run the app
# This is the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the build output from the previous stage
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 5000

# Start the application
ENTRYPOINT ["dotnet", "Server.dll"]