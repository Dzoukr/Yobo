module Yobo.Client.StateHandlers

open Elmish
open Yobo.Shared.Domain
open Yobo.Shared.Validation
open Yobo.Client.SharedView

let handleValidated
    (onSuccess:'a -> 'model * Cmd<_>)
    (onErrorModel:'model)
    (onValidationError:'model -> ValidationError list -> 'model)
    (res:ServerResult<'a>) =
        match res with
        | Ok v -> v |> onSuccess
        | Error error ->
            let cmd = error |> ServerResponseViews.showErrorToast
            let model =
                match error with
                | Validation errs -> onValidationError onErrorModel errs
                | _ -> onErrorModel
            model, cmd
            
let handle onSuccess onErrorModel res = handleValidated onSuccess onErrorModel (fun x _ -> x) res  