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

    //TODO: Add test for multiple url segments



    
