namespace RestSharp.Portable.Hal
open RestSharp.Portable
open Newtonsoft.Json.Linq

[<AutoOpen>]
module Client =
  
    let inline (=>) (left:string) (right) =
        (left, right.ToString())

    type Follow = 
        {rel:string; urlSegments:Parameter list}
    and 
        RequestParameters = { rootUrl : string; follow: Follow list; urlSegments : Parameter list }
    and
        EnvironmentParameters = { domain : string; headers : Parameter list }
    
    type Resource  = 
        { requestContext : RequestContext; response : IRestResponse; data: JObject}
        member this.Parse<'T>() = 
            this.data.ToObject<'T>()
        member this.Follow (next:Follow) rest : RequestContext = 
            let nextUrl = this.data.["_links"].[next.rel].Value<string>("href") 
            let nextUrlSegments = next.urlSegments

            let newRequestParameters = 
                {
                    this.requestContext.requestParameters 
                        with rootUrl = nextUrl;                          
                         urlSegments = this.requestContext.requestParameters.urlSegments @ next.urlSegments;
                         follow=rest 
                }

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
                
                let parameters = this.environment.headers @ this.requestParameters.urlSegments

                let req = 
                    parameters
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
                            newRequest.GetAsync() |> Async.RunSynchronously
                        
                return final
            }

        member this.GetAsync<'T> () : Async<'T> = 
            async{
                let! response = this.GetAsync()
                return response.Parse<'T>()
            }
          
        member this.Follow (rel:string, urlSegments: (string*string) list) : RequestContext =
            let rp = this.requestParameters
            let segments = 
                urlSegments 
                |> List.map (
                    fun (k, v) -> 
                        let p = new Parameter()
                        p.Name <- k
                        p.Value <- v
                        p.Type <- ParameterType.UrlSegment
                        p
                    )

            let newRp = {rp with follow = rp.follow @ [{rel=rel; urlSegments=segments}]}
            {this with requestParameters = newRp}
        
        member this.Follow (rel:string) : RequestContext =
            this.Follow (rel, [])

        member this.UrlSegments (urlSegments: (string*string) list) : RequestContext = 
           let segments = 
                urlSegments 
                |> List.map (
                    fun (k, v) -> 
                        let p = new Parameter()
                        p.Name <- k
                        p.Value <- v
                        p.Type <- ParameterType.UrlSegment
                        p
                    )
           let newRp = {this.requestParameters with urlSegments = this.requestParameters.urlSegments @ segments}
           {this with requestParameters = newRp} 

    type HalClient (env:EnvironmentParameters) = 
        member this.From (apiRelativeRoot:string) : RequestContext = 
            {
                RequestContext.environment = env;
                requestParameters = {rootUrl = apiRelativeRoot; follow = []; urlSegments = []}
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

   



