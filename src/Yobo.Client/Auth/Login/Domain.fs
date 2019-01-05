module Yobo.Client.Auth.Login.Domain

open System
open Yobo.Shared.Auth.Domain
open Yobo.Shared.Communication

type State = {
    IsLogging : bool
    Login : Login
    LoginResult : Result<string, ServerError> option
    ResendActivationResult: Result<Guid, ServerError> option
}
with
    static member Init = {
        IsLogging = false
        Login = Login.Init
        LoginResult = None
        ResendActivationResult = None
    }

type Msg =
    | Login
    | LoggedIn of Result<string, ServerError>
    | ChangeEmail of string
    | ChangePassword of string
    | ResendActivation of Guid
    | ActivationResent of Result<Guid, ServerError>
