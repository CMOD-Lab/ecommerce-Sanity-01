# EcommerceWebApi – Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Analysis](#project-analysis)
4. [Local Development with Docker Compose](#local-development-with-docker-compose)
5. [Building and Pushing the Docker Image](#building-and-pushing-the-docker-image)
6. [AWS EKS Deployment](#aws-eks-deployment)
7. [Kubernetes Manifest Reference](#kubernetes-manifest-reference)
8. [Configuration Management](#configuration-management)
9. [Scaling and Rolling Updates](#scaling-and-rolling-updates)
10. [Troubleshooting](#troubleshooting)
11. [Security Considerations](#security-considerations)
12. [.NET-Specific Notes](#net-specific-notes)

---

## Overview

**Application**: EcommerceWebApi  
**Technology**: ASP.NET Core Web API (.NET 6)  
**Target Platform**: AWS EKS (Elastic Kubernetes Service)  
**Container Registry**: AWS ECR or Docker Hub  
**Application Port**: 80 (HTTP)  
**Health Endpoint**: `/health`

This guide covers the complete lifecycle from local development to production deployment on AWS EKS.

---

## Prerequisites

### Local Development
| Tool | Version | Purpose |
|------|---------|---------|
| Docker Desktop | 24.x+ | Container build and local run |
| Docker Compose | 2.x+ | Multi-container local orchestration |
| .NET SDK | 6.0+ | Local development and testing |

### AWS EKS Deployment
| Tool | Version | Purpose |
|------|---------|---------|
| AWS CLI | 2.x+ | AWS authentication and ECR access |
| kubectl | 1.27+ | Kubernetes cluster management |
| eksctl | 0.150+ | EKS cluster provisioning (optional) |

### AWS IAM Permissions Required
```
ecr:GetAuthorizationToken
ecr:BatchCheckLayerAvailability
ecr:GetDownloadUrlForLayer
ecr:BatchGetImage
ecr:PutImage
ecr:InitiateLayerUpload
ecr:UploadLayerPart
ecr:CompleteLayerUpload
ecr:CreateRepository
ecr:DescribeRepositories
eks:DescribeCluster
eks:ListClusters
```

---

## Project Analysis

| Property | Value |
|----------|-------|
| Framework | .NET 6 (ASP.NET Core Web API) |
| Build Tool | dotnet CLI |
| Application Port | 80 |
| Health Endpoint | `/health` |
| Base Image (Build) | `mcr.microsoft.com/dotnet/sdk:6.0` |
| Base Image (Runtime) | `mcr.microsoft.com/dotnet/sdk:6.0` (explicit: `mcr.microsoft.com/dotnet/sdk:6.0`) |
| Key Dependencies | JWT Bearer Auth, SignalR, Redis (StackExchange), Serilog, Swagger |
| External Services | Redis (ElastiCache), SignalR CORS origin |

---

## Local Development with Docker Compose

### 1. Clone and Configure

```bash
git clone <repository-url>
cd ecommerceSanity01mono
```

### 2. Create a `.env` file (optional overrides)

```env
REDIS_CONNECTION_STRING=localhost:6379
REDIS_INSTANCE_NAME=EcommerceWebApi:
SIGNALR_CORS_ORIGIN=http://localhost:3001
APP_SECRET=your-super-secret-jwt-key-here
```

### 3. Start the Application

```bash
docker compose up --build
```

The API will be available at: `http://localhost:80`  
Swagger UI: `http://localhost:80/swagger`  
Health check: `http://localhost:80/health`

### 4. Stop the Application

```bash
docker compose down
```

### 5. View Logs

```bash
docker compose logs -f ecommercewebapi
```

---

## Building and Pushing the Docker Image

### Linux / macOS

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

### Windows

```cmd
scripts\build-push.bat
```

### Script Prompts

| Prompt | Description | Example |
|--------|-------------|---------|
| Image tag | Docker image tag | `v1.0.0` or `latest` |
| Registry choice | `1` = AWS ECR, `2` = Docker Hub | `1` |
| AWS Region | AWS region for ECR | `us-east-1` |
| AWS Account ID | 12-digit AWS account number | `123456789012` |
| ECR Repository | ECR repository name | `ecommercewebapi` |

The script will:
1. Sanitize the image name (lowercase, hyphenated)
2. Authenticate with the selected registry
3. Auto-create the ECR repository if it does not exist
4. Build the Docker image using the multi-stage Dockerfile
5. Push the image to the registry

### Manual Build and Push (AWS ECR)

```bash
# Authenticate
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  123456789012.dkr.ecr.us-east-1.amazonaws.com

# Build
docker build -f Dockerfile \
  -t 123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommercewebapi:latest .

# Push
docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommercewebapi:latest
```

---

## AWS EKS Deployment

### Step 1: Create or Connect to an EKS Cluster

```bash
# Create a new cluster (if needed)
eksctl create cluster \
  --name my-ecommerce-cluster \
  --region us-east-1 \
  --nodegroup-name standard-workers \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-min 1 \
  --nodes-max 4

# Or connect to an existing cluster
aws eks update-kubeconfig --region us-east-1 --name my-ecommerce-cluster
```

### Step 2: Install AWS Load Balancer Controller

The ingress manifest uses the AWS Load Balancer Controller. Install it on your cluster:

```bash
# Add the EKS chart repository
helm repo add eks https://aws.github.io/eks-charts
helm repo update

# Install the controller
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=my-ecommerce-cluster \
  --set serviceAccount.create=false \
  --set serviceAccount.name=aws-load-balancer-controller
```

### Step 3: Run the Deployment Script

#### Linux / macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows
```cmd
scripts\deploy-image.bat
```

### Step 4: Script Prompts

| Prompt | Description | Example |
|--------|-------------|---------|
| AWS Region | EKS cluster region | `us-east-1` |
| EKS Cluster Name | Name of your EKS cluster | `my-ecommerce-cluster` |
| Docker Image URI | Full image path with tag | `123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommercewebapi:latest` |
| REDIS_CONNECTION_STRING | ElastiCache endpoint | `my-cache.abc.cache.amazonaws.com:6379` |
| REDIS_INSTANCE_NAME | Redis key prefix | `EcommerceWebApi:` |
| SIGNALR_CORS_ORIGIN | Frontend origin for SignalR | `http://frontend-service:3001` |
| ApplicationSettings__Secret | JWT signing secret | `your-secret-key` |

### Step 5: Verify Deployment

```bash
# Check pods
kubectl get pods -n ecommercewebapi

# Check services
kubectl get svc -n ecommercewebapi

# Check ingress (wait for ALB provisioning ~2-3 minutes)
kubectl get ingress -n ecommercewebapi

# View pod logs
kubectl logs -f deployment/ecommercewebapi -n ecommercewebapi

# Test health endpoint
kubectl port-forward svc/ecommercewebapi-service 8080:80 -n ecommercewebapi
curl http://localhost:8080/health
```

### Step 6: Manual Manifest Application

If you prefer to apply manifests manually:

```bash
# 1. Create namespace
kubectl apply -f kubernetes/namespace.yaml

# 2. Update image URI in deployment.yaml
sed -i 's|{{IMAGE_URI}}|YOUR_IMAGE_URI|g' kubernetes/deployment.yaml

# 3. Update environment variables
sed -i 's|{{REDIS_CONNECTION_STRING}}|YOUR_REDIS_ENDPOINT|g' kubernetes/deployment.yaml
sed -i 's|{{REDIS_INSTANCE_NAME}}|EcommerceWebApi:|g' kubernetes/deployment.yaml
sed -i 's|{{SIGNALR_CORS_ORIGIN}}|http://your-frontend:3001|g' kubernetes/deployment.yaml
sed -i 's|{{APP_SECRET}}|your-jwt-secret|g' kubernetes/deployment.yaml

# 4. Apply all manifests
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# 5. Wait for rollout
kubectl rollout status deployment/ecommercewebapi -n ecommercewebapi
```

---

## Kubernetes Manifest Reference

### namespace.yaml
Creates the `ecommercewebapi` namespace to isolate all application resources.

### deployment.yaml
| Setting | Value |
|---------|-------|
| Replicas | 2 |
| CPU Request | 250m |
| CPU Limit | 500m |
| Memory Request | 512Mi |
| Memory Limit | 1Gi |
| Liveness Probe | `GET /health` (delay: 30s, period: 15s) |
| Readiness Probe | `GET /health` (delay: 15s, period: 10s) |
| Rolling Update | maxSurge: 1, maxUnavailable: 0 |

### service.yaml
Exposes the deployment internally via ClusterIP on port 80.

### ingress.yaml
Creates an AWS Application Load Balancer (ALB) via the AWS Load Balancer Controller.  
Update the `host` field (`ecommercewebapi.example.com`) to your actual domain.

---

## Configuration Management

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | .NET environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel binding URL | `http://+:80` |
| `REDIS_CONNECTION_STRING` | Redis/ElastiCache endpoint | `localhost:6379` |
| `REDIS_INSTANCE_NAME` | Redis key prefix | `EcommerceWebApi:` |
| `SIGNALR_CORS_ORIGIN` | Allowed SignalR CORS origin | `http://localhost:3001` |
| `ApplicationSettings__Secret` | JWT signing secret | *(required)* |
| `KESTREL_HTTP_PORT` | Override Kestrel HTTP port | `80` |

### Using Kubernetes Secrets (Recommended for Production)

```bash
# Create a secret for sensitive values
kubectl create secret generic ecommercewebapi-secrets \
  --from-literal=APP_SECRET="your-jwt-secret" \
  --from-literal=REDIS_CONNECTION_STRING="your-redis-endpoint:6379" \
  -n ecommercewebapi
```

Then reference in `deployment.yaml`:
```yaml
env:
  - name: ApplicationSettings__Secret
    valueFrom:
      secretKeyRef:
        name: ecommercewebapi-secrets
        key: APP_SECRET
```

---

## Scaling and Rolling Updates

### Manual Scaling

```bash
kubectl scale deployment ecommercewebapi --replicas=4 -n ecommercewebapi
```

### Horizontal Pod Autoscaler (HPA)

```bash
kubectl autoscale deployment ecommercewebapi \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n ecommercewebapi
```

### Rolling Update (New Image)

```bash
kubectl set image deployment/ecommercewebapi \
  ecommercewebapi=YOUR_NEW_IMAGE_URI \
  -n ecommercewebapi

kubectl rollout status deployment/ecommercewebapi -n ecommercewebapi
```

### Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/ecommercewebapi -n ecommercewebapi

# Rollback to specific revision
kubectl rollout history deployment/ecommercewebapi -n ecommercewebapi
kubectl rollout undo deployment/ecommercewebapi --to-revision=2 -n ecommercewebapi
```

---

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl describe pod -l app=ecommercewebapi -n ecommercewebapi

# Check pod logs
kubectl logs -l app=ecommercewebapi -n ecommercewebapi --previous
```

**Common causes:**
- `ImagePullBackOff`: ECR authentication issue or wrong image URI
- `CrashLoopBackOff`: Application startup failure (check logs for Redis connection errors)
- `OOMKilled`: Increase memory limits in `deployment.yaml`

### Redis Connection Failure

```bash
# Verify Redis connectivity from within the pod
kubectl exec -it deployment/ecommercewebapi -n ecommercewebapi -- \
  sh -c "nc -zv YOUR_REDIS_HOST 6379"
```

Ensure the EKS node security group allows outbound traffic to the ElastiCache security group on port 6379.

### Ingress / ALB Not Provisioning

```bash
# Check AWS Load Balancer Controller logs
kubectl logs -n kube-system deployment/aws-load-balancer-controller

# Check ingress events
kubectl describe ingress ecommercewebapi-ingress -n ecommercewebapi
```

**Common causes:**
- AWS Load Balancer Controller not installed
- Missing IAM permissions for the controller service account
- Subnet tags missing (`kubernetes.io/role/elb: 1`)

### Health Check Failing

```bash
# Test health endpoint directly
kubectl port-forward svc/ecommercewebapi-service 8080:80 -n ecommercewebapi
curl -v http://localhost:8080/health
```

Expected response:
```json
{"status":"Healthy","timestamp":"2024-01-01T00:00:00.0000000Z"}
```

### View Application Logs

```bash
# Stream logs from all pods
kubectl logs -f -l app=ecommercewebapi -n ecommercewebapi

# Logs are also written to /app/Logs inside the container
kubectl exec -it deployment/ecommercewebapi -n ecommercewebapi -- ls /app/Logs
```

---

## Security Considerations

1. **Non-root container**: The Dockerfile creates and uses a non-root user (`appuser`, UID 1001).
2. **JWT Secret**: Store `ApplicationSettings__Secret` in a Kubernetes Secret, not as a plain environment variable.
3. **Redis TLS**: Enable TLS for ElastiCache in production and update `REDIS_CONNECTION_STRING` accordingly (e.g., `rediss://...`).
4. **HTTPS**: Configure HTTPS termination at the ALB level using ACM certificates.
5. **Network Policies**: Restrict pod-to-pod communication using Kubernetes NetworkPolicy.
6. **Image Scanning**: Enable ECR image scanning to detect vulnerabilities.
7. **CORS**: Set `SIGNALR_CORS_ORIGIN` to the exact frontend origin; avoid wildcards in production.
8. **Secrets Rotation**: Rotate JWT secrets and Redis passwords regularly.

---

## .NET-Specific Notes

### Kestrel Port Configuration
The application reads the HTTP port from `KESTREL_HTTP_PORT` or `ASPNETCORE_HTTP_PORT` environment variables, defaulting to port 80. This allows dynamic port assignment in Kubernetes without rebuilding the image.

### Serilog File Logging
Logs are written to `/app/Logs/log-{date}.txt` inside the container. The Kubernetes deployment mounts an `emptyDir` volume at `/app/Logs`. For persistent log storage, replace `emptyDir` with a PersistentVolumeClaim or configure Serilog to write to CloudWatch Logs.

### Redis Distributed Cache
The application uses `StackExchange.Redis` for distributed caching. In production on EKS, point `REDIS_CONNECTION_STRING` to your Amazon ElastiCache for Redis endpoint.

### SignalR
The `NotificationHub` is mapped at `/notificationHub`. Ensure your ALB/ingress is configured to support WebSocket connections (the AWS ALB Controller handles this automatically).

### Swagger UI
Swagger is only enabled in the `Development` environment. It is disabled in `Production` by default. To enable it in production, modify `Program.cs` to remove the environment check.

### .NET GC Tuning
For memory-constrained containers, consider setting:
```yaml
env:
  - name: DOTNET_GCConserveMemory
    value: "9"
  - name: DOTNET_GCHeapHardLimit
    value: "805306368"  # 768 MiB
```

### Culture and Timezone
The container uses the default invariant culture. If locale-specific formatting is required, set:
```yaml
env:
  - name: DOTNET_SYSTEM_GLOBALIZATION_INVARIANT
    value: "false"
  - name: TZ
    value: "UTC"
```
