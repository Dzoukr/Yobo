module Yobo.Core.Data

type DbError =
    | ItemNotFound of obj
    | Exception of exn

let internal oneOrError<'a> i (s:seq<'a>) =
    try
        s |> Seq.head |> Ok
    with _ -> DbError.ItemNotFound(i) |> Error

let internal tryQueryResult ctx f =
    try
        f ctx
    with ex -> ex |> DbError.Exception |> Error

let internal tryQueryResultM errorMapFn ctx f =
    try
        f ctx
    with ex -> ex |> errorMapFn |> Error

let internal tryQuery ctx f = (f >> Ok) |> tryQueryResult ctx    