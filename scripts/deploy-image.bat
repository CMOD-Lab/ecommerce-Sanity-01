@echo off
setlocal enabledelayedexpansion

:: =============================================================================
:: deploy-image.bat - Deploy ecommerce-MContMono to AWS EKS (Windows)
:: =============================================================================

set "APP_NAME=ecommerce-mcontmono"
set "NAMESPACE=ecommerce-mcontmono"
set "K8S_DIR=kubernetes"

echo ==============================================
echo   Deploy to AWS EKS - ecommerce-MContMono
echo ==============================================
echo.

:: ---- Prompt for AWS / EKS configuration ----
set /p "AWS_REGION=Enter AWS Region (e.g. us-east-1): "
if "!AWS_REGION!"=="" (
    echo ERROR: AWS Region is required.
    exit /b 1
)

set /p "CLUSTER_NAME=Enter EKS Cluster Name: "
if "!CLUSTER_NAME!"=="" (
    echo ERROR: EKS Cluster Name is required.
    exit /b 1
)

set /p "IMAGE_URI=Enter full Docker Image URI (e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:latest): "
if "!IMAGE_URI!"=="" (
    echo ERROR: Docker Image URI is required.
    exit /b 1
)

echo.
echo ---- Application Environment Variables ----
echo Press Enter to skip any variable.
echo.

set /p "REDIS_CONNECTION_STRING=Enter REDIS_CONNECTION_STRING (e.g. my-redis.cache.amazonaws.com:6379): "
set /p "REDIS_INSTANCE_NAME=Enter REDIS_INSTANCE_NAME (default: EcommerceWebApi:): "
set /p "SIGNALR_CORS_ORIGINS=Enter SIGNALR_CORS_ORIGINS (comma-separated): "
set /p "APPLICATION_SECRET=Enter APPLICATION_SECRET (JWT signing secret): "

if "!REDIS_CONNECTION_STRING!"=="" set "REDIS_CONNECTION_STRING=localhost:6379"
if "!REDIS_INSTANCE_NAME!"=="" set "REDIS_INSTANCE_NAME=EcommerceWebApi:"
if "!SIGNALR_CORS_ORIGINS!"=="" set "SIGNALR_CORS_ORIGINS=http://localhost:3001"
if "!APPLICATION_SECRET!"=="" set "APPLICATION_SECRET=CHANGE_ME_IN_PRODUCTION"

echo.
echo ---- Configuring kubectl for EKS ----
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to configure kubectl for EKS cluster.
    exit /b 1
)

echo.
echo ---- Verifying cluster connectivity ----
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to EKS cluster.
    exit /b 1
)

echo.
echo ---- Updating Kubernetes manifests ----

:: Create backup of deployment manifest
copy /Y "!K8S_DIR!\deployment.yaml" "!K8S_DIR!\deployment.yaml.bak" >nul

:: Replace placeholders using PowerShell
powershell -NoProfile -Command ^
  "(Get-Content '!K8S_DIR!\deployment.yaml') ^
   -replace '\{\{IMAGE_URI\}\}', '!IMAGE_URI!' ^
   -replace '\{\{REDIS_CONNECTION_STRING\}\}', '!REDIS_CONNECTION_STRING!' ^
   -replace '\{\{REDIS_INSTANCE_NAME\}\}', '!REDIS_INSTANCE_NAME!' ^
   -replace '\{\{SIGNALR_CORS_ORIGINS\}\}', '!SIGNALR_CORS_ORIGINS!' ^
   -replace '\{\{APPLICATION_SECRET\}\}', '!APPLICATION_SECRET!' ^
   | Set-Content '!K8S_DIR!\deployment.yaml'"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to update deployment manifest.
    exit /b 1
)

echo Manifests updated successfully.

echo.
echo ---- Applying Kubernetes manifests ----

echo Applying namespace...
kubectl apply -f "!K8S_DIR!\namespace.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply namespace. & exit /b 1 )

echo Applying deployment...
kubectl apply -f "!K8S_DIR!\deployment.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply deployment. & exit /b 1 )

echo Applying service...
kubectl apply -f "!K8S_DIR!\service.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply service. & exit /b 1 )

echo Applying ingress...
kubectl apply -f "!K8S_DIR!\ingress.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply ingress. & exit /b 1 )

echo.
echo ---- Waiting for deployment rollout ----
kubectl rollout status deployment/!APP_NAME! -n !NAMESPACE! --timeout=300s
if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed. Rolling back...
    kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!
    copy /Y "!K8S_DIR!\deployment.yaml.bak" "!K8S_DIR!\deployment.yaml" >nul
    exit /b 1
)

:: Restore original manifest with placeholders
copy /Y "!K8S_DIR!\deployment.yaml.bak" "!K8S_DIR!\deployment.yaml" >nul
del "!K8S_DIR!\deployment.yaml.bak" >nul 2>&1

echo.
echo ---- Verifying deployed resources ----
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo ==============================================
echo   SUCCESS: Deployment complete!
echo   Namespace : !NAMESPACE!
echo   Image     : !IMAGE_URI!
echo ==============================================
echo.
echo Useful commands:
echo   kubectl get pods -n !NAMESPACE!
echo   kubectl logs -f deployment/!APP_NAME! -n !NAMESPACE!
echo   kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!

endlocal
