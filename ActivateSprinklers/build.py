#!/usr/bin/python

import sys

from shutil import copyfile

# constants
SEPARATOR = "#SPLIT#"
BUILDPATHFILES = ["ActivateSprinklers.dll", "ActivateSprinklers.pdb"]
SOURCEPATHFILES = ["manifest.json"]

# variables
modPath = ""
buildPath = ""
sourcePath = ""
configuration = ""


# edit manifest.json file if using debug build configuration
def editmanifest(path: str):
    try:
        data = []
        # read manifest.json
        # print(f"[post-build::] Editing file '{path}'.")
        with open(f"{path}", "r") as f:
            data = f.readlines()

        # get timestamps for debug build; not needed smapi super dumb
        # datenow = date.today()
        # timenow = datetime.now()        
        # datestamp = datenow.strftime("%d%m%Y")
        # timestamp = timenow.astimezone().timetz()

        # edit versiondata; "Version": "..."
        data[3] = data[3].replace("\",", f"-{configuration.lower()}\",")

        # save file to modpath; "../steamapps/common/Stardew Valley/Mods/<mod name>/"
        print(f"[post-build::] Saving file '{path}'.")
        with open(f"{path}", 'w') as f:
            f.writelines(data)

    except Exception:
        raise Exception


try:
    # get paths; format: [file, source path, mod path]
    args = sys.argv[1].replace("\\", "/").split(SEPARATOR)

    # set values
    modPath = args[0]
    buildPath = args[1]
    sourcePath = args[2]

    # determine build configuration
    configuration = sys.argv[2].lower()

    # copy files to mod path
    for file in BUILDPATHFILES:
        # don't copy debug files if not debug
        if (configuration != "debug" and file.endswith(".pdb")):
            continue
        print(f"[post-build::] Copying '{file}' from '{buildPath}' to '{modPath}'.")
        copyfile(rf"{buildPath}{file}", rf"{modPath}{file}")
    for file in SOURCEPATHFILES:
        print(f"[post-build::] Copying '{file}' from '{sourcePath}' to '{modPath}'.")
        copyfile(rf"{sourcePath}{file}", rf"{modPath}{file}")

    if (configuration != "release"):
        print(f"[post-build::] Editing 'manifest.json' of '{modPath}{SOURCEPATHFILES[0]}'.")
        editmanifest(f"{modPath}{SOURCEPATHFILES[0]}")

except Exception:
    raise Exception
