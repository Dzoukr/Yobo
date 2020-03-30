module Yobo.Libraries.Tuples

let fstOf3 (v,_,_) = v 
let sndOf3 (_,v,_) = v 
let thrOf3 (_,_,v) = v
let ignoreFstOf3 (_,a,b) = a,b
let optionOf2 = function
    | Some x, Some y -> Some (x, y)
    | _ -> None
