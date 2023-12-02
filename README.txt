- For debug set "Project properties > Debug > External program" to this:
C:\Program Files (x86)\ARTech\GeneXus\GeneXusXEv3\Genexus.exe

- To debug msbuild tasks:
	* Project > properties > debug > 
		External start: C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe
		Command line arguments: Template.msbuild
		Working directory: D:\kbases\subversion\lsigxextensions\trunk\LsiGxExtensions\MsBuild

