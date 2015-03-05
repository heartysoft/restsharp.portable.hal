module HalClientValidationTests
open HalClientTestInfra
open RestSharp.Portable.Hal
open FsUnit
open NUnit.Framework


[<TestFixture>]
type HalRemoteErrorTests() = 
    let mutable client:HalClient = TestConfig.CreateClient()

    [<SetUp>]
    member test.Setup () =
        client <- TestConfig.CreateClient() 
        ()

    [<Test>]
    member test.``should get validation errors`` () = 
        let resource = 
            client.From("api/witherror")
                .Follow("error-details", ["id" => "2"])
                .GetAsync() |> Async.Catch |> Async.RunSynchronously
        
        match resource with
        | Choice2Of2 (:?RemoteValidationException as e) -> 
             System.Console.WriteLine(e.ResponseBody)
             Assert.Pass("Exception thrown")
        | e -> Assert.Fail(sprintf "Did not throw expected exception. Got: %A" e) 

        
    [<Test>]
    member test.``should get arbitrary errors`` () = 
        let resource = 
            client.From("api/witherror")
                .Follow("error-details", ["id" => "1"])
                .GetAsync() |> Async.Catch |> Async.RunSynchronously
        
        match resource with
        | Choice2Of2 (:?UnexpectedResponseException as e) -> 
             System.Console.WriteLine(e.ResponseBody)
             Assert.Pass("Exception thrown")
        | e -> Assert.Fail(sprintf "Did not throw expected exception. Got: %A" e) 