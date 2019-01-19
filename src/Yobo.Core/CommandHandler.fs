module Yobo.Core.CommandHandler

open CosmoStore
open FSharp.Rop
open Yobo.Libraries.Security.SymetricCryptoProvider

let getHandleFn (cryptoProvider:SymetricCryptoProvider) (eventStore:EventStore) =
    let usersHandler = Users.CommandHandler.get cryptoProvider eventStore
    let lessonsHandler = Lessons.CommandHandler.get usersHandler eventStore

    let handle meta corrId cmd =
        match cmd with
        | CoreCommand.Users c -> c |> usersHandler.HandleCommand meta corrId <!> List.map CoreEvent.Users
        | CoreCommand.Lessons c -> c |> lessonsHandler.HandleCommand meta corrId <!> List.map CoreEvent.Lessons
    handle
