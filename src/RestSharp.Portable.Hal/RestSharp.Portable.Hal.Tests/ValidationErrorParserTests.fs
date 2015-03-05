module ValidationErrorParserTests

open HalClientTestInfra
open RestSharp.Portable.Hal
open RestSharp.Portable.Hal.Helpers
open FsUnit
open NUnit.Framework
open Newtonsoft.Json.Linq

[<TestFixture>]
type ValidationErrorParserTests() = 
    let response = """{
"type": "validation",
"message": "Overall message",
"errors": {
"name": [
"Your name is a bit weird. Are you sure it's Yoda?"
],
"age": [
"Yeah, right. You ain't 350 and I know it."
]
}
}"""

    [<Test>]
    member test.``should parse message`` () = 
        //Can't use parseValidationErrors jo due to pcl numptiness. 
        let ve = parseValidationErrorsFromBody response
        ve.message === "Overall message"

