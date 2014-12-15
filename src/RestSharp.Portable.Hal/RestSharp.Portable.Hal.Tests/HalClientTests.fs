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

    
    



    
