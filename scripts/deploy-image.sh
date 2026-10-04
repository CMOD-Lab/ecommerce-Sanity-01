#!/bin/bash
set -e
set -o pipefail

# =============================================================================
# deploy-image.sh – Deploy EcommerceWebApi to AWS EKS
# Usage: ./scripts/deploy-image.sh
# Prerequisites: aws-cli, kubectl
# =============================================================================

APP_NAME="ecommercewebapi"
NAMESPACE="ecommercewebapi"
K8S_DIR="kubernetes"

echo "=============================================="
echo "  EcommerceWebApi – Deploy to AWS EKS"
echo "=============================================="
echo ""

# ---------------------------------------------------------------------------
# Collect deployment parameters
# ---------------------------------------------------------------------------
read -rp "Enter AWS Region [us-east-1]: " AWS_REGION
AWS_REGION="${AWS_REGION:-us-east-1}"

read -rp "Enter EKS Cluster Name: " CLUSTER_NAME
if [ -z "$CLUSTER_NAME" ]; then
    echo "ERROR: EKS Cluster Name is required." >&2
    exit 1
fi

read -rp "Enter full Docker Image URI (e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/ecommercewebapi:latest): " IMAGE_URI
if [ -z "$IMAGE_URI" ]; then
    echo "ERROR: Docker Image URI is required." >&2
    exit 1
fi

# ---------------------------------------------------------------------------
# Application-specific environment variable prompts
# ---------------------------------------------------------------------------
echo ""
echo "--- Application Environment Variables ---"
echo "(Press Enter to keep placeholder values; update them in deployment.yaml later)"
echo ""

read -rp "Enter REDIS_CONNECTION_STRING (e.g. my-elasticache.abc.cache.amazonaws.com:6379): " REDIS_CONNECTION_STRING
REDIS_CONNECTION_STRING="${REDIS_CONNECTION_STRING:-localhost:6379}"

read -rp "Enter REDIS_INSTANCE_NAME [EcommerceWebApi:]: " REDIS_INSTANCE_NAME
REDIS_INSTANCE_NAME="${REDIS_INSTANCE_NAME:-EcommerceWebApi:}"

read -rp "Enter SIGNALR_CORS_ORIGIN (e.g. http://frontend-service:3001): " SIGNALR_CORS_ORIGIN
SIGNALR_CORS_ORIGIN="${SIGNALR_CORS_ORIGIN:-http://localhost:3001}"

read -rsp "Enter ApplicationSettings__Secret (JWT secret): " APP_SECRET
echo ""
APP_SECRET="${APP_SECRET:-change-me-in-production}"

# ---------------------------------------------------------------------------
# Configure kubectl for EKS
# ---------------------------------------------------------------------------
echo ""
echo "Configuring kubectl for EKS cluster '$CLUSTER_NAME' in '$AWS_REGION'..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"
echo "kubectl configured."

echo ""
echo "Verifying cluster connectivity..."
kubectl cluster-info || { echo "ERROR: Cannot connect to EKS cluster." >&2; exit 1; }

# ---------------------------------------------------------------------------
# Substitute placeholders in Kubernetes manifests
# ---------------------------------------------------------------------------
echo ""
echo "Updating Kubernetes manifests with deployment values..."

# Work on copies to avoid modifying originals
cp "${K8S_DIR}/deployment.yaml" "${K8S_DIR}/deployment.yaml.bak"

sed -i "s|{{IMAGE_URI}}|${IMAGE_URI}|g"                                   "${K8S_DIR}/deployment.yaml"
sed -i "s|{{REDIS_CONNECTION_STRING}}|${REDIS_CONNECTION_STRING}|g"       "${K8S_DIR}/deployment.yaml"
sed -i "s|{{REDIS_INSTANCE_NAME}}|${REDIS_INSTANCE_NAME}|g"               "${K8S_DIR}/deployment.yaml"
sed -i "s|{{SIGNALR_CORS_ORIGIN}}|${SIGNALR_CORS_ORIGIN}|g"               "${K8S_DIR}/deployment.yaml"
sed -i "s|{{APP_SECRET}}|${APP_SECRET}|g"                                  "${K8S_DIR}/deployment.yaml"

echo "Manifests updated."

# ---------------------------------------------------------------------------
# Apply Kubernetes manifests
# ---------------------------------------------------------------------------
echo ""
echo "Applying Kubernetes manifests..."

echo "  [1/4] Applying namespace..."
kubectl apply -f "${K8S_DIR}/namespace.yaml"

echo "  [2/4] Applying deployment..."
kubectl apply -f "${K8S_DIR}/deployment.yaml"

echo "  [3/4] Applying service..."
kubectl apply -f "${K8S_DIR}/service.yaml"

echo "  [4/4] Applying ingress..."
kubectl apply -f "${K8S_DIR}/ingress.yaml"

# ---------------------------------------------------------------------------
# Restore original deployment.yaml (remove substituted values)
# ---------------------------------------------------------------------------
mv "${K8S_DIR}/deployment.yaml.bak" "${K8S_DIR}/deployment.yaml"

# ---------------------------------------------------------------------------
# Wait for rollout
# ---------------------------------------------------------------------------
echo ""
echo "Waiting for deployment rollout..."
kubectl rollout status deployment/"${APP_NAME}" -n "${NAMESPACE}" --timeout=300s

# ---------------------------------------------------------------------------
# Verify resources
# ---------------------------------------------------------------------------
echo ""
echo "Verifying deployed resources..."
kubectl get pods,svc,ingress -n "${NAMESPACE}"

# ---------------------------------------------------------------------------
# Display application URL
# ---------------------------------------------------------------------------
echo ""
echo "Fetching application URL from ingress..."
INGRESS_HOST=$(kubectl get ingress "${APP_NAME}-ingress" -n "${NAMESPACE}" \
    -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || echo "pending")

echo ""
echo "=============================================="
echo "  Deployment Complete!"
echo "  Application URL: http://${INGRESS_HOST}"
echo "  Health Check:    http://${INGRESS_HOST}/health"
echo "=============================================="
echo ""
echo "Rollback command (if needed):"
echo "  kubectl rollout undo deployment/${APP_NAME} -n ${NAMESPACE}"
