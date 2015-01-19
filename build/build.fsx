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

let buildMode = getBuildParamOrDefault "buildMode" "Release"


// Default target
Target "Default" (fun _ ->
    trace "Finished default build. Bye bye."
)

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    let setParams x = {x with 
                        Verbosity=Some(Quiet); 
                        Targets=["Clean,Build"]; 
                        Properties = 
                            [
                                "Configuration", buildMode
                                "DebugSymbols", "True"
                                "Optimize", "True"
                            ]}
    "./src/RestSharp.Portable.Hal/RestSharp.Portable.Hal.sln"
        |> build setParams
        |> ignore
)

Target "Test" (fun _ ->
    !! (sprintf "./src/RestSharp.Portable.Hal/**/bin/%s/*Tests.dll" buildMode)
        |> NUnit (fun p ->
            {p with
                DisableShadowCopy = true;
                OutputFile = "./TestResults.xml"
            }
        )
)


let version = config.Assembly.Version
let preVersion = if config.Assembly.PreRelease <> "" then (sprintf "-%s" config.Assembly.PreRelease) else ""
let copyright = sprintf "Copyright © %s %d" config.Assembly.Company DateTime.Now.Year
let commitHash = Information.getCurrentHash ()
let branch = Information.getBranchName @".\"
let branchPlusHash = sprintf "%s-%s" branch commitHash
let informationVersion = sprintf "%s%s" version preVersion

Target "Version" (fun _ ->
    
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
)

Target "NUGET_HAL" (fun _ ->
    // Copy all the package files into a package folder
    let hal = config.Nuget.HAL    

    NuGet (fun p -> 
        {p with
            Authors = hal.authors |> List.ofSeq
            Project = hal.id
            Description = hal.description
            OutputPath = ".\out"
            Summary = hal.summary
            Version = informationVersion
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Tags = hal.tags
            Copyright = copyright
            Publish = hasBuildParam "publish" }) 
        "./nuget/RestSharp.Portable.Hal.nuspec"
)

Target "NUGET_HALCS" (fun _ ->
    // Copy all the package files into a package folder
    let hal = config.Nuget.HALCS    

    NuGet (fun p -> 
        {p with
            Authors = hal.authors |> List.ofSeq
            Project = hal.id
            Description = hal.description
            OutputPath = ".\out"
            Summary = hal.summary
            Version = informationVersion
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Tags = hal.tags
            Copyright = copyright
            Publish = hasBuildParam "publish" }) 
        "./nuget/RestSharp.Portable.Hal.CSharp.nuspec"
)

Target "CC" (fun _ ->
    let b = hasBuildParam "publish"
    trace (b.ToString())
)

Target "Nuget" (fun _ ->
    ()
)

"Clean"
    ==> "Version"
    ==> "Build"
    ==> "Test"
    ==> "Default"
 
"Test"
    ==> "NUGET_HAL"
    ==> "Nuget"

"Test"
    ==> "NUGET_HALCS"
    ==> "Nuget"

    

// start build
RunTargetOrDefault "Default"

