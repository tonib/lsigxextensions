##########################################
# COMPILATION SCRIPT
# COMMAND LINE IS :
# powershell "C:\...\build.ps1"
# DON'T REMOVE THE QUOTES!
#
# THE FIRST TIME EXECUTE "Set-ExecutionPolicy RemoteSigned" AS ADMINISTRATOR IN THE POWERSHELL CONSOLE AS ADMINISTRATOR
# (SEE http://technet.microsoft.com/en-us/library/ee176949.aspx)
##########################################

####### PARAMETERS

#Msbuild path (version 3.5!)
$MsBuild = "C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe"

#Log file of the process
$LogFile = "CompilationLog.txt"

#GxAlmac model names
$AlmacPrototype = "pNET GxAlmacSQL (Prototipo)"
$AlmacProduction = "Net GxAlmacSql (Producion)"

#GxAlmac generator names
$WinCsharpGenerator = "Default (C#)"
$AllGenerators = ""

######## FUNCTIONS

# Compile a Kbase
# $PathKbase: KBase path
# $Models Environment names to compile
# $Generator Generator name to compile. If it's empty, all generators will be compiled
function KBaseCompile( $PathKbase , $Models , $Generator ) 
{
    "Compiling kbase " + $PathKbase
    
    foreach( $Model in $Models ) 
    {
		"Compiling model " + $Model
		& ($MsBuild) Template.msbuild "/property:KBPath=$PathKbase" "/property:KBEnvironment=$Model" "/property:Generator=$Generator" >> $LogFile
    }
}

######## SCRIPT EXECUTION

# Remove previous log
If (Test-Path $LogFile) 
{
    Remove-Item $LogFile
}

# Connect network unit O:
& NET USE O: \\lsisrv-w12\Prog1

# Compile kbases
#KBaseCompile "O:\Programacion\Oscar\Desarrollo\Aplicaciones\Construccion\GXalmacSQL_vX2.1206" ( $AlmacPrototype ) $AllGenerators
#KBaseCompile "O:\Programacion\Oscar\Desarrollo\Aplicaciones\Construccion\GXalmacSQL_vX2.1205" ( $AlmacPrototype , $AlmacProduction ) $WinCsharpGenerator
#KBaseCompile "O:\Programacion\Oscar\Desarrollo\Aplicaciones\Construccion\GXalmacSQL_vX2.1204" ( $AlmacPrototype , $AlmacProduction ) $WinCsharpGenerator
#KBaseCompile "O:\Programacion\Oscar\Desarrollo\Aplicaciones\Construccion\GXalmacSQL_vX2.1203" ( $AlmacProduction ) $WinCsharpGenerator
KBaseCompile "D:\kbases\PruebasWinWeb" ( "Prototipo" ) $AllGenerators

