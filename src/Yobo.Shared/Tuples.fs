module Yobo.Shared.Tuples

let fstOf3 (v,_,_) = v 
let sndOf3 (_,v,_) = v 
let thrOf3 (_,_,v) = v
let ignoreFstOf3 (_,a,b) = a,b
let optionOf2 = function
    | Some x, Some y -> Some (x, y)
    | _ -> None

let mapFst fn (a,b) = (fn a), b
let mapSnd fn (a,b) = a, (fn b)