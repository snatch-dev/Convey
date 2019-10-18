#!/bin/bash
for dir in src/*/
do
    dir=${dir%*/}
    echo Publishing NuGet package:  ${dir##*/}
    exec ./$dir/scripts/dotnet-pack.sh &
    wait
done

echo Finished publishing NuGet packages.