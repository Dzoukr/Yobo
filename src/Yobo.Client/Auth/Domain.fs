module Yobo.Client.Auth.Domain

type Model =
    | Login of Login.Domain.Model

type Msg =
    | LoginMsg of Login.Domain.Msg