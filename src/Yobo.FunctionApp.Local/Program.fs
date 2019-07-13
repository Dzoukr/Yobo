// Learn more about F# at http://fsharp.org

open System
open System.Diagnostics
open System.IO

[<EntryPoint>]
let main argv =
    let d = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".Local",""))
    let p = ProcessStartInfo()
    p.WorkingDirectory <- d
    p.FileName <- "cmd.exe"
    p.Arguments <- "/K func host start"
    Process.Start(p).WaitForExit()
    0