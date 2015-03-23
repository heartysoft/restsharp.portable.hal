namespace RestSharp.Portable.Hal.Helpers
[<AutoOpen>]
module ValidationErrorExtensions = 
    open Newtonsoft.Json.Linq
    open RestSharp.Portable.Hal
    let parseValidationErrors (jo:JObject) : ValidationError =
        let message = jo.Value<string>("message")
        let errors = jo.["errors"].ToObject<System.Collections.Generic.Dictionary<string, string []>>()
        {ValidationError.message = message; errors = errors}

    ///Present to facilitate testing without PCL numptiness.
    let parseValidationErrorsFromBody (body:string) : ValidationError =
        body |> JObject.Parse |> parseValidationErrors


