module Yobo.Core.Users.CommandHandler

open Yobo.Core.EventStoreCommandHandler
open Yobo.Core.Users
open Yobo.Libraries.Security.SymetricCryptoProvider

let private getId = function
    | Register args -> args |> Extractor.getIdFromCommand
    | RegenerateActivationKey args -> args |> Extractor.getIdFromCommand
    | Activate args -> args |> Extractor.getIdFromCommand
    | AddCredits args -> args |> Extractor.getIdFromCommand

let private settings cryptoProvider = {
    Aggregate = {
        Init = State.Init
        Execute = Aggregate.execute
        Apply = Aggregate.apply
    }
    GetStreamId = getId >> sprintf "Users-%s"
    Serializer = {
        EventToData = EventSerializer.toData cryptoProvider
        DataToEvent = EventSerializer.toEvent cryptoProvider
    }
    Validators = [ ]
    RollbackEvents = fun _ -> []
}

let private cmdBuilder = function
    | Register args -> Registry.Add { UserId = args.Id; Email = args.Email } |> Some
    | _ -> None

let get (cryptoProvider:SymetricCryptoProvider) store =
    let registryHandler = store |> Registry.CommandHandler.get
    store |> getRollbackCommandHandler (settings cryptoProvider) registryHandler cmdBuilder
   