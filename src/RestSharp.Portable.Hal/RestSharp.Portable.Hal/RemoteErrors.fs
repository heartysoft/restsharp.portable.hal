namespace RestSharp.Portable.Hal
[<AutoOpen>]
module RemoteErrors = 
    open RestSharp.Portable
    open Newtonsoft.Json.Linq

    type ValidationError = {message: string; errors : Map<string, string list>}
    type RemoteValidationException(error:ValidationError, response:IRestResponse, body:string, jo:JObject, attemptedRequest :IRestRequest ) = 
        inherit System.Exception(error.message)
        member this.Errors = error.errors
        member this.Response = response
        member this.ResponseBody = body
        member this.AttemptedRequest = attemptedRequest
        member this.JSonBody = jo
        member this.TotalErrors () = 
            this.Errors
            |> Seq.sumBy (fun x -> x.Value.Length)

    type UnexpectedResponseException(response:IRestResponse, body:string, jo:JObject, attemptedRequest:IRestRequest) = 
        inherit System.Exception("Unexpected response received.")

        member this.Response = response
        member this.ResponseBody = body
        member this.AttemptedRequest = attemptedRequest
        member this.JSonBody = jo
    