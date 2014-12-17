namespace RestSharp.Portable.Hal.CSharp

open RestSharp.Portable.Hal
open System.Collections.Generic
open System.Threading.Tasks
open System.Reflection
open System

type Resource internal (inner:Client.Resource) = 
    member this.Parse<'T>() = 
        inner.Parse<'T>()
    member this.Response = inner.response
and
    RequestContext internal (inner:Client.RequestContext) = 

    let getAnonymousValues (obj:System.Object) = 
            obj.GetType().GetRuntimeProperties()
            |> Seq.map (fun o -> o.Name => o.GetValue(obj))
            |> List.ofSeq

    member this.GetAsync () = 
        let work = async {
            let! result = inner.GetAsync()
            return Resource(result)
        }
        work |> Async.StartAsTask
    
    member this.GetAsync<'T> () = 
        inner.GetAsync<'T>() |> Async.StartAsTask

    member this.PostAsync data = 
        let work = async{
            let! res = inner.PostAsync data
            return Resource(res)
        }
        work |> Async.StartAsTask

    member this.PostAsyncAndParse<'T> data = 
        inner.PostAsyncAndParse<'T> data |> Async.StartAsTask

    member this.UrlSegments (segments:System.Object) = 
        let properties = getAnonymousValues segments
        RequestContext (inner.UrlSegments(properties))
    
    member this.Follow (rel:string) = 
        RequestContext(inner.Follow rel)

    member this.Follow ([<ParamArray>] rels: string array) = 
        RequestContext (inner.Follow(List.ofArray rels))
    member this.Follow (rel:string, segments:System.Object) = 
        let properties = getAnonymousValues segments
        RequestContext (inner.Follow(rel, properties))

type HalClient internal (inner:Client.HalClient) = 
    member this.From domain = 
        inner.From domain
        |> fun rc -> RequestContext(rc)

type HalClientFactory private (inner: Client.HalClientFactory) = 
    new() = HalClientFactory(Client.HalClientFactory())

    member this.Accept x = 
        inner.Accept x
        |> fun c -> HalClientFactory(c)

    member this.CreateHalClient domain = 
        inner.CreateHalClient domain
        |> fun c -> HalClient(c)
