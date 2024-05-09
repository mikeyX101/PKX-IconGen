#!/bin/bash

output=$1;
project=$2;

find "$project"../PKX-IconGen.Python -depth -type d -name '__pycache__' -exec rm -r '{}' \;

rm -r "${output}Python"

mkdir -p "${output}Data/NameMaps";
mkdir -p "${output}Python/data";
mkdir -p "${output}Python/importer";
mkdir -p "${output}Python/patches";

cp "$project"../PKX-IconGen.Python/patches/*.py "${output}Python/patches";
cp "$project"../PKX-IconGen.Python/data/*.py "${output}Python/data";
cp "$project"../PKX-IconGen.Python/*.py "${output}Python";
cp "$project"../PKX-IconGen.Python/template.blend "${output}Python";
cp "$project"../PKX-IconGen.Python/importer/* "${output}Python/importer";

cp "$project"../PKX-IconGen.AvaloniaUI/Assets/gen-icon.png "${output}";
cp "$project"../PKX-IconGen.AvaloniaUI/Assets/gen-icon-bright.png "${output}";

cp "$project"../PKX-IconGen.Core/NameMaps/coloNameMap.json "${output}Data/NameMaps";
cp "$project"../PKX-IconGen.Core/NameMaps/xdNameMap.json "${output}Data/NameMaps";
