#!/bin/bash
set -e
set -o pipefail

# =============================================================================
# deploy-image.sh - Deploy ecommerce-MContMono to AWS EKS
# =============================================================================

APP_NAME="ecommerce-mcontmono"
NAMESPACE="ecommerce-mcontmono"
K8S_DIR="kubernetes"

echo "=============================================="
echo "  Deploy to AWS EKS - ecommerce-MContMono"
echo "=============================================="
echo ""

# ---- Prompt for AWS / EKS configuration ----
read -rp "Enter AWS Region (e.g. us-east-1): " AWS_REGION
if [ -z "$AWS_REGION" ]; then
  echo "ERROR: AWS Region is required."
  exit 1
fi

read -rp "Enter EKS Cluster Name: " CLUSTER_NAME
if [ -z "$CLUSTER_NAME" ]; then
  echo "ERROR: EKS Cluster Name is required."
  exit 1
fi

read -rp "Enter full Docker Image URI (e.g. 123456789.dkr.ecr.us-east-1.amazonaws.com/ecommerce-mcontmono:latest): " IMAGE_URI
if [ -z "$IMAGE_URI" ]; then
  echo "ERROR: Docker Image URI is required."
  exit 1
fi

echo ""
echo "---- Application Environment Variables ----"
echo "Press Enter to skip any variable (placeholder will remain in manifest)."
echo ""

read -rp "Enter REDIS_CONNECTION_STRING (e.g. my-redis.cache.amazonaws.com:6379): " REDIS_CONNECTION_STRING
read -rp "Enter REDIS_INSTANCE_NAME (default: EcommerceWebApi:): " REDIS_INSTANCE_NAME
read -rp "Enter SIGNALR_CORS_ORIGINS (comma-separated, e.g. https://myapp.example.com): " SIGNALR_CORS_ORIGINS
read -rp "Enter APPLICATION_SECRET (JWT signing secret): " APPLICATION_SECRET

# Set defaults if empty
[ -z "$REDIS_CONNECTION_STRING" ] && REDIS_CONNECTION_STRING="localhost:6379"
[ -z "$REDIS_INSTANCE_NAME" ] && REDIS_INSTANCE_NAME="EcommerceWebApi:"
[ -z "$SIGNALR_CORS_ORIGINS" ] && SIGNALR_CORS_ORIGINS="http://localhost:3001"
[ -z "$APPLICATION_SECRET" ] && APPLICATION_SECRET="CHANGE_ME_IN_PRODUCTION"

echo ""
echo "---- Configuring kubectl for EKS ----"
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"
if [ $? -ne 0 ]; then
  echo "ERROR: Failed to configure kubectl for EKS cluster."
  exit 1
fi

echo ""
echo "---- Verifying cluster connectivity ----"
kubectl cluster-info || { echo "ERROR: Cannot connect to EKS cluster."; exit 1; }

echo ""
echo "---- Updating Kubernetes manifests ----"

# Create working copies of manifests
cp "${K8S_DIR}/deployment.yaml" "${K8S_DIR}/deployment.yaml.bak"

# Replace placeholders using pipe delimiter
sed -i 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' "${K8S_DIR}/deployment.yaml"
sed -i 's|{{REDIS_CONNECTION_STRING}}|'"$REDIS_CONNECTION_STRING"'|g' "${K8S_DIR}/deployment.yaml"
sed -i 's|{{REDIS_INSTANCE_NAME}}|'"$REDIS_INSTANCE_NAME"'|g' "${K8S_DIR}/deployment.yaml"
sed -i 's|{{SIGNALR_CORS_ORIGINS}}|'"$SIGNALR_CORS_ORIGINS"'|g' "${K8S_DIR}/deployment.yaml"
sed -i 's|{{APPLICATION_SECRET}}|'"$APPLICATION_SECRET"'|g' "${K8S_DIR}/deployment.yaml"

echo "Manifests updated successfully."

echo ""
echo "---- Applying Kubernetes manifests ----"

echo "Applying namespace..."
kubectl apply -f "${K8S_DIR}/namespace.yaml"

echo "Applying deployment..."
kubectl apply -f "${K8S_DIR}/deployment.yaml"

echo "Applying service..."
kubectl apply -f "${K8S_DIR}/service.yaml"

echo "Applying ingress..."
kubectl apply -f "${K8S_DIR}/ingress.yaml"

echo ""
echo "---- Waiting for deployment rollout ----"
kubectl rollout status deployment/"${APP_NAME}" -n "${NAMESPACE}" --timeout=300s
if [ $? -ne 0 ]; then
  echo "ERROR: Deployment rollout failed. Rolling back..."
  kubectl rollout undo deployment/"${APP_NAME}" -n "${NAMESPACE}"
  # Restore original manifest
  mv "${K8S_DIR}/deployment.yaml.bak" "${K8S_DIR}/deployment.yaml"
  exit 1
fi

# Restore original manifest (with placeholders) after successful deploy
mv "${K8S_DIR}/deployment.yaml.bak" "${K8S_DIR}/deployment.yaml"

echo ""
echo "---- Verifying deployed resources ----"
kubectl get pods,svc,ingress -n "${NAMESPACE}"

echo ""
echo "---- Application Access URL ----"
INGRESS_HOST=$(kubectl get ingress "${APP_NAME}-ingress" -n "${NAMESPACE}" -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || echo "pending")
if [ "$INGRESS_HOST" = "pending" ] || [ -z "$INGRESS_HOST" ]; then
  echo "Ingress hostname is still provisioning. Run the following to check:"
  echo "  kubectl get ingress ${APP_NAME}-ingress -n ${NAMESPACE}"
else
  echo "Application URL: http://${INGRESS_HOST}"
fi

echo ""
echo "=============================================="
echo "  SUCCESS: Deployment complete!"
echo "  Namespace : ${NAMESPACE}"
echo "  Image     : ${IMAGE_URI}"
echo "=============================================="
echo ""
echo "Useful commands:"
echo "  kubectl get pods -n ${NAMESPACE}"
echo "  kubectl logs -f deployment/${APP_NAME} -n ${NAMESPACE}"
echo "  kubectl rollout undo deployment/${APP_NAME} -n ${NAMESPACE}  # rollback"
