namespace System
open System.Reflection

[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyInformationalVersionAttribute("0.0.1-alpha")>]
[<assembly: AssemblyCompanyAttribute("Heartysoft Solutions Limited")>]
[<assembly: AssemblyCopyrightAttribute("Copyright © Heartysoft Solutions Limited 2015")>]
[<assembly: AssemblyMetadataAttribute("githash","master-71baf2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
