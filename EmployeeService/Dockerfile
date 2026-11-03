# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["EmployeeService/EmployeeService.csproj", "EmployeeService/"]
RUN dotnet restore "EmployeeService/EmployeeService.csproj"

# Copy all source files and build
COPY EmployeeService/ EmployeeService/
WORKDIR /src/EmployeeService
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

USER root
RUN mkdir -p /app/logger /app/wwwroot /app/wwwroot/uploads/profile_pics /app/wwwroot/payslips && chown -R $APP_UID:$APP_UID /app
USER $APP_UID

ENTRYPOINT ["dotnet", "EmployeeService.dll"]
