namespace System
open System.Reflection

[<assembly: AssemblyVersionAttribute("0.0.2")>]
[<assembly: AssemblyInformationalVersionAttribute("0.0.2-alpha")>]
[<assembly: AssemblyCompanyAttribute("Heartysoft Solutions Limited")>]
[<assembly: AssemblyCopyrightAttribute("Copyright © Heartysoft Solutions Limited 2015")>]
[<assembly: AssemblyMetadataAttribute("githash","master-e8218a")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.2"
