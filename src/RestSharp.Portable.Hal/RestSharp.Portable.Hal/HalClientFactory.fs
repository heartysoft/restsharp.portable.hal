namespace RestSharp.Portable.Hal

[<AutoOpen>]
module Factories = 
    open RestSharp.Portable
    open RestSharp.Portable.Hal.Helpers

    type HalClientFactory private (headers : Parameter list, httpClientFactory:IHttpClientFactory option) = 
        new() = HalClientFactory([], None)
        
        member x.CreateHalClient(domain:string) : HalClient = 
            let client = new RestSharp.Portable.HttpClient.RestClient(domain)
            client.IgnoreResponseStatusCode <- true
            HalClient({EnvironmentParameters.client = client; headers = headers; httpClientFactory = httpClientFactory; responseVerificationStrategy = IRestResponseExtensions.verifyResponse})

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

