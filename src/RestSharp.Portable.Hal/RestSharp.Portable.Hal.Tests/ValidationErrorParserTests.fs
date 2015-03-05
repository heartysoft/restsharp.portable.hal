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
"Your name is a bit weird. Are you sure it's Yoda?",
"Names less than five characters are not allowed....mwahahahahah",
"Need to make the length 3...so...."
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
        ve.errors.Count === 2
        ve.errors.["name"].Length === 3
        ve.errors.["name"].[2] === "Need to make the length 3...so...."
        ve.errors.["age"].[0] === "Yeah, right. You ain't 350 and I know it."

                

