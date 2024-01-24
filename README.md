# Lsi.Extensions
This is a set of extensions for Genexus. See detailed documentation at 
[https://tonib.github.io/lsigxextensions-docs/](https://tonib.github.io/lsigxextensions-docs/).

## Download binaries

Binaries can be downloaded from 
https://sourceforge.net/projects/lsigxextensions/files/ or 
https://marketplace.genexus.com/product.aspx?lsiextensiones,en

## Build from sources

Edit SetEnvironmentVariables.bat and set the directories for Genexus and Genexus 
Platform SDK in your machine. If you don't have some Gx version on that file,
you can comment it.

There are two projects in solution for each Gx version: 
LsiGxUtilidades-XXX is a class library, and LsiGxExtensions-XXX
are the extensions for that Gx XXX version.

There is one of that LsiGxExtensions-XXX projects, called LsiGxExtensions-XXX-MAIN,
that contains the real source code. Other projects only contains LINKS to source
code in LsiGxExtensions-XXX-MAIN project.
This is like this because, I cannot use Shared Projects (Winform edition does not
work), and it means that, to make any change that adds / removes files, you should
make it at LsiGxExtensions-XXX-MAIN project, and then update links in other projects.

Same code structure / links are applied for LsiGxUtilidades-XXX.

## Debug
For debug set "Project properties > Debug > External program" to this:
[GENEXUS DIRECTORY]\Genexus.exe. Ex. C:\Program Files (x86)\ARTech\GeneXus\GeneXusXEv3

For MsBuild tasks debug:
* Project > properties > debug > 
	* External start: C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe
	* Command line arguments: Template.msbuild
	* Working directory: [PROJECT DIRECTORY]\MsBuild
