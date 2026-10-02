# =============================================================================
# Stage 1: Build Stage
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project file first for dependency caching
COPY ["EcommerceWebApi.csproj", "./"]

# Restore dependencies (cached layer)
RUN dotnet restore "EcommerceWebApi.csproj"

# Copy remaining source code
COPY . .

# Build and publish in Release configuration
RUN dotnet publish "EcommerceWebApi.csproj" -c Release -o /app/publish --no-restore

# =============================================================================
# Stage 2: Runtime Stage
# =============================================================================
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup --no-create-home appuser

# Create Logs directory with proper permissions
RUN mkdir -p /app/Logs && chown -R appuser:appgroup /app/Logs

# Copy published artifacts from builder stage
COPY --from=builder --chown=appuser:appgroup /app/publish .

# Copy database.json data file
COPY --chown=appuser:appgroup database.json .

# Set .NET environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV KESTREL_PORT=8080
ENV REDIS_CONNECTION_STRING=localhost:6379
ENV REDIS_INSTANCE_NAME=EcommerceWebApi:
ENV SIGNALR_CORS_ORIGINS=http://localhost:3001

# Expose application port
EXPOSE 8080

# Switch to non-root user
USER appuser

# Entry point
ENTRYPOINT ["dotnet", "EcommerceWebApi.dll"]
