#!/bin/sh
CuraEnginePath="$1"
configPath="$2"
gcodePath="$3"
stlPath="$4"

exec "$CuraEnginePath" slice -v -p -j "$configPath" -o "$gcodePath" -l "$stlPath" 2>&1



