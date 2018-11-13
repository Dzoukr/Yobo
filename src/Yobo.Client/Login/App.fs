module Yobo.Client.Login.App

open Elmish
open Elmish.React
open Yobo.Client.Login
open Elmish.Browser.UrlParser
open Elmish.Browser.Navigation

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram State.init State.update View.render
|> Program.toNavigable (parseHash Router.pageParser) State.urlUpdate
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run