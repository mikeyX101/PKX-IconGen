#!/bin/bash

# This is used to quickly launch Blender with the PKX-IconGen addon without the need of the C# PKX-IconGen application.

# Make sure that your models are at the path in your JSON
# Working directory must be this directory
# Arguments:
#   $1 = Blender executable path
#   $2 = Path to JSON data exported from PKX-IconGen
#   $3 = Optional, equivalent to {{AssetPath}} from PKX-IconGen

cd ..
cp ./template.blend ./debugging/debug.blend # To avoid overriding template.blend
$1 --debug-python --enable-autoexec --python-exit-code 200 ./debugging/debug.blend --python modify_data.py -- --pkx-debug "$2" --assets-path "$3"
