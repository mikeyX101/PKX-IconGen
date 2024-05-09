rmdir /Q /S ".\Publish\linux-x64"

dotnet publish ./PKX-IconGen.AvaloniaUI/PKX-IconGen.AvaloniaUI.csproj --configuration Release --framework net8.0 --self-contained true --arch x64 --os linux -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --output ./Publish/linux-x64

ren ".\Publish\linux-x64\PKX-IconGen.AvaloniaUI.pdb" "PKX-IconGen.pdb"
ren ".\Publish\linux-x64\PKX-IconGen.AvaloniaUI" "PKX-IconGen"
