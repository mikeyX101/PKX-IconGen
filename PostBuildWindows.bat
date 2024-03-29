set output=%1
set output=%output:"=%
set project=%2
set project=%project:"=%

::TODO remove all __pycache__ in %project%\..\PKX-IconGen.Python

rmdir /Q /S "%output%\Python"

mkdir "%output%\Python\data"
mkdir "%output%\Python\importer"

xcopy "%project%\..\PKX-IconGen.Python\data\*.py" "%output%\Python\data" /Y /E
xcopy "%project%\..\PKX-IconGen.Python\*.py" "%output%\Python" /Y /E
xcopy "%project%\..\PKX-IconGen.Python\template.blend" "%output%\Python" /Y /E
xcopy "%project%\..\PKX-IconGen.Python\importer\*" "%output%\Python\importer" /Y /E

xcopy "%project%\..\PKX-IconGen.AvaloniaUI\Assets\gen-icon.png" "%output%" /Y /E
xcopy "%project%\..\PKX-IconGen.AvaloniaUI\Assets\gen-icon-bright.png" "%output%" /Y /E