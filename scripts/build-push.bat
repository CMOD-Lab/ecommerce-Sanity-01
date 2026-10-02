@echo off
setlocal enabledelayedexpansion

:: =============================================================================
:: build-push.bat - Build and Push Docker Image for ecommerce-MContMono
:: =============================================================================

set "PROJECT_NAME=ecommerce-mcontmono"
set "DOCKERFILE_PATH=Dockerfile"

echo ==============================================
echo   Docker Build ^& Push - ecommerce-MContMono
echo ==============================================
echo.

:: Sanitize image name using PowerShell
for /f "delims=" %%i in ('powershell -NoProfile -Command "$n = 'ecommerce-mcontmono'.ToLower() -replace '[^a-z0-9]','-'; $n = $n.Trim('-'); $n"') do set "IMAGE_NAME=%%i"

:: Prompt for image tag
set /p "IMAGE_TAG_INPUT=Enter image tag (press Enter for 'latest'): "
if "!IMAGE_TAG_INPUT!"=="" (
    set "IMAGE_TAG=latest"
) else (
    for /f "delims=" %%t in ('powershell -NoProfile -Command "$t = '!IMAGE_TAG_INPUT!'.ToLower() -replace '[^a-z0-9._-]','-'; $t = $t.Trim('-'); if ($t -eq '') { 'latest' } else { $t }"') do set "IMAGE_TAG=%%t"
)

echo.
echo Select container registry:
echo   1. AWS ECR
echo   2. Docker Hub
set /p "REGISTRY_CHOICE=Enter choice (1 or 2): "

echo.

if "!REGISTRY_CHOICE!"=="1" (
    :: ---- AWS ECR ----
    set /p "AWS_REGION=Enter AWS Region (e.g. us-east-1): "
    set /p "AWS_ACCOUNT_ID=Enter AWS Account ID: "

    set "REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com"
    set "ECR_REPO=!IMAGE_NAME!"
    set "FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!"

    echo.
    echo Logging in to AWS ECR...
    aws ecr get-login-password --region !AWS_REGION! | docker login --username AWS --password-stdin !REGISTRY_URL!
    if !ERRORLEVEL! neq 0 (
        echo ERROR: ECR login failed.
        exit /b 1
    )

    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository...
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )

) else if "!REGISTRY_CHOICE!"=="2" (
    :: ---- Docker Hub ----
    set /p "DOCKER_USERNAME=Enter Docker Hub username: "
    set /p "DOCKER_PASSWORD=Enter Docker Hub password/token: "

    set "FULL_IMAGE_NAME=!DOCKER_USERNAME!/!IMAGE_NAME!:!IMAGE_TAG!"

    echo.
    echo Logging in to Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    if !ERRORLEVEL! neq 0 (
        echo ERROR: Docker Hub login failed.
        exit /b 1
    )

) else (
    echo ERROR: Invalid registry choice. Please enter 1 or 2.
    exit /b 1
)

echo.
echo Building Docker image: !FULL_IMAGE_NAME!
echo ----------------------------------------------
docker build -f "!DOCKERFILE_PATH!" -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)

echo.
echo Pushing Docker image: !FULL_IMAGE_NAME!
echo ----------------------------------------------
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed.
    exit /b 1
)

echo.
echo ==============================================
echo   SUCCESS: Image pushed successfully!
echo   Image: !FULL_IMAGE_NAME!
echo ==============================================

endlocal
