module Yobo.Shared.Domain

type DomainError =
    | ItemAlreadyExists of Text.TextValue
    | ItemDoesNotExist of Text.TextValue
    | UserAlreadyActivated
    | ActivationKeyDoesNotMatch