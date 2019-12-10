module Yobo.Client.Auth.View

open Domain

let view (model:Model) (dispatch : Msg -> unit) =
    match model with
    | Login m -> Login.View.view m (LoginMsg >> dispatch)