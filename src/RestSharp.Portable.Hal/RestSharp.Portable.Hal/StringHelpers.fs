namespace RestSharp.Portable.Hal.Helpers

[<AutoOpen>]
module StringHelpers = 
    open System
    
    let toCamelCase (k:String) = 
        let first = Char.ToLowerInvariant k.[0]
        let rest = k.Substring(1)
        sprintf "%c%s" first rest

    let convertToCamelCase convert kv = 
        match convert with
        | false -> kv
        | true -> 
            kv 
            |> List.map (fun (k, v) -> (toCamelCase k, v))


