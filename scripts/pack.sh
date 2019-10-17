#!/bin/bash
echo Preparing NuGet packages...
file=nuget_packages.txt
rm -f $file
git diff --dirstat=files,0 HEAD~1 | sed 's/^[ 0-9.]\+% //g' >> $file

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
done <$file | sort | uniq

echo Finished publishing NuGet packages.