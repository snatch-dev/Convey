#!/bin/bash
echo Generating a diff between the previous commit...
file=scripts/diff
rm -f $file
git diff --dirstat=files,0 HEAD~1 | sed 's/^[ 0-9.]\+% //g' >> $file