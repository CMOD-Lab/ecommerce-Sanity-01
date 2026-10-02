# Deployment Guide: ecommerce-MContMono on AWS EKS

## Overview

This guide covers the complete deployment process for the **EcommerceWebApi** (.NET 6 ASP.NET Core Web API) application to **AWS Elastic Kubernetes Service (EKS)**.

- **Application**: EcommerceWebApi (ecommerce-MContMono)
- **Framework**: .NET 6 / ASP.NET Core Web API
- **Runtime Image**: `mcr.microsoft.com/dotnet/runtime:6.0`
- **Application Port**: `8080` (Kestrel, configurable via `KESTREL_PORT`)
- **Health Endpoint**: `/health`
- **Target Platform**: AWS EKS

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Structure](#project-structure)
3. [Local Development with Docker Compose](#local-development-with-docker-compose)
4. [Build and Push Docker Image](#build-and-push-docker-image)
5. [AWS EKS Prerequisites](#aws-eks-prerequisites)
6. [EKS Cluster Setup](#eks-cluster-setup)
7. [Kubernetes Deployment](#kubernetes-deployment)
8. [Environment Variables Reference](#environment-variables-reference)
9. [Scaling and Management](#scaling-and-management)
10. [Troubleshooting](#troubleshooting)
11. [Security Considerations](#security-considerations)
12. [.NET-Specific Notes](#net-specific-notes)

---

## Prerequisites

### Local Development Tools
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) 20.10+
- [Docker Compose](https://docs.docker.com/compose/install/) v2+
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (for local development)

### AWS & Kubernetes Tools
- [AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)
- [kubectl](https://kubernetes.io/docs/tasks/tools/) v1.24+
- [eksctl](https://eksctl.io/introduction/#installation) (optional, for cluster creation)
- AWS IAM credentials with EKS permissions

### AWS IAM Permissions Required
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "eks:DescribeCluster",
        "eks:ListClusters",
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:CreateRepository",
        "ecr:DescribeRepositories",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload"
      ],
      "Resource": "*"
    }
  ]
}
```

---

## Project Structure

```
ecommerce-MContMono/
├── Dockerfile                    # Multi-stage Docker build
├── docker-compose.yml            # Local development compose file
├── .dockerignore                 # Docker build exclusions
├── EcommerceWebApi.csproj        # .NET 6 project file
├── Program.cs                    # Application entry point
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Development overrides
├── database.json                 # JSON flat-file data store
├── kubernetes/
│   ├── namespace.yaml            # Kubernetes namespace
│   ├── deployment.yaml           # Application deployment
│   ├── service.yaml              # ClusterIP service
│   └── ingress.yaml              # AWS ALB ingress
├── scripts/
│   ├── build-push.sh             # Linux/macOS build & push
│   ├── build-push.bat            # Windows build & push
│   ├── deploy-image.sh           # Linux/macOS EKS deploy
│   └── deploy-image.bat          # Windows EKS deploy
└── docs/
    └── DEPLOYMENT.md             # This file
```

---

## Local Development with Docker Compose

### 1. Configure Environment Variables

Create a `.env` file in the project root:

```env
REDIS_CONNECTION_STRING=localhost:6379
REDIS_INSTANCE_NAME=EcommerceWebApi:
SIGNALR_CORS_ORIGINS=http://localhost:3001
APPLICATION_SECRET=your-jwt-secret-here
```

### 2. Start the Application

```bash
docker-compose up --build
```

### 3. Verify the Application

```bash
# Health check
curl http://localhost:8080/health

# Swagger UI (Development mode only)
open http://localhost:8080/swagger
```

### 4. Stop the Application

```bash
docker-compose down
```

---

## Build and Push Docker Image

### Linux / macOS

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

### Windows

```cmd
scripts\build-push.bat
```

The script will prompt you to:
1. Enter an image tag (defaults to `latest`)
2. Select registry type (AWS ECR or Docker Hub)
3. Provide registry credentials

### Manual Build (AWS ECR)

```bash
# Authenticate with ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  123456789012.dkr.ecr.us-east-1.amazonaws.com

# Create ECR repository (if not exists)
aws ecr create-repository --repository-name ecommerce-mcontmono --region us-east-1

# Build and push
docker build -f Dockerfile -t 123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:latest .
docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:latest
```

---

## AWS EKS Prerequisites

### 1. Configure AWS CLI

```bash
aws configure
# Enter: AWS Access Key ID, Secret Access Key, Region, Output format
```

### 2. Verify AWS Identity

```bash
aws sts get-caller-identity
```

### 3. Install AWS Load Balancer Controller

The ingress uses the AWS Load Balancer Controller. Install it on your EKS cluster:

```bash
# Add Helm repo
helm repo add eks https://aws.github.io/eks-charts
helm repo update

# Install controller
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=<your-cluster-name> \
  --set serviceAccount.create=false \
  --set serviceAccount.name=aws-load-balancer-controller
```

---

## EKS Cluster Setup

### Create a New EKS Cluster (Optional)

```bash
eksctl create cluster \
  --name ecommerce-cluster \
  --region us-east-1 \
  --nodegroup-name standard-workers \
  --node-type t3.medium \
  --nodes 2 \
  --nodes-min 1 \
  --nodes-max 4 \
  --managed
```

### Configure kubectl

```bash
aws eks update-kubeconfig --region us-east-1 --name ecommerce-cluster
kubectl cluster-info
```

---

## Kubernetes Deployment

### Automated Deployment

#### Linux / macOS

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows

```cmd
scripts\deploy-image.bat
```

The script will prompt for:
- AWS Region
- EKS Cluster Name
- Docker Image URI
- Application environment variables (Redis, CORS, JWT secret)

### Manual Deployment

```bash
# 1. Apply namespace
kubectl apply -f kubernetes/namespace.yaml

# 2. Update deployment.yaml with your image URI
sed -i 's|{{IMAGE_URI}}|123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:latest|g' kubernetes/deployment.yaml
sed -i 's|{{REDIS_CONNECTION_STRING}}|my-redis.cache.amazonaws.com:6379|g' kubernetes/deployment.yaml
sed -i 's|{{REDIS_INSTANCE_NAME}}|EcommerceWebApi:|g' kubernetes/deployment.yaml
sed -i 's|{{SIGNALR_CORS_ORIGINS}}|https://myapp.example.com|g' kubernetes/deployment.yaml
sed -i 's|{{APPLICATION_SECRET}}|my-jwt-secret|g' kubernetes/deployment.yaml

# 3. Apply manifests
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# 4. Wait for rollout
kubectl rollout status deployment/ecommerce-mcontmono -n ecommerce-mcontmono

# 5. Verify
kubectl get pods,svc,ingress -n ecommerce-mcontmono
```

### Verify Deployment

```bash
# Check pod status
kubectl get pods -n ecommerce-mcontmono

# Check pod logs
kubectl logs -f deployment/ecommerce-mcontmono -n ecommerce-mcontmono

# Check ingress and get URL
kubectl get ingress ecommerce-mcontmono-ingress -n ecommerce-mcontmono

# Test health endpoint
curl http://<INGRESS_HOSTNAME>/health
```

---

## Environment Variables Reference

| Variable | Description | Default |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |
| `KESTREL_PORT` | Kestrel listening port | `8080` |
| `REDIS_CONNECTION_STRING` | Redis/ElastiCache connection string | `localhost:6379` |
| `REDIS_INSTANCE_NAME` | Redis cache key prefix | `EcommerceWebApi:` |
| `SIGNALR_CORS_ORIGINS` | Comma-separated allowed CORS origins for SignalR | `http://localhost:3001` |
| `ApplicationSettings__Secret` | JWT signing secret | *(required)* |

---

## Scaling and Management

### Horizontal Pod Autoscaler (HPA)

```bash
kubectl autoscale deployment ecommerce-mcontmono \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n ecommerce-mcontmono
```

### Manual Scaling

```bash
kubectl scale deployment ecommerce-mcontmono --replicas=4 -n ecommerce-mcontmono
```

### Rolling Update

```bash
# Update image
kubectl set image deployment/ecommerce-mcontmono \
  ecommerce-mcontmono=123456789012.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:v2.0 \
  -n ecommerce-mcontmono

# Monitor rollout
kubectl rollout status deployment/ecommerce-mcontmono -n ecommerce-mcontmono
```

### Rollback

```bash
kubectl rollout undo deployment/ecommerce-mcontmono -n ecommerce-mcontmono
kubectl rollout history deployment/ecommerce-mcontmono -n ecommerce-mcontmono
```

---

## Troubleshooting

### Pod Not Starting

```bash
# Describe pod for events
kubectl describe pod -l app=ecommerce-mcontmono -n ecommerce-mcontmono

# Check logs
kubectl logs -l app=ecommerce-mcontmono -n ecommerce-mcontmono --previous
```

### Health Check Failing

```bash
# Port-forward to test locally
kubectl port-forward deployment/ecommerce-mcontmono 8080:8080 -n ecommerce-mcontmono

# Test health endpoint
curl http://localhost:8080/health
```

### Redis Connection Issues

```bash
# Verify REDIS_CONNECTION_STRING env var
kubectl exec -it deployment/ecommerce-mcontmono -n ecommerce-mcontmono -- env | grep REDIS

# Check ElastiCache security group allows traffic from EKS node group
```

### Ingress Not Getting External IP

```bash
# Check AWS Load Balancer Controller logs
kubectl logs -n kube-system deployment/aws-load-balancer-controller

# Verify ingress annotations
kubectl describe ingress ecommerce-mcontmono-ingress -n ecommerce-mcontmono
```

### Image Pull Errors

```bash
# Verify ECR permissions on EKS node role
# Attach AmazonEC2ContainerRegistryReadOnly policy to the EKS node IAM role

# Check image pull secret if using private registry
kubectl get events -n ecommerce-mcontmono | grep -i pull
```

---

## Security Considerations

1. **Non-root container**: The application runs as user `appuser` (UID 1001) — never as root.
2. **JWT Secret**: Store `ApplicationSettings__Secret` in AWS Secrets Manager and inject via Kubernetes Secrets:
   ```bash
   kubectl create secret generic ecommerce-secrets \
     --from-literal=jwt-secret=your-secret \
     -n ecommerce-mcontmono
   ```
3. **Redis TLS**: Use TLS-enabled ElastiCache endpoints in production.
4. **CORS**: Restrict `SIGNALR_CORS_ORIGINS` to known frontend domains only.
5. **Network Policies**: Apply Kubernetes NetworkPolicies to restrict pod-to-pod traffic.
6. **HTTPS**: Configure HTTPS on the ALB ingress using ACM certificates:
   ```yaml
   annotations:
     alb.ingress.kubernetes.io/certificate-arn: arn:aws:acm:us-east-1:123456789012:certificate/xxx
     alb.ingress.kubernetes.io/listen-ports: '[{"HTTPS":443}]'
   ```

---

## .NET-Specific Notes

### Kestrel Port Configuration
The application uses `KESTREL_PORT` environment variable (default: `8080`) to configure the listening port dynamically. This is set in `Program.cs` and allows port configuration without rebuilding the image.

### Redis Distributed Cache
The application uses `StackExchange.Redis` with `Microsoft.Extensions.Caching.StackExchangeRedis` for distributed caching. In production on EKS, point `REDIS_CONNECTION_STRING` to your **Amazon ElastiCache for Redis** endpoint.

### SignalR
The application exposes a SignalR hub at `/notificationHub`. Ensure your ALB/ingress supports WebSocket connections:
```yaml
annotations:
  alb.ingress.kubernetes.io/target-group-attributes: stickiness.enabled=true,stickiness.lb_cookie.duration_seconds=86400
```

### JSON Flat-File Data Store
The application uses `JsonFlatFileDataStore` with `database.json`. In Kubernetes, this file is baked into the container image. For production, consider migrating to a proper database (PostgreSQL, SQL Server) with Entity Framework Core.

### Serilog Logging
Logs are written to `/app/Logs/` inside the container. The Kubernetes deployment mounts an `emptyDir` volume at this path. For persistent log storage, consider:
- AWS CloudWatch Logs via Fluent Bit DaemonSet
- Amazon OpenSearch Service

### Health Checks
The `/health` endpoint is registered via `app.MapHealthChecks("/health")` in `Program.cs`. Both liveness and readiness probes use this endpoint.

### .NET GC Memory Tuning
For containers with memory limits, consider setting:
```yaml
env:
  - name: DOTNET_GCHeapHardLimit
    value: "805306368"  # 768 MiB (75% of 1Gi limit)
  - name: DOTNET_GCConserveMemory
    value: "5"
```
