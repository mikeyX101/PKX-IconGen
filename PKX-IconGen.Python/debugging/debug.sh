#!/bin/bash

# This is used to quickly launch Blender with the PKX-IconGen addon without the need of the C# PKX-IconGen application and allow debugging using PyCharm.

# Make sure that your models are at the path in your JSON or you have an assets ppth set
# Working directory must be this directory
# Arguments:
#   $1 = Blender executable path
#   $2 = Script to run, "modify_data.py" or "render.py"
#   $3 = Path to JSON data exported from PKX-IconGen
#   $4 = Optional, Path to debug egg from PyCharm to remote debug scripts
#   $5 = Optional, equivalent to {{AssetsPath}} from PKX-IconGen

cd ..
cp ./template.blend ./debugging/debug.blend # To avoid overriding template.blend
$1 --debug-python --enable-autoexec --python-exit-code 200 ./debugging/debug.blend --python "$2" -- --pkx-debug "$3" --debug-egg "$4" --assets-path "$5"
wait