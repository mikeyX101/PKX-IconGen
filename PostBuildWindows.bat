:: 1 is output folder
:: 2 is project folder
set output=%1
set output=%output:"=%
set project=%2
set project=%project:"=%

mkdir "%output%\Python\addon"
mkdir "%output%\Python\core"
mkdir "%output%\Python\core\data"

copy "%project%..\PKX-IconGen.Python.Addon\*.py" "%output%\Python\addon"

copy "%project%..\PKX-IconGen.Python.Core\*.py" "%output%\Python\core"
copy "%project%..\PKX-IconGen.Python.Core\data\*.py" "%output%\Python\core\data"