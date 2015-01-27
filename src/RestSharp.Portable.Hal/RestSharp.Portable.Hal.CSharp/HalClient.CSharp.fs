namespace RestSharp.Portable.Hal.CSharp

open RestSharp.Portable.Hal
open RestSharp.Portable.Hal.Helpers
open RestSharp.Portable.Hal.CSharp.Helpers
open System.Collections.Generic
open System.Threading.Tasks
open System.Reflection
open System

type Resource internal (inner:Client.Resource, requestContext:RequestContext) = 
    member this.Parse<'T>() = 
        inner.Parse<'T>()
    member this.Response = inner.response
    member this.Data = inner.data
    member this.Embedded = inner.Embedded
    member this.Links = inner.Links
    member this.FollowHeader header = inner.response.Headers.GetValues(header) |> Seq.head |> requestContext.FollowHeader
    member this.Follow (rel:string) = RequestContext (inner.Follow(rel))
    member this.Follow (rel:string, segments:System.Object, toCamelCase) = 
      let properties = getAnonymousValues segments |> StringHelpers.convertToCamelCase toCamelCase
      RequestContext (inner.Follow(rel, properties))
    member this.Follow (rel:string, segments:System.Object) = this.Follow (rel, segments, false)

//    member this.Follow ([<ParamArray>] rels: string array) = 
//        RequestContext (inner.Follow(List.ofArray rels))
    
and
    RequestContext internal (inner:Client.RequestContext) = 

    

    member this.GetAsync () = 
        let work = async {
            let! result = inner.GetAsync()
            return Resource(result, this)
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
                | _ -> failwith(System.String.Format("unsupported method {0}", ``method``))

            let! res = handler data
            return Resource(res, this)
        }
        work |> Async.StartAsTask
    
    member this.PostAsync data = this.submitAsync "POST" data
    member this.PutAsync data = this.submitAsync "PUT" data
    member this.DeleteAsync data = this.submitAsync "DELETE" data

    member this.PostAsyncAndParse<'T> data = inner.PostAsyncAndParse<'T> data |> Async.StartAsTask
    member this.PutAsyncAndParse<'T> data =  inner.PutAsyncAndParse<'T> data |> Async.StartAsTask
    member this.DeleteAsyncAndParse<'T> data = inner.DeleteAsyncAndParse<'T> data |> Async.StartAsTask


    member private this.urlSegmentsHelper (segments:System.Object) toCamelCase = 
        let properties = 
            getAnonymousValues segments |> StringHelpers.convertToCamelCase toCamelCase

        RequestContext (inner.UrlSegments(properties))

    member this.UrlSegments (segments, toCamelCase) = 
        this.urlSegmentsHelper segments toCamelCase
    
    member this.UrlSegments segments = 
        this.UrlSegments (segments, false)

    member this.Follow (rel:string) = 
        RequestContext(inner.Follow rel)

    member this.Follow ([<ParamArray>] rels: string array) = 
        RequestContext (inner.Follow(List.ofArray rels))
    member this.Follow (rel:string, segments:System.Object, toCamelCase) = 
        let properties = getAnonymousValues segments |> StringHelpers.convertToCamelCase toCamelCase
        RequestContext (inner.Follow(rel, properties))
    member this.Follow (rel:string, segments:System.Object) = this.Follow (rel, segments, false)

    member this.FollowHeader(path:string) =
        RequestContext(inner.FollowHeader path)

type HalClient internal (inner:Client.HalClient) = 
    member this.From domain = 
        inner.From domain
        |> fun rc -> RequestContext(rc)

type HalClientFactory private (inner: Factories.HalClientFactory) = 
    new() = HalClientFactory(Factories.HalClientFactory())

    member this.Header k v =
        inner.Header k v
        |> fun c -> HalClientFactory(c)

    member this.Accept x = 
        inner.Accept x
        |> fun c -> HalClientFactory(c)
    
    member this.HttpClientFactory httpClientFactory = 
        inner.HttpClientFactory(Some httpClientFactory)
        |> fun c -> HalClientFactory(c)

    member this.CreateHalClient domain = 
        inner.CreateHalClient domain
        |> fun c -> HalClient(c)

