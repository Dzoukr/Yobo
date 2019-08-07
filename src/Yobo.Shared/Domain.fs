module Yobo.Shared.Domain

open System

type DomainError =
    | ItemAlreadyExists of string
    | ItemDoesNotExist of string
    | UserAlreadyActivated
    | UserNotActivated
    | ActivationKeyDoesNotMatch
    | PasswordResetKeyDoesNotMatch
    | LessonIsFull
    | LessonIsAlreadyReserved
    | LessonIsNotReserved
    | LessonCancellingIsClosed
    | LessonIsClosed
    | NotEnoughCredits
    | CreditsExpiresBeforeLessonStart
    | CashReservationIsBlocked
    | LessonIsAlreadyCancelled
    | WorkshopIsAlreadyDeleted
    with
        member x.Explain() =
            match x with
            | ItemAlreadyExists v -> v |> sprintf "Tento %s již v systému existuje."
            | ItemDoesNotExist v -> v |> sprintf "Položka s touto hodnotou %s v systému neexistuje." 
            | UserAlreadyActivated -> "Uživatelský účet je již zaktivován." 
            | UserNotActivated -> "Uživatelský účet není zaktivován."
            | ActivationKeyDoesNotMatch -> "Aktivační klíč nebyl nalezen."
            | PasswordResetKeyDoesNotMatch -> "Klíč pro reset hesla nebyl nalezen."
            | LessonIsFull -> "Lekce je již plně obsazená."
            | LessonIsAlreadyReserved -> "Lekce je již zarezervována."
            | LessonIsNotReserved -> "Lekce není zarezervována."
            | LessonCancellingIsClosed -> "Odhlašování z lekce bylo již uzavřeno."
            | LessonIsClosed -> "Lekce je již uzavřena."
            | NotEnoughCredits -> "Nemáte dostatek kreditů."
            | CreditsExpiresBeforeLessonStart -> "Lekce začíná až po platnosti vašich kreditů."
            | CashReservationIsBlocked -> "Rezervaci v hotovosti nelze provést."
            | LessonIsAlreadyCancelled -> "Lekce je již zrušena."
            | WorkshopIsAlreadyDeleted -> "Workshop je již smazán."

type User = {
    Id : Guid
    Email : string
    FirstName : string
    LastName : string
    Activated : DateTimeOffset option
    Credits : int
    CreditsExpiration : DateTimeOffset option
    CashReservationBlockedUntil : DateTimeOffset option
    IsAdmin : bool
}

type Payment =
    | Cash
    | Credits

type UserReservation =
    | ForOne of Payment
    | ForTwo
with
    member x.ToIntAndBool =
        match x with
        | ForOne (Cash) -> 1, false
        | ForOne (Credits) -> 1, true
        | ForTwo -> 2, true

type Workshop = {
    Id : Guid
    StartDate : DateTimeOffset
    EndDate : DateTimeOffset
    Name : string
    Description : string
}