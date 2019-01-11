module Yobo.Client.View

open Yobo.Client.Domain
open Fulma
open Fable.Helpers.React
open Fable.Helpers.React.Props

let displayLoggedPage (page:Router.Page) content =

    let item (pg:Router.Page) icon text =
        let isActive = page = pg
        Navbar.Item.a [ Navbar.Item.IsActive isActive ; Navbar.Item.Option.Props [ Href <| pg.ToPath(); OnClick Router.goToUrl ] ] [
            i [ ClassName icon; Style [ MarginRight 5] ] [ ]
            str text
        ]

    div [] [
        Navbar.navbar [ Navbar.Color IsLight; ] [
            Container.container [] [
                Navbar.End.div [] [
                    item (Router.Page.Admin(Router.AdminPage.Users)) "fas fa-users" "Uživatelé"
                    item (Router.Page.Admin(Router.AdminPage.Lessons)) "fas fa-calendar-alt" "Lekce"
                    // buttons
                    Navbar.Item.div [] [
                        div [ ClassName "buttons" ] [
                            Button.a [ Button.Color IsDanger; Button.Props [ Href <| Router.Page.Auth(Router.Logout).ToPath(); OnClick Router.goToUrl ] ] [
                                str "Odhlásit"
                            ]
                        ]
                    ]
                ]
            ]
        ]
        Container.container [ ] [
            content
        ]
    ]

let render (state : State) (dispatch : Msg -> unit) =
    match state.Page with
    | Router.Page.Auth auth ->
        match auth with
        | Router.AuthPage.Login -> Auth.Login.View.render state.Auth.Login (LoginMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.Registration -> Auth.Registration.View.render state.Auth.Registration (RegistrationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.ForgottenPassword -> Auth.Registration.View.render state.Auth.Registration (RegistrationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.AccountActivation _ -> Auth.AccountActivation.View.render state.Auth.AccountActivation (AccountActivationMsg >> AuthMsg >> dispatch)
        | Router.AuthPage.Logout -> str ""
    | Router.Page.Admin admin ->
        let content =
            match admin with
            | Router.AdminPage.Users -> Admin.Users.View.render state.Admin.Users (UsersMsg >> AdminMsg >> dispatch)
            | Router.AdminPage.Lessons -> str "LEKCE" //Admin.View.render state.Admin (AdminMsg >> dispatch)
        
        displayLoggedPage state.Page content
