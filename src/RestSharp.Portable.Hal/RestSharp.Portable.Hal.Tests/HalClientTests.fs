module RestSharp.Portable.Hal.Tests

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
    
let rootUrl = "http://dummy-unused/"

module TestConfig = 
    let private server:TestServer = TestServer.Create<Hal.Startup>()
    let CreateClient() = 
        HalClientFactory()
            .Accept("application/hal+json")
            .HttpClientFactory(TestHttpClientFactory(server):> IHttpClientFactory |> Some)
            .CreateHalClient rootUrl

let inline (===) left = left |> should equal

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


type HalTests() = 

    let mutable client:HalClient = TestConfig.CreateClient()

    [<SetUp>]
    member test.Setup () =
        client <- TestConfig.CreateClient() 
        ()

    [<Test>]
    member test.``should be able to get resource`` () =
        let resource = client.From("api/cardholders").GetAsync() |> Async.RunSynchronously
        resource |> should be ofExactType<Resource>

    [<Test>]
    member test.``should be able to get resource following link`` () =
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .GetAsync() |> Async.RunSynchronously
        
        let tjo = resource.Parse<'RegisterForm>()
        tjo.id === -1
    
    [<Test>]
    member test.``should be able to get resource following multiple links`` () =
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .Follow("self")
                .GetAsync<RegistrationForm>() |> Async.RunSynchronously
        
        resource.id === -1
    
    [<Test>]
    member test.``should follow templated link`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardholder", ["id" => "123"])
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        resource === expected
   
    [<Test>]
    member test.``should allow url segment state`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "123"])
                .Follow("cardholder")
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        resource === expected
        
    [<Test>]
    member test.``provided url segment should take precedence`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "123"])
                .Follow("cardholder", ["id" => "112"])
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 112;
            name = "Customer Number112";
            anotherCard = { idAgain = "again" }
        }

        resource === expected

    [<Test>]
    member test.``json nuances`` () = 
        let resource = 
            client.From("api/cardholders")
                //.Follow("cardholder", [("id", "112")])
                .Follow("cardholder", ["id" => "112"])
                .GetAsync()
                |> Async.RunSynchronously
        
        let embedded = resource.data.["_embedded"]
        embedded |> should not' (be Null)

        let target = embedded.["card"]
        target |> should not' (be Null)

        let ne = Newtonsoft.Json.Linq.JObject.Parse("{foo:20}")
        let wn = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:null}") //pain
        let wo = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:22}")
        let we = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{card:1234}}")
        let wen = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{card:null}}") //pain
        let wenc = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{foo:null}}")
        
        ne.["_embedded"] |> should be Null
        wo.["_embedded"] |> should not' (be Null)
        we.["_embedded"].["card"] |> should not' (be Null)
        wenc.["_embedded"].["card"] |> should be Null

        wn.["_embedded"].Type === Newtonsoft.Json.Linq.JTokenType.Null


    [<Test>]
    member test.``should follow multiple rels in one go`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "112"])
                .Follow(["cardholder"; "card"])
                .GetAsync<CardEmbedded>() |> Async.RunSynchronously
        
        let expected = { CardEmbedded.number = 101; ``type``="mastercard" }

        resource === expected

    [<Test>]
    member test.``should get embedded resource`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardholder", ["id" => "112"])
                .Follow("card")
                .GetAsync<CardEmbedded>() |> Async.RunSynchronously
        
        let expected = { CardEmbedded.number = 101; ``type``="mastercard" }

        resource === expected

    [<Test>]
    member test.``should follow link in embedded resource`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardholder", ["id" => "112"])
                .Follow("card")
                .Follow("loadcard")
                .GetAsync<LoadCardForm>() |> Async.RunSynchronously
        
        let expected = { LoadCardForm.amount = 100M; currency="GBP" }
       
        resource === expected
  

    [<Test>]
    member test.``merging forms`` () = 
        let data = { LoadCardForm.amount = 100M; currency="GBP" }
        let resource = JObject.Parse("{amount:0, currency:'USD', unknown:22, _links: {self:'/api/foo'}, _embedded:{card:{}}}")

        let merged = merge resource data
       
        merged.Value<int>("amount") === 100
        merged.Value<string>("currency") === "GBP"
        merged.Value<int>("unknown") === 22
        merged.["_links"] |> should be Null
        merged.["_embedded"] |> should be Null

    [<Test>]
    member test.``should post form to server`` () = 
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PostAsync(newData) |> Async.RunSynchronously
       
        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.Created
        locationHeader === "/api/cardholders/55"

    [<Test>]
    member test.``should post form to server and parse body (if you want)`` () = 
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PostAsync(newData) |> Async.RunSynchronously

        let body = resource.Parse<CardHolderDetails>()
        
        body.name === "Johny"
        body.anotherCard.idAgain === "lala"

    [<Test>]
    member test.``should post form to sever and parse body nicely (if you want)`` () = 
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PostAsyncAndParse<CardHolderDetails>(newData) |> Async.RunSynchronously

        resource.name === "Johny"
        resource.anotherCard.idAgain === "lala"

    [<Test>]
    member test.``should put to server`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PutAsync(newData) |> Async.RunSynchronously

        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.Created
        locationHeader === "/api/cardholders/55"

    [<Test>]
    member test.``should delete to server`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .DeleteAsync(newData) |> Async.RunSynchronously
        
        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.OK
        locationHeader === "/api/cardholders" 

    [<Test>]
    member test.``should follow location header`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PostAsync(newData) |> Async.RunSynchronously                              

        let nr = 
            resource.FollowHeader("Location")
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        nr.name === "Customer Number55" //irl would be Johny but server does not persist
        nr.anotherCard.idAgain === "again"

    [<Test>]
    member test.``should follow location header and continue traversal`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .PostAsync(newData) |> Async.RunSynchronously                              

        let nr = 
            resource.FollowHeader("Location")
                .Follow("updatecardholder")
                .GetAsync<UpdateCardHolderForm>() |> Async.RunSynchronously

        nr.id === 0

    [<Test>]
    member test.``should follow from resource``() = 
        //i.e. from resource response, should be able to continue traversal
        let resource = 
            client.From("api/cardholders")
                .GetAsync() |> Async.RunSynchronously
        let next = resource.Follow("register")
                    .GetAsync<RegistrationForm>() |> Async.RunSynchronously
        
        next.id === -1

    [<Test>]
    member test.``should follow with segments from resource``() = 
        //i.e. from resource response, should be able to continue traversal
        let resource = 
            client.From("api/cardholders").GetAsync() |> Async.RunSynchronously
        let next =
                resource
                    .Follow("cardholder", ["id" => "123"])
                    .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        next === expected     
        
    [<Test>]
    member test.``should follow multiple times with segments from resource``() = 
        //i.e. from resource response, should be able to continue traversal
        let resource = 
            client.From("api/cardholders").GetAsync() |> Async.RunSynchronously
        let next =
                resource
                    .Follow("cardholder", ["id" => "123"])
                    .GetAsync() |> Async.RunSynchronously
        let next2 = next.Follow("self").GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        next2 === expected         

    [<Test>]
    member test.``should be able to post from fetched resource``() = 
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let form = 
            client.From("api/cardholders")
                .Follow("register").GetAsync() |> Async.RunSynchronously
        
        let resource = form.PostAsync(newData) |> Async.RunSynchronously
       
        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.Created
        locationHeader === "/api/cardholders/55" 

    [<Test>]
    member test.``should be able to post from fetched resource (async example)``() = 
        let newData = {RegistrationForm.id = 55; name="Johny"}
        
        let finalResult = 
            async{
                let! form = 
                    client.From("api/cardholders")
                             .Follow("register").GetAsync() 
                //let formData = form.Parse<>() # use parsed data

                let! postResult = form.PostAsync(newData)
                return postResult
            } |> Async.RunSynchronously

        let locationHeader = finalResult.response.Headers.GetValues "Location" |> Seq.head
        
        locationHeader === "/api/cardholders/55"
        finalResult.response.StatusCode === HttpStatusCode.Created 
    
    [<Test>]
    member test.``should put to server from resource`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource = 
            async {
                let! form = 
                    client.From("api/cardholders")
                        .Follow("register").GetAsync()
                return! form.PutAsync(newData) 
            } |> Async.RunSynchronously

        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.Created
        locationHeader === "/api/cardholders/55"

    [<Test>]
    member test.``should delete to server from resource`` () =
        let newData = {RegistrationForm.id = 55; name="Johny"}
        let resource =
            async{ 
                let! form = client.From("api/cardholders")
                                .Follow("register").GetAsync()
                return! form.DeleteAsync(newData) }|> Async.RunSynchronously
        
        let locationHeader = resource.response.Headers.GetValues("Location") |> Seq.head

        resource.response.StatusCode === HttpStatusCode.OK
        locationHeader === "/api/cardholders" 
