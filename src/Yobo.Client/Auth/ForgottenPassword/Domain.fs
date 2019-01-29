module Yobo.Client.Auth.ForgottenPassword.Domain

open Yobo.Shared.Communication

type State = {
    EmailToReset : string
    ResetResult : ServerResult<unit> option
}
with
    static member Init = {
        EmailToReset = ""
        ResetResult = None
    }

type Msg =
    | EmailChanged of string
    | InitiateReset
    | ResetInitiated of ServerResult<unit>