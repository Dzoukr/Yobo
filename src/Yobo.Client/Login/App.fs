module Yobo.Client.Login.App

open Elmish
open Elmish.React
open Yobo.Client.Login

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update View.render

#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run