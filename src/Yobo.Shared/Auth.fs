module Yobo.Shared.Auth

open System

type AuthError =
    | InvalidLoginOrPassword
    | AccountNotActivated of Guid
    | InvalidOrExpiredToken