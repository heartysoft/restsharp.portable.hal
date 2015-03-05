namespace RestSharp.Portable.Hal.Helpers
[<AutoOpen>]
module ValidationErrorExtensions = 
    open Newtonsoft.Json.Linq
    open RestSharp.Portable.Hal
    let parseValidationErrors (jo:JObject) : ValidationError =
        let message = jo.Value<string>("message")
        {ValidationError.message = message; errors = Map.empty}

    ///Present to facilitate testing without PCL numptiness.
    let parseValidationErrorsFromBody (body:string) : ValidationError =
        body |> JObject.Parse |> parseValidationErrors


