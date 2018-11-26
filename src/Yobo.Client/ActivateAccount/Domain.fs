module Yobo.Client.ActivateAccount.Domain

open System
open Yobo.Shared.Communication

type State = {
    IsActivating : bool
    
}
with
    static member Init = {
        IsActivating = false
    }

type Msg =
    | Activate
    | ActivateDone of Result<Guid,ServerError>