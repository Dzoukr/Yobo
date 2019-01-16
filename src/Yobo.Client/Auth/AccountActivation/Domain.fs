module Yobo.Client.Auth.AccountActivation.Domain

open System
open Yobo.Shared.Communication

type State = {
    Id : Guid
    ActivationResult: Result<Guid,ServerError> option
}
with
    static member Init i = {
        Id = i
        ActivationResult = None
    }

type Msg =
    | Activate of Guid
    | ActivateDone of Result<Guid,ServerError>