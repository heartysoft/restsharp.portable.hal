namespace RestSharp.Portable.Hal.Helpers

[<AutoOpen>]
module JsonParsingHelpers = 
    open RestSharp.Portable
    open Newtonsoft.Json.Linq

    let parse (response:IRestResponse) : string * JObject = 
        let encodingStr = 
    //HOLY KRAP BATMAN...WHY Contains throw exceptionz?!?!?!
    //            match this.response.Headers.Contains("Content-Encoding") with
    //            | true -> 
    //                this.response.Headers.GetValues("Content-Encoding")
    //                |> Seq.head
    //            | _ -> 
            "UTF-8"
                         
        let encoding = System.Text.Encoding.GetEncoding encodingStr
        let str = encoding.GetString(response.RawBytes, 0, response.RawBytes.Length)
        
        match str with
        | "" -> (str, null )
        | _ -> (str, JObject.Parse(str))

