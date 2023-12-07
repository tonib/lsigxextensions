import os
import glob
import subprocess
import xml.etree.ElementTree as ET
import zipfile
from win32api import GetFileVersionInfo, LOWORD, HIWORD

##############################################
# Setup
##############################################

# Run:
# pip install -r requirements.txt

##############################################
# Configuration
##############################################
# MsBuild path
MsBuildPath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Professional\\MSBuild\\Current\\Bin\\MSBuild.exe"
# Directory name where release zip will be created
ReleaseDir = "Releases"
# Dll name from which get the version number for the release zip
DllToGetVersion = "LSI.Packages.Extensiones.dll"

def get_version_number(filename):
   info = GetFileVersionInfo (filename, "\\")
   ms = info['FileVersionMS']
   ls = info['FileVersionLS']
   return f"{HIWORD(ms)}.{LOWORD(ms)}.{HIWORD(ls)}.{LOWORD(ls)}"
   
def prepare_project_release(project_dir: str, gx_version_name: str, catalog_path="LsiGxExtensions-Gx16\\Catalog.xml"):

    # Get project name
    project_file_path = next(iter(glob.glob(f'{project_dir}/*.csproj')), None)
    if not project_file_path:
        raise Exception(f"Project not found in {project_dir}")
    
    # Compile with release configuration:
    cmd = [ MsBuildPath, project_file_path, "/t:Rebuild", "/p:Configuration=Release", "/p:Platform=AnyCPU" ]
    print("Compiling project", ' '.join(cmd) )
    result = subprocess.run(cmd , shell=True )
    if result.returncode != 0:
        raise Exception("Msbuild compilation failed")

    # Parse catalog file
    print("Parsing catalog", catalog_path)
    root = ET.parse(catalog_path).getroot()
    files_to_zip = [file_node.attrib["Name"] for file_node in root.findall("./Project/File")]
    
    # Get compiled version
    version = get_version_number(os.path.join(project_dir, "bin\\Release", DllToGetVersion))
    
    # Create zip
    zip_path = os.path.join(ReleaseDir, f'LsiExtensions-{version}_{gx_version_name}.zip')
    print("Crating zip", zip_path)
    with zipfile.ZipFile(zip_path, 'w') as zipMe:        
        for file in files_to_zip:
            zipMe.write(os.path.join(project_dir, "bin\\Release", file), file, compress_type=zipfile.ZIP_DEFLATED)

##############################################
# Script execution
##############################################

if not os.path.exists(ReleaseDir):
    print("Creating directory", ReleaseDir)
    os.makedirs(ReleaseDir)
        
prepare_project_release("LsiGxExtensions-XEv3", 'XEv3')
prepare_project_release("LsiGxExtensions-Gx15", 'Gx15')
prepare_project_release("LsiGxExtensions-Gx16", 'Gx16')
prepare_project_release("LsiGxExtensions-Gx17", 'Gx17')
prepare_project_release("LsiGxExtensions-Gx18", 'Gx18')
