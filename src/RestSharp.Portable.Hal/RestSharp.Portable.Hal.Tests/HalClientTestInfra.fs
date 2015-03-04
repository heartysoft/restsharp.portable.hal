namespace HalClientTestInfra

[<AutoOpen>]
module HalClientTestInfraHelpers =
    open NUnit.Framework
    open FsUnit
    open RestSharp.Portable.Hal
    open RestSharp.Portable.Hal.Helpers
    open Newtonsoft.Json.Linq
    open System.Net
    open Microsoft.Owin
    open System
    open System.Net.Http
    open Microsoft.Owin.Hosting
    open Microsoft.Owin.Testing
    open RestSharp.Portable
    open RestSharp.Portable.HttpClientImpl

    type TestHttpClientFactory (server:TestServer) =
        inherit DefaultHttpClientFactory() 
        override this.CreateClient(restClient, request) : HttpClient = 
            server.HttpClient

    let public rootUrl = "http://dummy-unused/"

    module TestConfig = 
        let private server:TestServer = TestServer.Create<Hal.Startup>()
        let CreateClient() = 
            HalClientFactory()
                .Accept("application/hal+json")
                .HttpClientFactory(TestHttpClientFactory(server):> IHttpClientFactory |> Some)
                .CreateHalClient rootUrl

    let public (===) left = left |> should equal

    type RegistrationForm = {
            id:int;
            name:string
        }

    type UpdateCardHolderForm = {
            id:int;
            name:string
        }

    type Card = {idAgain:string}

    type CardHolderDetails = {
            id:int; 
            name:string;
            anotherCard:Card
        }

    type LoadCardForm = {amount:decimal; currency:string}

    type CardEmbedded = { number:int; ``type``:string }

