#!/usr/bin/python3
from os import *
from os.path import *
import shutil
import platform
import sys

if platform.system() == "Windows":
    system("cls")
else:
    system("clear")

productname = sys.argv[1]

print("pybuild, version 0.3.0, not up-to-date, stay tuned")
print("building app \"" + productname + "\"...")

config = 0
print("selected configuration #" + str(config))

includefolder = abspath("./..")

if config == 0:
    gcc = "i686-elf"
    gxxargs = " -g -fpie -ffreestanding -Wall -Wextra -fno-exceptions -fno-asynchronous-unwind-tables -O0"
    gccargs = " -g -fpie -ffreestanding -Wall -Wextra -O0"

source = "./"
output = "bin/" + productname + ".bin"
objfolder = "bin/"
restorefolders = ["bin"]

debug = False

cobjects = ""

def restorestruct():
    global restorefolders

    print("restoring folder structure...")

    for folder in restorefolders:
        try: mkdir(folder); 
        except: pass

def buildsrc(path):
    global cobjects

    print("building file at \"" + path + "\"...")

    if path.endswith(".c"):
        print("    compiling file " + path + " ...", end = '')
        cobjects += objfolder + path.replace(".c", ".o ")
        cmd = gcc + "-gcc -c " + source + path + " -o " + objfolder + path.replace(".c", ".o") + gccargs
        if debug: print(cmd)
        system(cmd)
        print(" done!")
    elif path.endswith(".cpp"):
        print("    compiling file " + path + " ...", end = '')
        cobjects += objfolder + path.replace(".cpp", ".o ")
        cmd = gcc + "-g++ -c " + source + path + " -o " + objfolder + path.replace(".cpp", ".o") + gxxargs
        if debug: print(cmd)
        system(cmd)
        print(" done!")
    
def linkapp():
    global cobjects
    global output

    print("linking app...", end = '')
    try:
        cmd = gcc + "-gcc -T linker.ld -fpie -o " + output + " -ffreestanding -static -nostdlib -O0 " + cobjects + "../wdOS.SDK/bin/librt.bin"
        if debug: print(cmd)
        system(cmd)
    except:
        print(" an error occurred!")
        sys.exit(1)
    finally:
        print(" done!")
    
restorestruct()
buildsrc("./" + productname + ".cpp")
linkapp()

print("building done!")