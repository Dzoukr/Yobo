module Yobo.Core.Data

open System

type DbError =
    | ItemNotFoundById of Guid
    | ItemNotFoundByEmail of string
    | Exception of exn

let private oneOrError<'a> (s:seq<'a>) error =
    try
        s |> Seq.head |> Ok
    with _ -> error |> Error

let internal oneOrErrorById<'a> i (s:seq<'a>) = i |> ItemNotFoundById |> oneOrError s
let internal oneOrErrorByEmail<'a> e (s:seq<'a>) = e |> ItemNotFoundByEmail |> oneOrError s

let internal tryQueryResult ctx f =
    try
        f ctx
    with ex -> ex |> DbError.Exception |> Error

let internal tryQueryResultM errorMapFn ctx f =
    try
        f ctx
    with ex -> ex |> errorMapFn |> Error

let internal tryQuery ctx f = (f >> Ok) |> tryQueryResult ctx    