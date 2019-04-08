module Yobo.Core.CommandHandler

open CosmoStore
open FSharp.Rop
open Yobo.Libraries.Security.SymetricCryptoProvider
open Yobo.Core.EventStoreCommandHandler
open Yobo.Core.Lessons.CmdArgs
open Yobo.Core.CQRS

module Users =
    let getSaga cmd =
        match cmd with
        | Users.Register args ->
            Users.Registry.Add { UserId = args.Id; Email = args.Email } 
            |> CoreCommand.UsersRegistry
            |> Before
        | _ -> Direct

module Lessons =
    open Yobo.Core.Lessons

    let getSaga lessonsHandler cmd =
        match cmd with
        | Lessons.AddReservation args ->
            let uc =
                match args with
                | { UseCredits = true } -> Users.WithdrawCredits { Id = args.UserId; Amount = args.Count; LessonId = args.Id } |> Ok
                | { UseCredits = false} ->
                    cmd
                    |> lessonsHandler.GetState
                    <!> (fun state -> Users.BlockCashReservations { Id = args.UserId; Expires = state.EndDate; LessonId = args.Id })
            uc 
            <!> (CoreCommand.Users >> Before)
            
        | Lessons.CancelReservation args ->
            result {
                let! state = cmd |> lessonsHandler.GetState
                return state.Reservations 
                |> List.tryFind (fun (x,_,_) -> x = args.UserId)
                |> Option.map (fun (u,c,useCredits) -> 
                    if useCredits then Users.RefundCredits { Id = u; Amount = c; LessonId = args.Id }
                    else Users.UnblockCashReservations { Id = u }
                )
            }
            |> Result.liftOption (Yobo.Shared.Domain.DomainError.LessonIsNotReserved |> DomainError)
            <!> (CoreCommand.Users >> After)
            
        | Lessons.Cancel args ->
            let toCmd (u,c,useCredits) =
                if useCredits then Users.RefundCredits { Id = u; Amount = c; LessonId = args.Id }
                else Users.UnblockCashReservations { Id = u }
            cmd |> lessonsHandler.GetState
            <!> (fun x -> x.Reservations)
            <!> List.map (toCmd >> CoreCommand.Users)
            <!> BatchAfter
        | _ -> Ok Direct
        
let getSagaSetup (cryptoProvider:SymetricCryptoProvider) (eventStore:EventStore) = 
    let userRegistryHandler = Users.Registry.CommandHandler.get eventStore
    let usersHandler = Users.CommandHandler.get cryptoProvider eventStore
    let lessonsHandler = Lessons.CommandHandler.get eventStore
    let workshopsHandler = Workshops.CommandHandler.get eventStore

    let handle meta corrId cmd =
        match cmd with
        | CoreCommand.Users c -> c |> usersHandler.HandleCommand meta corrId <!> List.map CoreEvent.Users
        | CoreCommand.Lessons c -> c |> lessonsHandler.HandleCommand meta corrId <!> List.map CoreEvent.Lessons
        | CoreCommand.Workshops c -> c |> workshopsHandler.HandleCommand meta corrId <!> List.map CoreEvent.Workshops
        | CoreCommand.UsersRegistry c -> c |> userRegistryHandler.HandleCommand meta corrId <!> List.map CoreEvent.UsersRegistry

    let compensate meta corrId evn =
        (match evn with
        | CoreEvent.Users c -> c |> usersHandler.CompensateEvent meta corrId
        | CoreEvent.Lessons c -> c |> lessonsHandler.CompensateEvent meta corrId
        | CoreEvent.Workshops c -> c |> workshopsHandler.CompensateEvent meta corrId
        | CoreEvent.UsersRegistry c -> c |> userRegistryHandler.CompensateEvent meta corrId
        ) |> ignore

    let getCommandSaga = function
        | CoreCommand.Users c -> c |> Users.getSaga |> Ok
        | CoreCommand.Lessons c -> c |> Lessons.getSaga lessonsHandler
        | _ -> Ok Direct

    handle, compensate, getCommandSaga

let getHandler (cryptoProvider:SymetricCryptoProvider) (eventStore:EventStore) =
    let handleCommand, compensate, getCommandSaga = getSagaSetup cryptoProvider eventStore

    let handle meta corrId cmd =
        let h = handleCommand meta corrId
        let c = compensate meta corrId
        cmd |> handleWithSaga { Handle = h; Compensate = c; GetCommandSaga = getCommandSaga }

    handle