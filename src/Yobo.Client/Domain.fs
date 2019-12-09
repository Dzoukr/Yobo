module Yobo.Client.Domain

type Model = {
    CurrentPage : Router.Page
    ShowQuickView : bool
}

module Model =
    let init = {
        CurrentPage = Router.defaultPage
        ShowQuickView = false
    }

type Msg =
    | UrlChanged of Router.Page
    | ToggleQuickView