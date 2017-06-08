﻿module RX = Observable
module T  = Telegram
module I  = Interactive

module Domain =
    let private inputLimit = 150
    let private outputLimit = 200
    let format (message: string) =
        if message = "/start" then Error "TODO description\n\nSource code (MIT): https://github.com/y2k/TryKotlinBot"
        else if message.Length > inputLimit then Error (sprintf "Code too long (limit is %O charactes)" inputLimit)
        else Ok (message.Replace('”', '"').Replace('“', '"'))
    
    let formatOut (message: string) =
        if message.Trim() = "" then "[ERROR] Empty output"
        else if message.Length > outputLimit then 
            message.Substring(0, outputLimit) + "...\n\n[RESULT TOO LONG (" + (string message.Length) + ")]"
        else message
        |> sprintf "```\n%O\n```"

[<EntryPoint>]
let main argv =
    T.listenForMessages argv.[0]
        |> RX.add (fun x -> 
            async {
                let pm = Domain.format x.text
                let! resp = match pm with
                            | Error e   -> async.Return e
                            | Ok script -> async {
                                               do! T.setProgress argv.[0] x.user
                                               let result = I.compileAndExecute script
                                               return Domain.formatOut result
                                           }
                do! T.send argv.[0] x.user resp
            } |> Async.Start)

    printfn "Listening for updates..."
    System.Threading.Thread.Sleep(-1);
    0