module Yobo.Core.EmailSettings

open System
open Yobo.Libraries.Emails

type Settings = {
    From : Address
    BaseUrl : Uri
}
