module Yobo.API.ArgsBuilder

open Yobo.Shared.Validation
open FSharp.Rop

let build<'src,'trgt> (mapper:'src -> 'trgt) (validator:'src -> Result<'src,ValidationError list>) src =
    src
    |> validator
    <!> mapper