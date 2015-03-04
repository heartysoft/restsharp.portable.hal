module HalClientEmbeddedTests
open HalClientTestInfra
open RestSharp.Portable.Hal
open FsUnit
open NUnit.Framework

type HalEmbeddedTests() = 
    let mutable client:HalClient = TestConfig.CreateClient()

    [<SetUp>]
    member test.Setup () =
        client <- TestConfig.CreateClient() 
        ()

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




    

