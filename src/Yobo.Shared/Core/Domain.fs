module Yobo.Shared.Core.Domain

open System

let calculateCredits amount expiration =
    match expiration with
    | None -> amount
    | Some exp ->
        if DateTimeOffset.UtcNow > exp then 0 else amount
        
let canLessonBeCancelled (isCanceled:bool) (lessonStart:DateTimeOffset) =
    (not isCanceled) && lessonStart > DateTimeOffset.Now
    
let canLessonBeDeleted (lessonStart:DateTimeOffset) =
    lessonStart > DateTimeOffset.Now
    
let canOnlineLessonBeCancelled = canLessonBeCancelled        
let canOnlineLessonBeDeleted = canLessonBeDeleted

module Queries =
    type LessonPayment =
        | Cash
        | Credits