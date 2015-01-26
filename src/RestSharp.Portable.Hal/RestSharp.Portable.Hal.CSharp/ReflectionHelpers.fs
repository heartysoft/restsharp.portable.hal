namespace RestSharp.Portable.Hal.CSharp.Helpers

[<AutoOpen>]
module ReflectionHelpers = 
    open System.Reflection
    open RestSharp.Portable.Hal

    let getAnonymousValues (obj:System.Object) = 
            obj.GetType().GetRuntimeProperties()
            |> Seq.map (fun o -> o.Name => o.GetValue(obj))
            |> List.ofSeq

