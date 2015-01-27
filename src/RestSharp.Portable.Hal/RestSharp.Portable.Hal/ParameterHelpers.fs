namespace RestSharp.Portable.Hal.Helpers

[<AutoOpen>]
module ParameterHelpers = 
   open RestSharp.Portable
   open Newtonsoft.Json.Linq 

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

   let merge (jo:JObject) data : JObject = 
        let newJo = jo.DeepClone() :?> JObject
        let newData = JObject.FromObject(data)
        let mergeSettings = new JsonMergeSettings()
        mergeSettings.MergeArrayHandling <- MergeArrayHandling.Replace

        newJo.Merge(newData, mergeSettings) |> ignore
        newJo.Remove("_links") |> ignore
        newJo.Remove("_embedded") |> ignore

        newJo


