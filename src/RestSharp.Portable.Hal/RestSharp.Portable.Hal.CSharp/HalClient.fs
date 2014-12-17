namespace RestSharp.Portable.Hal.CSharp

open RestSharp.Portable.Hal
open System.Collections.Generic
open System.Threading.Tasks

type Resource internal (inner:Client.Resource) = 
    class end
and
    RequestContext internal (inner:Client.RequestContext) = 
    member this.GetAsync () = 
        let work = async {
            let! result = inner.GetAsync()
            return Resource(result)
        }
        work |> Async.StartAsTask

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
