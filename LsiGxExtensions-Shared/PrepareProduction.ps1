# Prepare production zips

#################################
# SETTINGS
#################################

# Extensions version:
$ExtensionsVersion = "5.1.0"

# Extensions project path
$PublicExtensionsPath = "D:\kbases\subversion\lsigxextensions\branches\branch-GX17\LsiGxExtensions"

# Private extensions path
$PrivateExtensionsPath = "D:\kbases\subversion\ExtensionesGXPrivadas\branches\branch-GX17\LsiGxPrivateExtensions"

# Compression tool
If ( Test-Path "C:\Archivos de programa\WinRAR\WinRAR.exe" ) 
{
    $PathCompresor = "C:\Archivos de programa\WinRAR\WinRAR.exe"
}
elseif ( Test-Path "C:\Program Files (x86)\WinRAR\WinRAR.exe" )
{
    $PathCompresor = "C:\Program Files (x86)\WinRAR\WinRAR.exe"
}
else {
    "WINRAR is not installed"
    EXIT
}

#################################
# FUNCTIONS
#################################

function ZipProjectResults( $ProjectPath , $Files , $DestinationZipPath ) 
{
	# Ir a la carpeta bin\release, para que cuando se comprima no incluya la carpeta
	$CurrentPath = $ProjectPath + "\bin\release\"
	Set-Location -Path $CurrentPath
	
	if( Test-Path $DestinationZipPath )
	{
		# Borrarlo antes, porque si no actualiza el archivo y archivos antiguos siguen en el ZIP
		Remove-Item $DestinationZipPath
	}
	
	# Run the compressor
	& ($PathCompresor)  a ( $DestinationZipPath ) ( $Files )
	
}

#################################
# BUILD
#################################

$MsBuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"

# Compile with release configuration:
& ($MsBuild) ..\LsiGxExtensions.sln /t:Rebuild /p:Configuration=Release

#################################
# PREPARE ZIPS
#################################

# Public extensions
$PathZipBin = $PublicExtensionsPath + "\LsiExtensions-" + $ExtensionsVersion + "_Gx17-Beta.zip"
ZipProjectResults $PublicExtensionsPath ( "LSI.Packages.Extensiones.dll" , "LSI.Packages.Extensiones.Utilidades.dll" , "LsiExtensions.targets" , "TfLiteNetWrapper.dll" , "tensorflowlite_c.dll" ) $PathZipBin

# Private extensions
$PathZipBin = $PublicExtensionsPath + "\LsiExtensionesPrivadas-" + $ExtensionsVersion + "_Gx17-Beta.zip"
ZipProjectResults $PrivateExtensionsPath ( "ExCSS.dll" , "HtmlAgilityPack.dll" , "LSI.Packages.PrivateExtensions.dll" ) $PathZipBin

& PAUSE