namespace RestSharp.Portable.Hal
[<AutoOpen>]
module RemoteErrors = 
    open RestSharp.Portable
    open Newtonsoft.Json.Linq

    type ValidationError = {message: string; errors : System.Collections.Generic.IDictionary<string, string []>}
    type RemoteValidationException(error:ValidationError, response:IRestResponse, body:string, jo:JObject) = 
        inherit System.Exception(error.message)
        member this.Errors = error.errors
        member this.Response = response
        member this.ResponseBody = body
        member this.JSonBody = jo
        member this.TotalErrors () = 
            this.Errors
            |> Seq.sumBy (fun x -> x.Value.Length)

    type UnexpectedResponseException(response:IRestResponse, body:string, jo:JObject) = 
        inherit System.Exception("Unexpected response received.")
        
        member this.Response = response
        member this.ResponseBody = body
        member this.JSonBody = jo
    