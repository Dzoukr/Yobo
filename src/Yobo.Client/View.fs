module Yobo.Client.View

open Yobo.Client.Domain
open Fulma
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Yobo.Shared.Domain

let private displayLoggedPage termsViewed (user:User option) (page:string) content dispatch =
    let item (pg:string) icon text =
        let isActive = page = pg
        Navbar.Item.a [ Navbar.Item.IsActive isActive ; Navbar.Item.Option.Props [ Href pg; OnClick Router.goToUrl ] ] [
            i [ ClassName icon; Style [ MarginRight 5] ] [ ]
            str text
        ]

    let adminButtons =
        match user with
        | Some { IsAdmin = true } ->
                [ item Router.Routes.users "fas fa-users" "Uživatelé"
                  item Router.Routes.lessons "fas fa-calendar-alt" "Lekce" ]
        | _ -> []

    let userInfo =
        match user with
        | Some user ->
            Navbar.Item.div [] [
                Tag.tag [ Tag.Color IsInfo; Tag.Props [ Style [ MarginRight 10 ] ] ] [ sprintf "%i kreditů" user.Credits |> str ]
                i [ ClassName "fas fa-user"; Style [ MarginRight 5 ] ] []
                sprintf "%s %s" user.FirstName user.LastName |> str
            ]
        | None -> str ""
    
    div [] [
        Navbar.navbar [ Navbar.Color IsLight; ] [
            Container.container [] [
                Navbar.Start.div [] [
                    item Router.Routes.calendar "fas fa-calendar-alt" "Kalendář"
                    item Router.Routes.mylessons "fas fa-calendar-alt" "Moje lekce"
                ]
                Navbar.End.div [] [
                    yield! adminButtons
                    yield userInfo
                    yield Navbar.Item.div [] [
                        div [ ClassName "buttons" ] [
                            Button.a [ Button.Color IsDanger; Button.Props [ OnClick (fun _ -> LoggedOut |> dispatch) ] ] [
                                str "Odhlásit"
                            ]
                        ]
                    ]
                ]
            ]
        ]
        main [ Style [ PaddingTop "2rem" ] ] [
            Container.container [ ] [
                content
            ]
            Container.container [ ] [
                a [ ClassName "terms-link"; OnClick (fun _ -> ToggleTermsView |> dispatch) ] [str "Obchodní podmínky"]
                SharedView.termsModal termsViewed (fun _ -> ToggleTermsView |> dispatch)
            ]
        ]
    ]

let render (state : State) (dispatch : Msg -> unit) =
    let showInTemplate content =
        displayLoggedPage state.TermsDisplayed state.LoggedUser state.Route content dispatch
    match state.Page with
    | Auth pg ->
        match pg with
        | Login state -> Auth.Login.View.render state (LoginMsg >> AuthMsg >> dispatch)
        | Registration state -> Auth.Registration.View.render state (RegistrationMsg >> AuthMsg >> dispatch)
        | AccountActivation state -> Auth.AccountActivation.View.render state (AccountActivationMsg >> AuthMsg >> dispatch)
        | ForgottenPassword state -> Auth.ForgottenPassword.View.render state (ForgottenPasswordMsg >> AuthMsg >> dispatch)
        | ResetPassword state -> Auth.ResetPassword.View.render state (ResetPasswordMsg >> AuthMsg >> dispatch)
    | Admin pg ->
        let content =
            match pg with
            | Users state -> Admin.Users.View.render state (UsersMsg >> AdminMsg >> dispatch)
            | Lessons state -> Admin.Lessons.View.render state (LessonsMsg >> AdminMsg >> dispatch)
        content |> showInTemplate
    | Calendar st ->
        let content =
            if state.LoggedUser.IsSome then
                Calendar.View.render state.LoggedUser.Value st (CalendarMsg >> dispatch)
            else str ""
        content |> showInTemplate
    | MyLessons st ->
        MyLessons.View.render st (MyLessonsMsg >> dispatch) |> showInTemplate