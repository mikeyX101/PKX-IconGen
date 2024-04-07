#!/bin/bash

rm -r ./Publish/linux-x64

dotnet publish ./PKX-IconGen.AvaloniaUI/PKX-IconGen.AvaloniaUI.csproj --configuration Release --framework net8.0 --self-contained true --arch x64 --os linux -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --output ./Publish/linux-x64

mv ./Publish/linux-x64/PKX-IconGen.AvaloniaUI.pdb ./Publish/linux-x64/PKX-IconGen.pdb
mv ./Publish/linux-x64/PKX-IconGen.AvaloniaUI ./Publish/linux-x64/PKX-IconGen

chmod +x ./Publish/linux-x64/PKX-IconGen