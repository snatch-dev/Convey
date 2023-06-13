#!/bin/bash
echo Executing after success scripts on branch $GITHUB_REF_NAME
echo Triggering Nuget package build

cd src/Convey.Tracing.Jaeger/src/Convey.Tracing.Jaeger
dotnet pack -c release /p:PackageVersion=1.1.$GITHUB_RUN_NUMBER --no-restore -o .

echo Uploading Convey.Tracing.Jaeger package to Nuget using branch $GITHUB_REF_NAME

case "$GITHUB_REF_NAME" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
esac