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

productname = "librt"

print("pybuild, version 0.3.0, not up-to-date, stay tuned")
print("building app \"" + productname + "\"...")

config = 0
print("selected configuration #" + str(config))

includefolder = abspath("./..")

if config == 0:
    gcc = "i686-elf"
    gxxargs = " -g -ffreestanding -Wall -Wextra -fno-exceptions -fno-asynchronous-unwind-tables -O0"
    gccargs = " -g -ffreestanding -Wall -Wextra -O0"

source = "librt/"
output = "bin/librt.bin"
objfolder = "bin/"
restorefolders = ["bin"]

debug = False

cobjects = ""

def restorestruct():
    global restorefolders

    print("restoring folder structure...")

    for folder in restorefolders:
        try: shutil.rmtree(folder); 
        except: pass

        try: mkdir(folder); 
        except: pass

def builddir(dir):
    global cobjects

    print("building files from dir \"" + dir + "\"...")
    #try:
    files = [f for f in listdir(getcwd() + "/" + source + dir) if isfile(join(getcwd() + "/" + source + dir, f))]

    for x in files:
        if x.endswith(".c"):
            print("    compiling file " + x + " ...", end = '')
            cobjects += objfolder + x.replace(".c", ".o ")
            cmd = gcc + "-gcc -c " + source + dir + x + " -o " + objfolder + x.replace(".c", ".o") + gccargs
            if debug: print(cmd)
            system(cmd)
            print(" done!")
        elif x.endswith(".cpp"):
            print("    compiling file " + x + " ...", end = '')
            cobjects += objfolder + x.replace(".cpp", ".o ")
            cmd = gcc + "-g++ -c " + source + dir + x + " -o " + objfolder + x.replace(".cpp", ".o") + gxxargs
            if debug: print(cmd)
            system(cmd)
            print(" done!")

    #except:
    #    print(" an error occurred!")
    #    sys.exit(1)
    #finally:
    #    print(" done!")
    
def linklibrary():
    global cobjects
    global output

    print("linking library...", end = '')
    try:
        cmd = gcc + "-gcc -flinker-output=dyn -o " + output + " -ffreestanding -shared -static -nostdlib -O0 " + cobjects
        if debug: print(cmd)
        system(cmd)
    except:
        print(" an error occurred!")
        sys.exit(1)
    finally:
        print(" done!")
    
restorestruct()
builddir("./")
linklibrary()

print("building done!")