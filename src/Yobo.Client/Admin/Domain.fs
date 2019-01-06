module Yobo.Client.Admin.Domain

open System
open Yobo.Shared.Auth.Domain
open Yobo.Shared.Communication
open Yobo.Shared.Domain

type State = {
    Users : User list
}
with
    static member Init = {
        Users = []
    }

type Msg =
    | Init
    | LoadUsers
    | UsersLoaded of Result<User list, ServerError>