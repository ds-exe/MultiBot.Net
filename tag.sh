#!/usr/bin/env bash
set -e

IN=$(cat tag.txt)
arrIN=(${IN//./ })

pre=${arrIN[0]}
post=${arrIN[1]}
number=$(echo ${arrIN[2]} | tr -dc 0-9)
((number+=1))

tag=$pre.$post.$number

echo $tag > tag.txt

git tag -s $tag -m "Release $tag"
git push --tags
