module Yobo.Client.State

open Domain
open Elmish
open Feliz.Router

let init () = Model.init, Cmd.none

let private upTo model toState toMsg (m,cmd) =
    { model with CurrentPage = toState(m) }, Cmd.map(toMsg) cmd

let update (msg:Msg) (model:Model) : Model * Cmd<Msg> =
    match model.CurrentPage, msg with
    | _, UrlChanged p -> { model with CurrentPage = p }, Cmd.none
    // auth
    | Login m, LoginMsg subMsg -> Pages.Login.State.update subMsg m |> upTo model Login LoginMsg 
    | Registration m, RegistrationMsg subMsg -> Pages.Registration.State.update subMsg m |> upTo model Registration RegistrationMsg 
        
    
