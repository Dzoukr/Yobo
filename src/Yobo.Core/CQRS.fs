module Yobo.Core.CQRS

open FSharp.Rop

type CoreCommand =
    | UsersRegistry of Users.Registry.Command
    | Users of Users.Command
    | Lessons of Lessons.Command

type CoreEvent = 
    | UsersRegistry of Users.Registry.Event
    | Users of Users.Event
    | Lessons of Lessons.Event

type Saga =
    | Direct
    | Before of CoreCommand
    | After of CoreCommand
    | BatchBefore of CoreCommand list
    | BatchAfter of CoreCommand list
    
type SagaSetup<'err> = {
    GetCommandSaga : CoreCommand -> Result<Saga, 'err>
    Handle : CoreCommand -> Result<CoreEvent list, 'err>
    Compensate : CoreEvent -> unit
}

let handleWithSaga (setup:SagaSetup<_>) =
    
    let withRollback (fn1, arg1) (fn2, arg2) =
        arg1 |> fn1
        >>= (fun events1 ->
            match arg2 |> fn2 with
            | Ok events2 -> Ok (events1 @ events2)
            | Error err -> 
                events1 |> List.iter setup.Compensate
                Error err
        )
    
    let rec run (cmd:CoreCommand) =
        let foldFn acc item =
            match acc with
            | Ok evns ->
                match run item with
                | Ok newEvns -> evns @ newEvns |> Ok
                | Error err ->
                    evns |> List.iter setup.Compensate
                    Error err
            | Error err -> Error err

        cmd |> setup.GetCommandSaga
        >>= (fun saga -> 
            match saga with
            | Direct -> cmd |> setup.Handle
            | Before bef -> withRollback (run, bef) (setup.Handle, cmd)
            | After aft -> withRollback (setup.Handle, cmd) (run, aft)
            | BatchBefore cmds -> withRollback (List.fold foldFn (Ok []), cmds) (setup.Handle, cmd)
            | BatchAfter cmds -> withRollback (setup.Handle, cmd) (List.fold foldFn (Ok []), cmds) 
        )
    run