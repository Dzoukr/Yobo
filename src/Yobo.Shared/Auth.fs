namespace Yobo.Shared.Auth

open System

type AuthError =
    | InvalidLoginOrPassword
    | InvalidLogin
    | AccountNotActivated of Guid
    | InvalidOrExpiredToken
    | ActivationKeyDoesNotMatch
    with
        member x.Explain() =
            match x with
            | InvalidLoginOrPassword -> "Zadali jste nesprávný email nebo heslo."
            | InvalidLogin -> "Zadali jste nesprávný email."
            | AccountNotActivated _ -> "Váš účet ještě nebyl zaktivován."
            | InvalidOrExpiredToken -> "Token je nevalidní, nebo již vypršela jeho platnost"
            | ActivationKeyDoesNotMatch -> "Aktivační klíč nebyl nalezen."