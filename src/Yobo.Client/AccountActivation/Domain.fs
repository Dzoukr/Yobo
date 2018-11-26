module Yobo.Client.AccountActivation.Domain

open System
open Yobo.Shared.Communication

type State = {
    IsActivating : bool
    ActivationResult: Result<Guid,ServerError> option
}
with
    static member Init = {
        IsActivating = false
        ActivationResult = None
    }

type Msg =
    | Activate of Guid
    | ActivateDone of Result<Guid,ServerError>