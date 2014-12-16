module RestSharp.Portable.Hal.Tests

open NUnit.Framework
open RestSharp.Portable.Hal



let rootUrl = "http://localhost:62582/"
let clientFactory = 
            HalClientFactory()
                .Accept("application/hal+json")
let client = clientFactory.CreateHalClient(rootUrl)


type RegistrationForm = {
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

[<TestFixture>]
type HalTests() = 
    [<Test>]
    member test.``should be able to get resource`` () =
        let resource = client.From("api/cardholders").GetAsync() |> Async.RunSynchronously
        Assert.IsInstanceOf<Resource>(resource)

    [<Test>]
    member test.``should be able to get resource following link`` () =
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .GetAsync() |> Async.RunSynchronously
        
        let jo = resource.Parse()
        let tjo = resource.Parse<'RegisterForm>()

        
        Assert.AreEqual(-1, tjo.id)
    
    [<Test>]
    member test.``should be able to get resource following multiple links`` () =
        let resource = 
            client.From("api/cardholders")
                .Follow("register")
                .Follow("self")
                .GetAsync<RegistrationForm>() |> Async.RunSynchronously
        
        Assert.AreEqual(-1, resource.id)
    
    [<Test>]
    member test.``should follow templated link`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardHolder", ["id" => "123"])
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        Assert.AreEqual(expected, resource)
   
    [<Test>]
    member test.``should allow url segment state`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "123"])
                .Follow("cardHolder")
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 123;
            name = "Customer Number123";
            anotherCard = { idAgain = "again" }
        }

        Assert.AreEqual(expected, resource)
        
    [<Test>]
    member test.``provided url segment should take precedence`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "123"])
                .Follow("cardHolder", ["id" => "112"])
                .GetAsync<CardHolderDetails>() |> Async.RunSynchronously
        
        let expected = {
            CardHolderDetails.id = 112;
            name = "Customer Number112";
            anotherCard = { idAgain = "again" }
        }

        Assert.AreEqual(expected, resource)  

    

    [<Test>]
    member test.``json nuances`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardHolder", ["id" => "112"])
                .GetAsync()
                |> Async.RunSynchronously
        
        let embedded = resource.data.["_embedded"]

        Assert.IsNotNull embedded

        let target = embedded.["card"]
        Assert.IsNotNull target

        let ne = Newtonsoft.Json.Linq.JObject.Parse("{foo:20}")
        let wn = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:null}") //pain
        let wo = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:22}")
        let we = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{card:1234}}")
        let wen = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{card:null}}") //pain
        let wenc = Newtonsoft.Json.Linq.JObject.Parse("{foo:20, _embedded:{foo:null}}")
        
        ne.["_embedded"] |> Assert.IsNull

        wo.["_embedded"] |> Assert.IsNotNull
        
        we.["_embedded"].["card"] |> Assert.IsNotNull
        wenc.["_embedded"].["card"] |> Assert.IsNull

        Assert.AreEqual(Newtonsoft.Json.Linq.JTokenType.Null, wn.["_embedded"].Type)


    [<Test>]
    member test.``should follow multiple rels in one go`` () = 
        let resource = 
            client.From("api/cardholders")
                .UrlSegments(["id" => "112"])
                .Follow(["cardHolder"; "card"])
                .GetAsync<CardEmbedded>() |> Async.RunSynchronously
        
        let expected = { CardEmbedded.number = 101; ``type``="mastercard" }

        Assert.AreEqual(expected, resource)  

    [<Test>]
    member test.``should get embedded resource`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardHolder", ["id" => "112"])
                .Follow("card")
                .GetAsync<CardEmbedded>() |> Async.RunSynchronously
        
        let expected = { CardEmbedded.number = 101; ``type``="mastercard" }

        Assert.AreEqual(expected, resource)  

    [<Test>]
    member test.``should follow link in embedded resource`` () = 
        let resource = 
            client.From("api/cardholders")
                .Follow("cardHolder", ["id" => "112"])
                .Follow("card")
                .Follow("loadCard")
                .GetAsync<LoadCardForm>() |> Async.RunSynchronously
        
        let expected = { LoadCardForm.amount = 100M; currency="GBP" }

        Assert.AreEqual(expected, resource)  




    
