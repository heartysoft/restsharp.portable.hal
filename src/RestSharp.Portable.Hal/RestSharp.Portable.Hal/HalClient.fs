namespace RestSharp.Portable.Hal
open RestSharp.Portable
open Newtonsoft.Json.Linq

type Follow = {rel:string}

type RequestParameters = { rootUrl : string; follow: Follow list }

type EnvironmentParameters = { domain : string; headers : Parameter list }


type Resource  = 
    { requestContext : RequestContext; response : IRestResponse; data: JObject}
    member this.Parse<'T>() = 
        this.data.ToObject<'T>()
    member this.Follow next rest : RequestContext = 
        let newRequestParameters = 
            {this.requestContext.requestParameters with {rootUrl = next; follow=rest }}
        {this.requestContext with requestParameters = newRequestParameters}
and
    RequestContext = 
    { environment: EnvironmentParameters; requestParameters : RequestParameters}
    member private this.parse (response:IRestResponse) : JObject = 
        let encodingStr = 
//HOLY KRAP BATMAN...WHY Contains throw exceptionz?!?!?!
//            match this.response.Headers.Contains("Content-Encoding") with
//            | true -> 
//                this.response.Headers.GetValues("Content-Encoding")
//                |> Seq.head
//            | _ -> 
            "UTF-8"
                         
        let encoding = System.Text.Encoding.GetEncoding encodingStr
        let str = encoding.GetString(response.RawBytes, 0, response.RawBytes.Length)
        
        JObject.Parse(str)

    member private this.getResponse () : Async<Resource> = 
        async {
            let client = RestClient(this.environment.domain)
            let restRequest = RestRequest(this.requestParameters.rootUrl) :> IRestRequest
            
            let req = 
                this.environment.headers
                |> List.fold (fun (state:IRestRequest) p -> state.AddParameter(p)) restRequest 

            let! res = client.Execute(req) |> Async.AwaitTask
            
            let data = this.parse(res)
            return { Resource.requestContext = this; response = res; data=data}
        } 

    member this.GetAsync () : Async<Resource> = 
        async {
            let! rootResponse = this.getResponse()
            
            let final = 
                match this.requestParameters.follow with
                | [] -> rootResponse
                | x::xs ->
                    let newRequest : RequestContext = rootResponse.Follow x xs
                    async {
                        let! res = newRequest.GetAsync()
                        return res
                    }
            return final
        }
//        this.requestParameters.follow
//        |> List.fold (fun (state:Resource) follow -> 
//                
//                state) rootResponse

        
    member this.Follow (rel:string) : RequestContext =
        let rp = this.requestParameters
        let newRp = {rp with follow = {rel=rel}::rp.follow}

        {this with requestParameters = newRp}
         


type HalClient (env:EnvironmentParameters) = 
    member this.From (apiRelativeRoot:string) : RequestContext = 
        {
            RequestContext.environment = env;
            requestParameters = {rootUrl = apiRelativeRoot; follow = []}
        }


type HalClientFactory(headers : Parameter list) = 
    new() = HalClientFactory([])
    
    member x.CreateHalClient(domain:string) : HalClient = 
        HalClient({EnvironmentParameters.domain = domain; headers = headers})

    member x.Header (key:string) (value:string) : HalClientFactory= 
        let p = new Parameter()
        p.Name <- key
        p.Value <- value
        p.Type <- ParameterType.HttpHeader

        HalClientFactory(p :: headers)

    member x.Accept(headerValue:string) = 
        x.Header "Accept" headerValue

   



