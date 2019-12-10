module Yobo.Client.Domain

type Page =
    | Calendar
    // auth
    | Auth of Auth.Domain.Model

type Model = {
    CurrentPage : Page
}

module Model =
    let init = {
        CurrentPage = Calendar
    }

type Msg =
    // navigation
    | UrlChanged of Page
    // auth
    | AuthMsg of Auth.Domain.Msg