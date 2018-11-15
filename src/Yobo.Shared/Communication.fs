module Yobo.Shared.Communication

open System
open Yobo.Shared.Validation

type ServerError =
    | ValidationError of ValidationError
    | Exception of Exception