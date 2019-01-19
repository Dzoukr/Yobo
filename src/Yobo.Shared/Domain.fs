module Yobo.Shared.Domain

type DomainError =
    | ItemAlreadyExists of string
    | ItemDoesNotExist of string
    | UserAlreadyActivated
    | UserNotActivated
    | ActivationKeyDoesNotMatch
    | LessonIsFull
    | LessonIsAlreadyReserved
    with
        member x.Explain() =
            match x with
            | ItemAlreadyExists v -> v |> sprintf "Tento %s již v systému existuje."
            | ItemDoesNotExist v -> v |> sprintf "Položka s touto hodnotou %s v systému neexistuje." 
            | UserAlreadyActivated -> "Uživatelský účet je již zaktivován." 
            | UserNotActivated -> "Uživatelský účet není zaktivován."
            | ActivationKeyDoesNotMatch -> "Aktivační klíč nebyl nalezen."
            | LessonIsFull -> "Lekce je již plně obsazená."
            | LessonIsAlreadyReserved -> "Lekce je již zarezervována."