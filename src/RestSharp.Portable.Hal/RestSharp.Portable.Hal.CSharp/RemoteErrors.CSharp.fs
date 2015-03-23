namespace RestSharp.Portable.Hal.CSharp
open System
open System.Collections.Generic
open System.Linq

type RemoteValidationException (e:RestSharp.Portable.Hal.RemoteErrors.RemoteValidationException) =
    inherit System.Exception(e.Message)
    member this.Errors : IDictionary<string, string []> = e.Errors 
    member this.Response = e.Response
    member this.ResponseBody = e.ResponseBody
    member this.JSonBody = e.JSonBody
    member this.TotalErrors () = e.TotalErrors()
        
type UnexpectedResponseException(e:RestSharp.Portable.Hal.RemoteErrors.UnexpectedResponseException) = 
    inherit System.Exception(e.Message)
        
    member this.Response = e.Response
    member this.ResponseBody = e.ResponseBody
    member this.JSonBody = e.JSonBody