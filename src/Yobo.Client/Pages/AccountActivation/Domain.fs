module Yobo.Client.Pages.AccountActivation.Domain

open System
open Yobo.Shared.Errors

type Model = {
    ActivationId : Guid
    ActivationResult: ServerResult<unit> option
}

type Msg =
    | Activate
    | Activated of ServerResult<unit>