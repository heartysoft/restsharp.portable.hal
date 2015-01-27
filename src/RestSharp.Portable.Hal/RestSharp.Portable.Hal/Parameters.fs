namespace RestSharp.Portable.Hal
open RestSharp.Portable

type Follow =
        | LinkFollow of string * Parameter list
        | HeaderFollow of string
    and 
        RequestParameters = { rootUrl : string; follow: Follow list; urlSegments : Parameter list; }
    and
        EnvironmentParameters = { client : RestClient; headers : Parameter list; httpClientFactory : IHttpClientFactory option }

