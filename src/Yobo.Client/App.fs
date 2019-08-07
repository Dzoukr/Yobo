module Yobo.Client.App

open Elmish
open Elmish.React
open Elmish.Navigation
open Thoth.Elmish
open Elmish.UrlParser

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update View.render
|> Program.withSubscription State.subscribe
|> Program.toNavigable (parsePath State.pageParser) State.urlUpdate
|> Toast.Program.withToast Toast.render
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run