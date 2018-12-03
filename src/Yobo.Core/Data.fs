module Yobo.Core.Data

open System

type DbError =
    | ItemNotFound of obj
    | Exception of exn

let internal oneOrError<'a> i (s:seq<'a>) =
    try
        s |> Seq.head |> Ok
    with _ -> DbError.ItemNotFound(i) |> Error

let internal tryQueryM errorMapFn ctx f = 
    try 
        f ctx
    with ex -> ex |> errorMapFn |> Error

let internal tryQuery ctx f = tryQueryM (fun ex -> DbError.Exception(ex)) ctx f