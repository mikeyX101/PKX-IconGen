rmdir /Q /S ./Publish/win-x64

dotnet publish ./PKX-IconGen.AvaloniaUI/PKX-IconGen.AvaloniaUI.csproj --configuration Release --framework net8.0 --self-contained true --arch x64 --os win -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --output ./Publish/win-x64

ren "%output%\PKX-IconGen.AvaloniaUI.pdb" "PKX-IconGen.pdb"
ren "%output%\PKX-IconGen.AvaloniaUI.exe" "PKX-IconGen.exe" 