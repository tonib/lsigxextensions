using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Artech.Architecture.Common.Packages;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LSI.Packages.Extensiones")]
[assembly: AssemblyDescription("Un conjunto de extensiones para Genexus")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Laboratorio de sistemas informaticos")]
[assembly: AssemblyProduct("LSI.Extensiones")]
[assembly: AssemblyCopyright("Laboratorio de sistemas informaticos 2015")]
[assembly: AssemblyTrademark("LSI.Extensiones")]
[assembly: AssemblyCulture("")]

// The following attributes are declarations related to this assembly
// as a GeneXus Package
//[assembly: PackageAttribute(typeof(LSI.Packages.Extensiones.Package))]
[assembly: PackageAttribute(typeof(LSI.Packages.Extensiones.Package), IsCore = false, IsUIPackage = true)] 

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6e0659a9-ffe1-4d2c-a4ff-4d6c7eeae365")]
[assembly: AssemblyVersion("6.1.0")]
[assembly: AssemblyFileVersion("6.1.0")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

