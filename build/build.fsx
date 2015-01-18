#r @"./../src/restsharp.portable.hal/packages/FAKE/tools/FakeLib.dll"
open Fake

let buildDir = "./out/"

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
                //ProcessModel = SeparateProcessModel;
                //Domain = MultipleDomainModel
            }
        )
)

"Clean"
    ==> "Build"
    ==> "Test"
    ==> "Default"

// start build
RunTargetOrDefault "Default"

