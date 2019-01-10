module Yobo.Core.CommandHandler

open CosmoStore
open FSharp.Rop
open Yobo.Libraries.Security.SymetricCryptoProvider

let getHandler (cryptoProvider:SymetricCryptoProvider) (eventStore:EventStore) =
    let usersHandler = Users.CommandHandler.get cryptoProvider eventStore

    let handle meta corrId cmd =
        match cmd with
        | CoreCommand.Users c -> c |> usersHandler meta corrId <!> List.map CoreEvent.Users
    handle
