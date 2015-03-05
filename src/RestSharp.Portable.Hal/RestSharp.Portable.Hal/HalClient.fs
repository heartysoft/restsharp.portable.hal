namespace RestSharp.Portable.Hal
open RestSharp.Portable
open Newtonsoft.Json.Linq
open System.Runtime.CompilerServices

[<AutoOpen>]
module Client =
    open RestSharp.Portable.Hal.Helpers
    open System.Net.Http

    [<MethodImpl(MethodImplOptions.NoInlining)>] //reflection used. if inlined, non portable clients break.
    let (=>) (left:string) (right:System.Object) =
        (left, System.Convert.ToString(right))   
    
    
    type Resource = 
        { requestContext : RequestContext; response : IRestResponse; data: JObject; body:string}
        
        member this.Links = 
            this.data.["_links"]

        member this.Embedded = 
             this.data.["_embedded"]
        
        member this.Parse<'T>() = 
            this.data.ToObject<'T>()

        member this.FollowLocation () = this.FollowHeader "Location"

        member this.FollowHeader (header:string) : RequestContext =
            this.response.Headers.GetValues(header) |> Seq.head |> this.requestContext.FollowHeader
       
        member this.ApplyFollows (next:Follow) rest : RequestContext = 
            let (nextUrl, nextUrlSegments) = 
                match next with
                | LinkFollow(rel, segments) -> this.data.["_links"].[rel].Value<string>("href") , segments
                | HeaderFollow(header) -> header, []

            let newRequestParameters = 
                {
                    this.requestContext.requestParameters 
                        with rootUrl = nextUrl;                          
                         urlSegments = this.requestContext.requestParameters.urlSegments @ nextUrlSegments;
                         follow = rest 
                }

            {this.requestContext with requestParameters = newRequestParameters}

        member this.Follow (rel:string, urlSegments: (string*string) list) : RequestContext =
            this.ApplyFollows (LinkFollow(rel, urlSegments|> getUrlSegments)) []
        
        member this.Follow (rel:string) : RequestContext = 
            this.ApplyFollows (LinkFollow(rel, [])) []

        member private this.submitAsync (``method`` : System.Net.Http.HttpMethod) newData : Async<Resource> =
            async {
                let form = this.data
                let merged = merge form newData
                let url = this.Links.["self"].Value<string>("href")//TODO: fallback or maybe prefer response.uri

                let client = this.requestContext.environment.client
                let restRequest = 
                    RestRequest(url, ``method``)
                        .AddJsonBody(merged)

                let parameters = this.requestContext.environment.headers @ this.requestContext.requestParameters.urlSegments

                let req = 
                    parameters
                    |> List.fold (fun (state:IRestRequest) p -> state.AddParameter(p)) restRequest 

                let! response = client.Execute(req) |> Async.AwaitTask
                let body, jo = parse(response)
                this.requestContext.environment.responseVerificationStrategy req response body jo
                return {Resource.data = jo; Resource.response = response; Resource.requestContext = this.requestContext; body = body}
            }

        member this.PostAsync newData = 
            this.submitAsync HttpMethod.Post newData
        member this.PutAsync data =  this.submitAsync System.Net.Http.HttpMethod.Put data
        member this.DeleteAsync data =  this.submitAsync System.Net.Http.HttpMethod.Delete data 

    and
        RequestContext = 
        { environment: EnvironmentParameters; requestParameters : RequestParameters}

        member private this.getResponse () : Async<Resource> = 
            async {
                let client = this.environment.client
                
                match this.environment.httpClientFactory with
                | Some f -> client.HttpClientFactory <- f
                | _ -> ()

                let restRequest = RestRequest(this.requestParameters.rootUrl) :> IRestRequest
                
                let parameters = this.environment.headers @ this.requestParameters.urlSegments

                let req = 
                    parameters
                    |> List.fold (fun (state:IRestRequest) p -> state.AddParameter(p)) restRequest 

                let! res = client.Execute(req) |> Async.AwaitTask

                let body, jo = parse(res)
                this.environment.responseVerificationStrategy req res body jo
                return { Resource.requestContext = this; response = res; data=jo; body=body}
            } 

        static member private getEmbeddedResource (embedded:JToken) (follow:Follow) : Option<JToken> = 
            match follow with
            | HeaderFollow(_) -> None
            | LinkFollow(rel, segments) ->
                if embedded = null then
                    None
                else if embedded.Type = Newtonsoft.Json.Linq.JTokenType.Null then None
                     else
                        let target = embedded.[rel]
                        if target = null then
                            None
                        else if target.Type = Newtonsoft.Json.Linq.JTokenType.Null then None
                            else Some target
                
        member private this.getNext (resource:Resource) : Async<Resource> = 
            async {
                let final = 
                    match resource.requestContext.requestParameters.follow with
                    | [] -> resource
                    | x :: xs -> 
                        let embedded = RequestContext.getEmbeddedResource resource.Embedded x
                        match embedded with
                        | None -> 
                            let newRequest : RequestContext = resource.ApplyFollows x xs
                            newRequest.GetAsync() |> Async.RunSynchronously
                        | Some jt -> 
                            let rp = {resource.requestContext.requestParameters with follow = xs }
                            let rc = {resource.requestContext with requestParameters=rp }
                            let nextResource = {resource with data=jt :?> JObject; requestContext = rc }

                            this.getNext nextResource |> Async.RunSynchronously

                return final
            }

        member this.GetAsync () : Async<Resource> = 
            async {
                let! rootResponse = this.getResponse()
                return! this.getNext(rootResponse)
            }

        member this.GetAsync<'T> () : Async<'T> = 
            async{
                let! response = this.GetAsync()
                return response.Parse<'T>()
            }

        member this.Follow (rel:string, urlSegments: (string*string) list) : RequestContext =
            let rp = this.requestParameters
            let segments = getUrlSegments urlSegments

            let newRp = {rp with follow = rp.follow @ [LinkFollow(rel, segments)]}
            {this with requestParameters = newRp}
        
        member this.Follow (rel:string) : RequestContext =
            this.Follow (rel, [])

        member this.Follow (rels:string list) : RequestContext = 
            match rels with
            | [] -> this
            | x::xs -> this.Follow(x).Follow(xs)

        member this.FollowHeader (headerPath:string) : RequestContext =
            let rp = this.requestParameters
            let newRp = {rp with follow = rp.follow @ [HeaderFollow(headerPath)]}
            {this with requestParameters = newRp}
        

        member this.UrlSegments (urlSegments: (string*string) list) : RequestContext = 
           let rp = this.requestParameters
           let segments = getUrlSegments urlSegments
           let newRp = {rp with urlSegments = rp.urlSegments @ segments}
           {this with requestParameters = newRp} 

    type HalClient (env:EnvironmentParameters) = 
        member this.From (apiRelativeRoot:string) : RequestContext = 
            {
                RequestContext.environment = env;
                requestParameters = {rootUrl = apiRelativeRoot; follow = []; urlSegments = []; }
            }

    






   



