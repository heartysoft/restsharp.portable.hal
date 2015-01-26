namespace RestSharp.Portable.Hal.Helpers

[<AutoOpen>]
module ParameterHelpers = 
   open RestSharp.Portable

   let getUrlSegments(urlSegments: (string*string) list) : Parameter list = 
        let segments = 
            urlSegments 
            |> List.map (
                fun (k, v) -> 
                    let p = new Parameter()
                    p.Name <- k
                    p.Value <- v
                    p.Type <- ParameterType.UrlSegment
                    p
                )
        segments


