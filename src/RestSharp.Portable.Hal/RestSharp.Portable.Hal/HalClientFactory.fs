namespace RestSharp.Portable.Hal

[<AutoOpen>]
module Factories = 
    open RestSharp.Portable

    type HalClientFactory private (headers : Parameter list, httpClientFactory:IHttpClientFactory option) = 
        new() = HalClientFactory([], None)
        
        member x.CreateHalClient(domain:string) : HalClient = 
            HalClient({EnvironmentParameters.client = new RestClient(domain); headers = headers; httpClientFactory = httpClientFactory})

        member x.HttpClientFactory httpClientFactory = 
            HalClientFactory(headers, httpClientFactory)

        member x.Header (key:string) (value:string) : HalClientFactory = 
            let p = new Parameter()
            p.Name <- key
            p.Value <- value
            p.Type <- ParameterType.HttpHeader

            HalClientFactory(p :: headers, httpClientFactory)

        member x.Accept(headerValue:string) = 
            x.Header "Accept" headerValue

