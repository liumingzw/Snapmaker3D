set CuraEnginePath=%1
set configPath=%2
set gcodePath=%3
set stlPath=%4

%CuraEnginePath% slice -v -p -j %configPath% -o %gcodePath% -l %stlPath% 2>&1 