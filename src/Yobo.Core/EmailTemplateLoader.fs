module internal Yobo.Core.EmailTemplateLoader

open FSharp.Control.Tasks.V2
open System.Reflection
open System.IO

let loadTemplate name =
    let ass = Assembly.GetExecutingAssembly()
    use stream = ass.GetManifestResourceStream(sprintf "Yobo.Core.%s" name)
    use reader = new StreamReader(stream)
    reader.ReadToEnd()