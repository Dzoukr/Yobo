open Fake.IO

#r "paket: groupref Build //"
#load ".fake/build.fsx/intellisense.fsx"

open System.IO
open Fake.Core
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators

module Tools =
    let private findTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match ProcessUtils.tryFindFileOnPath tool with
        | Some t -> t
        | _ ->
            let errorMsg =
                tool + " was not found in path. " +
                "Please install it and make sure it's available from your path. "
            failwith errorMsg
            
    let private runTool (cmd:string) args workingDir =
        let arguments = args |> String.split ' ' |> Arguments.OfArgs
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore
        
    let dotnet cmd workingDir =
        let result =
            DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
        if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir
    
    let femto = runTool "femto"        
    let node = runTool (findTool "node" "node.exe")        
    let yarn = runTool (findTool "yarn" "yarn.cmd")             

let clientSrcPath = "src" </> "Yobo.Client"
let clientDeployPath = "deploy" </> "client"

// Targets
let clean proj = [ proj </> "bin"; proj </> "obj" ] |> Shell.cleanDirs


Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    Tools.node "--version" clientSrcPath
    printfn "Yarn version:"
    Tools.yarn "--version" clientSrcPath
    Tools.yarn "install --frozen-lockfile" clientSrcPath
)

Target.create "PublishClient" (fun _ ->
    let clientDeployLocalPath = (clientSrcPath </> "deploy")
    [ clientSrcPath; clientDeployLocalPath] |> Shell.cleanDirs
    Tools.yarn "webpack-cli -p" clientSrcPath
    Shell.copyDir clientDeployPath clientDeployLocalPath FileFilter.allFiles
)

Target.create "Run" (fun _ -> Tools.yarn "webpack-dev-server" clientSrcPath)

"InstallClient" ==> "Run"
"InstallClient" ==> "PublishClient"

Target.runOrDefaultWithArguments "Run"