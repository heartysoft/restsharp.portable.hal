#r @"./../src/restsharp.portable.hal/packages/FAKE/tools/FakeLib.dll"
#r @"./../src/restsharp.portable.hal/packages/FSharp.Configuration/lib/net40/FSharp.Configuration.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open FSharp.Configuration
open System

let buildDir = "./out/"

type BuildConfig = YamlConfig<"./../build-config.yaml">
let config = BuildConfig()

// Default target
Target "Default" (fun _ ->
    trace "Finished default build. Bye bye."
)

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    let setParams x = {x with Verbosity=Some(Quiet); Targets=["Clean,Build"]}
    "./src/RestSharp.Portable.Hal/RestSharp.Portable.Hal.sln"
        |> build setParams
        |> ignore
)

Target "Test" (fun _ ->
    !! ("./src/RestSharp.Portable.Hal/**/bin/**/*Tests.dll")
        |> NUnit (fun p ->
            {p with
                DisableShadowCopy = true;
                OutputFile = "./TestResults.xml"
            }
        )
)

Target "Version" (fun _ ->
    let version = config.Assembly.Version
    let preVersion = if config.Assembly.PreRelease <> "" then (sprintf "-%s" config.Assembly.PreRelease) else ""
    let copyright = sprintf "Copyright © %s %d" config.Assembly.Company DateTime.Now.Year
    let commitHash = Information.getCurrentHash ()
    let branch = Information.getBranchName @".\"
    let branchPlusHash = sprintf "%s-%s" branch commitHash
    let informationVersion = sprintf "%s%s" version preVersion


    let versionFiles creator files = 
        files
        |> Seq.iter (fun x -> 
          creator 
            x
            [
             Attribute.Version version      
             Attribute.InformationalVersion informationVersion
             Attribute.Company <| config.Assembly.Company
             Attribute.Copyright <| copyright
             Attribute.Metadata("githash", branchPlusHash)
            ]
        )

    !! ("./src/RestSharp.Portable.Hal/**/Properties/AssemblyInfo.cs")
        |> Seq.map (fun x -> x.ToString().Replace("AssemblyInfo.cs", "SolutionInfo.cs"))
        |> versionFiles CreateCSharpAssemblyInfo

    [
      "./src/RestSharp.Portable.Hal/RestSharp.Portable.Hal/SolutionInfo.fs"
      "./src/RestSharp.Portable.Hal/RestSharp.Portable.Hal.CSharp/SolutionInfo.fs"
    ]
    |> versionFiles CreateFSharpAssemblyInfo



    //commitHash.ToString() |> System.Console.WriteLine
    //CreateCSharpAssemblyInfo 
    //    !!"./src/app/Calculator/Properties/AssemblyInfo.cs"
    //    [Attribute.Title "Calculator Command line tool"
    //     Attribute.Description "Sample project for FAKE - F# MAKE"
    //     Attribute.Guid "A539B42C-CB9F-4a23-8E57-AF4E7CEE5BAA"
    //     Attribute.Product "Calculator"
    //     Attribute.Version version
    //     Attribute.FileVersion version]
)

Target "CC" (fun _ ->
    let version = config.Assembly.Version
    let preVersion = if config.Assembly.PreRelease <> "" then (sprintf "-%s" config.Assembly.PreRelease) else ""
    let copyright = sprintf "Copyright © %s %d" config.Assembly.Company DateTime.Now.Year
    let commitHash = Information.getCurrentHash ()
    let informationVersion = sprintf "%s%s" version preVersion
    trace <| informationVersion
)

"Clean"
    ==> "Version"
    ==> "Build"
    ==> "Test"
    ==> "Default"

// start build
RunTargetOrDefault "Default"

