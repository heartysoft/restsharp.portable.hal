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
    member this.Data = inner.data
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

    member private this.submitAsync (``method``:string) data = 
        let work = async{
            let handler = 
                match ``method`` with
                | "POST" -> inner.PostAsync
                | "PUT" -> inner.PutAsync
                | "DELETE" -> inner.DeleteAsync
                | _ -> failwith(System.String.Format("unsupport method {0}", ``method``))

            let! res = handler data
            return Resource(res)
        }
        work |> Async.StartAsTask
    
    member this.PostAsync data = this.submitAsync "POST" data
    member this.PutAsync data = this.submitAsync "PUT" data
    member this.DeleteAsync data = this.submitAsync "DELETE" data

    member this.PostAsyncAndParse<'T> data = inner.PostAsyncAndParse<'T> data |> Async.StartAsTask
    member this.PutAsyncAndParse<'T> data =  inner.PutAsyncAndParse<'T> data |> Async.StartAsTask
    member this.DeleteAsyncAndParse<'T> data = inner.DeleteAsyncAndParse<'T> data |> Async.StartAsTask

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
