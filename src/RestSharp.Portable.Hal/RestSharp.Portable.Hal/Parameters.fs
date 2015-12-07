namespace RestSharp.Portable.Hal
open RestSharp.Portable
open Newtonsoft.Json.Linq

type Follow =
        | LinkFollow of string * Parameter list
        | HeaderFollow of string
    and 
        RequestParameters = { rootUrl : string; follow: Follow list; urlSegments : Parameter list; }

type ResponseVerificationStrategy = IRestResponse -> string -> JObject -> Unit
  
type EnvironmentParameters = { client : RestSharp.Portable.HttpClient.RestClient; headers : Parameter list; httpClientFactory : IHttpClientFactory option; responseVerificationStrategy : ResponseVerificationStrategy}

