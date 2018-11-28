module Yobo.Core.Data

open System

type DbError =
    | ItemNotFound of Guid
    | Exception of exn

let internal oneOrError<'a> i (s:seq<'a>) =
    try
        s |> Seq.head |> Ok
    with _ -> DbError.ItemNotFound(i) |> Error

let internal tryQuery ctx f = 
    try 
        f ctx
    with ex -> DbError.Exception(ex) |> Error