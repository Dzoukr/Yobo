module Yobo.Client.Admin.Domain

open System
open Yobo.Shared.Auth.Domain
open Yobo.Shared.Communication
open Yobo.Shared.Domain
open Yobo.Shared.Admin.Domain

type State = {
    Users : User list
    AddCreditsOpenedForm : Guid option
}
with
    static member Init = {
        Users = []
        AddCreditsOpenedForm = None
    }

type Msg =
    | Init
    | ToggleAddCreditsForm of Guid
    | AddCredit
    | LoadUsers
    | UsersLoaded of Result<User list, ServerError>
    | CalendarChanged of DateTime option