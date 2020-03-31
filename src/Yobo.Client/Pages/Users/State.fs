module Yobo.Client.Pages.Users.State

open Domain
open Elmish
open Yobo.Client.Server
open Yobo.Client.Forms
open Yobo.Client.SharedView
open Yobo.Shared.Core.Admin.Validation

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    match msg with
    | LoadUsers -> { model with UsersLoading = true }, Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.GetAllUsers)) () UsersLoaded
    | UsersLoaded res ->
        let model = { model with UsersLoading = false }
        match res with
        | Ok users -> { model with Users = users }, Cmd.none
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | ShowAddCreditsForm userId ->
        let model = { model with AddCreditsSelectedUser = userId }
        match userId with
        | Some i -> 
            { model with
                AddCreditsForm =
                    model.AddCreditsForm
                    |> ValidatedForm.updateWithFn (fun x -> { x with UserId = i }) }, Cmd.none
        | None -> model, Cmd.none            
    | AddCreditsFormChanged f ->
        { model with AddCreditsForm = model.AddCreditsForm |> ValidatedForm.updateWith f |> ValidatedForm.validateWith validateAddCredits }, Cmd.none
    | AddCredits ->
        let model = { model with AddCreditsForm = model.AddCreditsForm |> ValidatedForm.validateWith validateAddCredits }
        if model.AddCreditsForm |> ValidatedForm.isValid then
            { model with AddCreditsForm = model.AddCreditsForm |> ValidatedForm.startLoading },
                Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.AddCredits)) model.AddCreditsForm.FormData CreditsAdded
        else model, Cmd.none
    | CreditsAdded res ->
        let model = { model with AddCreditsForm = model.AddCreditsForm |> ValidatedForm.stopLoading }
        match res with
        | Ok _ -> model, Cmd.batch [ ServerResponseViews.showSuccessToast "Kredity úspěšně přidány."; Cmd.ofMsg <| ShowAddCreditsForm(None); Cmd.ofMsg LoadUsers ]
        | Error e -> model, e |> ServerResponseViews.showErrorToast
    | ShowSetExpirationForm userId ->
        let model = { model with SetExpirationSelectedUser = userId }
        match userId with
        | Some i -> 
            { model with
                SetExpirationForm =
                    model.SetExpirationForm
                    |> ValidatedForm.updateWithFn (fun x -> { x with UserId = i }) }, Cmd.none
        | None -> model, Cmd.none
    | SetExpirationFormChanged f ->
        { model with SetExpirationForm = model.SetExpirationForm |> ValidatedForm.updateWith f |> ValidatedForm.validateWith validateSetExpiration }, Cmd.none
    | SetExpiration ->
        let model = { model with SetExpirationForm = model.SetExpirationForm |> ValidatedForm.validateWith validateSetExpiration }
        if model.SetExpirationForm |> ValidatedForm.isValid then
            { model with SetExpirationForm = model.SetExpirationForm |> ValidatedForm.startLoading },
                Cmd.OfAsync.eitherAsResult (onAdminService (fun x -> x.SetExpiration)) model.SetExpirationForm.FormData ExpirationSet
        else model, Cmd.none
    | ExpirationSet res ->
        let model = { model with SetExpirationForm = model.SetExpirationForm |> ValidatedForm.stopLoading }
        match res with
        | Ok _ -> model, Cmd.batch [ ServerResponseViews.showSuccessToast "Platnost úspěšně změněna."; Cmd.ofMsg <| ShowSetExpirationForm(None); Cmd.ofMsg LoadUsers ]
        | Error e -> model, e |> ServerResponseViews.showErrorToast