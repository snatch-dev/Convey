#!/bin/bash
echo Diff between the previous commit:
cat scripts/diff
echo Preparing NuGet packages...

while read f; do
  if [[ $f == src/Convey.*/src/Convey.* ]];
  then
    dir="$f../../scripts"
    if [ -d $dir ]; then
      echo Publishing NuGet package: $f
      exec ./$dir/dotnet-pack.sh &
      wait
    fi
  fi
done <scripts/diff | sort | uniq

echo Finished publishing NuGet packages.