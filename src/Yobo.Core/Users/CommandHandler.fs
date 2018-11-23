module Yobo.Core.Users.CommandHandler

open Yobo.Core.EventStoreCommandHandler
open Yobo.Core.Users
open Yobo.Libraries.Security.SymetricCryptoProvider

let private getId = function
    | Register args -> args |> Extractor.getIdFromCommand

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
}

let private compensationBuilder = function
    | Register args -> 
        let cmd = Registry.Add { UserId = args.Id; Email = args.Email }
        let events = Registry.Removed { UserId = args.Id; Email = args.Email } |> List.singleton
        cmd, events

let get (cryptoProvider:SymetricCryptoProvider) store =
    let userCmdHandler = store |> getCommandHandler (settings cryptoProvider)
    store |> Registry.CommandHandler.get compensationBuilder userCmdHandler
    
    

