output=$1;
project=$2;

echo $output;
echo $project;

mkdir $output;
mkdir "${output}Python";
mkdir "${output}Python/addon";
mkdir "${output}Python/core";
mkdir "${output}Python/core/data";

cp $project../PKX-IconGen.Python.Addon/*.py "${output}Python/addon";

cp $project../PKX-IconGen.Python.Core/*.py "${output}Python/core";
cp $project../PKX-IconGen.Python.Core/data/*.py "${output}Python/core/data";