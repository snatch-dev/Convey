#!/bin/bash
echo Executing after success scripts on branch $GITHUB_REF_NAME
echo Triggering Nuget package build

cd src/Convey.Auth.Distributed/src/Convey.Auth.Distributed
dotnet pack -c release /p:PackageVersion=1.1.$GITHUB_RUN_NUMBER --no-restore -o .

echo Uploading Convey.Auth.Distributed package to Nuget using branch $GITHUB_REF_NAME

case "$GITHUB_REF_NAME" in
  "master")
    dotnet nuget push *.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
    ;;
esac