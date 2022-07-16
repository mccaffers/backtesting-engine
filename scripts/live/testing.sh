set -o allexport
source ../../../../.env/local.env
set +o allexport

dotnet build;
dotnet lambda-test-tool-6.0