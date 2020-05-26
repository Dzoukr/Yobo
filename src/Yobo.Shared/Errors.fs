module Yobo.Shared.Errors

open System
open Yobo.Shared.Validation

type AuthenticationError =
    | InvalidLoginOrPassword
    | InvalidOrExpiredToken
    | EmailAlreadyRegistered
    | AccountAlreadyActivatedOrNotFound
    | InvalidPasswordResetKey
    
module AuthenticationError =
    let explain = function
        | InvalidLoginOrPassword -> "Nesprávně vyplněný email nebo heslo."
        | InvalidOrExpiredToken -> "Token není validní nebo již vypršela jeho platnost."
        | EmailAlreadyRegistered -> "Tento email je již v systému registrován."
        | AccountAlreadyActivatedOrNotFound -> "Tento účet je již zaktivován nebo byl zadán neplatný aktivační klíč."
        | InvalidPasswordResetKey -> "Kód pro nastavení nového hesla je nesprávný, nebo byl již použit."

type DomainError =
    | UserNotActivated
    | LessonCannotBeCancelled
    | LessonCannotBeDeleted
    | LessonAlreadyFull
    | LessonAlreadyReserved
    | LessonCannotBeReserved
    | NotEnoughCredits
    | CreditsExpiresBeforeLessonStart
    | CashReservationIsBlocked

module DomainError =
    let explain = function
        | UserNotActivated -> "Uživatel ještě nebyl aktivován."
        | LessonCannotBeCancelled -> "Lekci nelze zrušit."
        | LessonCannotBeDeleted -> "Lekci nelze smazat."
        | LessonAlreadyFull -> "Lekci je již plně obsazena."
        | LessonAlreadyReserved -> "Lekci je již zarezervována."
        | LessonCannotBeReserved -> "Lekci nelze zarezervovat."
        | NotEnoughCredits -> "Nemáte dostatek kreditů."
        | CreditsExpiresBeforeLessonStart -> "Kredity vyprší před začátkem lekce."
        | CashReservationIsBlocked -> "Rezervace za hotové je zablokována."

type ServerError =
    | Exception of string
    | Validation of ValidationError list
    | Authentication of AuthenticationError
    | DatabaseItemNotFound of Guid
    | Domain of DomainError

type ServerResult<'a> = Result<'a, ServerError>

exception ServerException of ServerError

module ServerError =
    let explain = function
        | Exception e -> e
        | Validation errs ->
            errs
            |> List.map ValidationError.explain
            |> String.concat ", "
        | Authentication e -> e |> AuthenticationError.explain
        | DatabaseItemNotFound i -> sprintf "Položka s ID %A nebyla nalezena v databázi." i
        | Domain e -> e |> DomainError.explain
        
    let failwith (er:ServerError) = raise (ServerException er)
    
    let ofOption err (v:Option<_>) =
        v
        |> Option.defaultWith (fun _ -> err |> failwith)
    
    let ofResult<'a> (v:Result<'a,ServerError>) =
        match v with
        | Ok v -> v
        | Error e -> e |> failwith

    let validate (validationFn:'a -> ValidationError list) (value:'a) =
        match value |> validationFn with
        | [] -> value
        | errs -> errs |> Validation |> failwith