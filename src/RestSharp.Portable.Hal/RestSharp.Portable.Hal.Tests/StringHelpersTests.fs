module StringHelpersTests

open RestSharp.Portable.Hal.Helpers
open NUnit.Framework
open FsUnit

let inline (===) left = left |> should equal

[<Test>]
let ``should camelcase single character string`` () = 
    toCamelCase "S" === "s"

[<Test>]
let ``no change for single lower case character string`` () = 
    toCamelCase "s" === "s"

[<Test>]
let ``should camelcase longer string`` () = 
    toCamelCase "HelloThere" === "helloThere"

[<Test>]
let ``no change for alread camelcased string`` () = 
    toCamelCase "helloThere" === "helloThere"
    

