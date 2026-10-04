@echo off
setlocal enabledelayedexpansion

:: =============================================================================
:: deploy-image.bat – Deploy EcommerceWebApi to AWS EKS (Windows)
:: Usage: scripts\deploy-image.bat
:: Prerequisites: aws-cli, kubectl
:: =============================================================================

set "APP_NAME=ecommercewebapi"
set "NAMESPACE=ecommercewebapi"
set "K8S_DIR=kubernetes"

echo ==============================================
echo   EcommerceWebApi - Deploy to AWS EKS
echo ==============================================
echo.

:: ---------------------------------------------------------------------------
:: Collect deployment parameters
:: ---------------------------------------------------------------------------
set /p "AWS_REGION=Enter AWS Region [us-east-1]: "
if "!AWS_REGION!"=="" set "AWS_REGION=us-east-1"

set /p "CLUSTER_NAME=Enter EKS Cluster Name: "
if "!CLUSTER_NAME!"=="" (
    echo ERROR: EKS Cluster Name is required.
    exit /b 1
)

set /p "IMAGE_URI=Enter full Docker Image URI: "
if "!IMAGE_URI!"=="" (
    echo ERROR: Docker Image URI is required.
    exit /b 1
)

:: ---------------------------------------------------------------------------
:: Application-specific environment variable prompts
:: ---------------------------------------------------------------------------
echo.
echo --- Application Environment Variables ---
echo (Press Enter to keep placeholder values)
echo.

set /p "REDIS_CONNECTION_STRING=Enter REDIS_CONNECTION_STRING [localhost:6379]: "
if "!REDIS_CONNECTION_STRING!"=="" set "REDIS_CONNECTION_STRING=localhost:6379"

set /p "REDIS_INSTANCE_NAME=Enter REDIS_INSTANCE_NAME [EcommerceWebApi:]: "
if "!REDIS_INSTANCE_NAME!"=="" set "REDIS_INSTANCE_NAME=EcommerceWebApi:"

set /p "SIGNALR_CORS_ORIGIN=Enter SIGNALR_CORS_ORIGIN [http://localhost:3001]: "
if "!SIGNALR_CORS_ORIGIN!"=="" set "SIGNALR_CORS_ORIGIN=http://localhost:3001"

set /p "APP_SECRET=Enter ApplicationSettings__Secret (JWT secret): "
if "!APP_SECRET!"=="" set "APP_SECRET=change-me-in-production"

:: ---------------------------------------------------------------------------
:: Configure kubectl for EKS
:: ---------------------------------------------------------------------------
echo.
echo Configuring kubectl for EKS cluster '!CLUSTER_NAME!' in '!AWS_REGION!'...
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to configure kubectl.
    exit /b 1
)
echo kubectl configured.

echo.
echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to EKS cluster.
    exit /b 1
)

:: ---------------------------------------------------------------------------
:: Substitute placeholders using PowerShell
:: ---------------------------------------------------------------------------
echo.
echo Updating Kubernetes manifests with deployment values...

copy /Y "!K8S_DIR!\deployment.yaml" "!K8S_DIR!\deployment.yaml.bak" >nul

powershell -NoProfile -Command ^
    "(Get-Content '!K8S_DIR!\deployment.yaml') ^
    -replace '{{IMAGE_URI}}','!IMAGE_URI!' ^
    -replace '{{REDIS_CONNECTION_STRING}}','!REDIS_CONNECTION_STRING!' ^
    -replace '{{REDIS_INSTANCE_NAME}}','!REDIS_INSTANCE_NAME!' ^
    -replace '{{SIGNALR_CORS_ORIGIN}}','!SIGNALR_CORS_ORIGIN!' ^
    -replace '{{APP_SECRET}}','!APP_SECRET!' ^
    | Set-Content '!K8S_DIR!\deployment.yaml'"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to update deployment.yaml.
    copy /Y "!K8S_DIR!\deployment.yaml.bak" "!K8S_DIR!\deployment.yaml" >nul
    exit /b 1
)
echo Manifests updated.

:: ---------------------------------------------------------------------------
:: Apply Kubernetes manifests
:: ---------------------------------------------------------------------------
echo.
echo Applying Kubernetes manifests...

echo   [1/4] Applying namespace...
kubectl apply -f !K8S_DIR!\namespace.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply namespace. & goto :restore_and_exit )

echo   [2/4] Applying deployment...
kubectl apply -f !K8S_DIR!\deployment.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply deployment. & goto :restore_and_exit )

echo   [3/4] Applying service...
kubectl apply -f !K8S_DIR!\service.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply service. & goto :restore_and_exit )

echo   [4/4] Applying ingress...
kubectl apply -f !K8S_DIR!\ingress.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply ingress. & goto :restore_and_exit )

:: Restore original deployment.yaml
copy /Y "!K8S_DIR!\deployment.yaml.bak" "!K8S_DIR!\deployment.yaml" >nul
del "!K8S_DIR!\deployment.yaml.bak" >nul 2>&1

:: ---------------------------------------------------------------------------
:: Wait for rollout
:: ---------------------------------------------------------------------------
echo.
echo Waiting for deployment rollout...
kubectl rollout status deployment/!APP_NAME! -n !NAMESPACE! --timeout=300s
if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed.
    exit /b 1
)

:: ---------------------------------------------------------------------------
:: Verify resources
:: ---------------------------------------------------------------------------
echo.
echo Verifying deployed resources...
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo ==============================================
echo   Deployment Complete!
echo   Check ingress for the application URL.
echo   Health Check: /health
echo ==============================================
echo.
echo Rollback command (if needed):
echo   kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!

endlocal
goto :eof

:restore_and_exit
copy /Y "!K8S_DIR!\deployment.yaml.bak" "!K8S_DIR!\deployment.yaml" >nul
del "!K8S_DIR!\deployment.yaml.bak" >nul 2>&1
exit /b 1
