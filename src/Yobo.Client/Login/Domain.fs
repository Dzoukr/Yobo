module Yobo.Client.Login.Domain

open Yobo.Shared.Login.Domain
open Yobo.Shared.Domain
open Yobo.Shared.Communication

type State = {
    IsLogging : bool
    Login : Login
    LoginResult : Result<User, ServerError> option
}
with
    static member Init = {
        IsLogging = false
        Login = Login.Init
        LoginResult = None
    }

type Msg =
    | Login
    | LoginDone of Result<User, ServerError>
    | ChangeEmail of string
    | ChangePassword of string
