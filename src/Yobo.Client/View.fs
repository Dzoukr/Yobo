module Yobo.Client.View

open Yobo.Client.Domain
open Fulma
open Fable.Helpers.React
open Fable.Helpers.React.Props

let private displayLoggedPage (page:string) content dispatch =

    let item (pg:string) icon text =
        let isActive = page = pg
        Navbar.Item.a [ Navbar.Item.IsActive isActive ; Navbar.Item.Option.Props [ Href pg; OnClick Router.goToUrl ] ] [
            i [ ClassName icon; Style [ MarginRight 5] ] [ ]
            str text
        ]

    div [] [
        Navbar.navbar [ Navbar.Color IsLight; ] [
            Container.container [] [
                Navbar.Start.div [] [
                    item Router.Routes.calendar "fas fa-calendar-alt" "Kalendář"
                ]
                Navbar.End.div [] [
                    item Router.Routes.users "fas fa-users" "Uživatelé"
                    item Router.Routes.lessons "fas fa-calendar-alt" "Lekce"
                    // buttons
                    Navbar.Item.div [] [
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
        ]
    ]

let render (state : State) (dispatch : Msg -> unit) =
    let showInTemplate content =
        displayLoggedPage state.Route content dispatch
    match state.Page with
    | Auth pg ->
        match pg with
        | Login state -> Auth.Login.View.render state (LoginMsg >> AuthMsg >> dispatch)
        | Registration state -> Auth.Registration.View.render state (RegistrationMsg >> AuthMsg >> dispatch)
        | AccountActivation state -> Auth.AccountActivation.View.render state (AccountActivationMsg >> AuthMsg >> dispatch)
    | Admin pg ->
        let content =
            match pg with
            | Users state -> Admin.Users.View.render state (UsersMsg >> AdminMsg >> dispatch)
            | Lessons state -> Admin.Lessons.View.render state (LessonsMsg >> AdminMsg >> dispatch)
        content |> showInTemplate
    | Calendar state ->
        Calendar.View.render state (CalendarMsg >> dispatch) |> showInTemplate