# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src

# Copy project file first for dependency caching
COPY EcommerceWebApi.csproj ./

# Restore NuGet packages (cached layer)
RUN dotnet restore EcommerceWebApi.csproj

# Copy remaining source code
COPY . .

# Build and publish in Release configuration
RUN dotnet publish EcommerceWebApi.csproj -c Release -o /app/publish --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime

# Set working directory
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup --shell /bin/false appuser

# Copy published output from build stage
COPY --from=builder /app/publish .

# Create Logs directory and set ownership
RUN mkdir -p /app/Logs \
    && chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose application port
EXPOSE 80

# .NET environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "EcommerceWebApi.dll"]
