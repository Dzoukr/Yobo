module Yobo.Client.Pages.AccountActivation.Domain

open System
open Yobo.Shared.Domain

type Model = {
    ActivationId : Guid
    ActivationResult: ServerResult<unit> option
}

module Model =
    let init id  = {
        ActivationId = id
        ActivationResult = None
    }

type Msg =
    | ErrorOccured of ServerError
    | Activate
    | Activated