output=$1;
project=$2;

rmdir -P "${output}Python"

mkdir -p "${output}Python/data";
mkdir -p "${output}Python/importer";

cp $project../PKX-IconGen.Python/data/*.py "${output}Python/data";

cp $project../PKX-IconGen.Python/*.py "${output}Python";
cp $project../PKX-IconGen.Python/template.blend "${output}Python";
cp $project../PKX-IconGen.Python/importer/* "${output}Python/importer";