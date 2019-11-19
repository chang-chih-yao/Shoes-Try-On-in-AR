# OpenPose(Server)

## Installation

### OpenPose(Python API)

1. Clone OpenPose v1.5.1 : https://github.com/CMU-Perceptual-Computing-Lab/openpose/tree/v1.5.1
2. Follow the instructions : https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/v1.5.1/doc/installation.md  
In OpenPose Configuration step 2 : `However, new CMake versions require you to select only the VS version as the generator, e.g., Visual Studio 15 2017, and then you must manually choose x64 for the Optional platform for generator.`
3. Python API : `To install the Python API, ensure that the BUILD_PYTHON flag is turned on while running CMake GUI.`  
Python version : python 3.7
4. Put `webcam.py` in folder `openpose(clone root)/build(build name)/examples/tutorial_api_python`.