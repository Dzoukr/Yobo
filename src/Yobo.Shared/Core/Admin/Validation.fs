module Yobo.Shared.Core.Admin.Validation

open System
open Yobo.Shared.Validation
open Communication

let validateAddCredits (l:Request.AddCredits) =
    [
        nameof(l.UserId), validateNotEmptyGuid l.UserId
        nameof(l.Credits), validateMinimumValue 1 l.Credits
        nameof(l.Expiration), validateMinimumDate DateTimeOffset.UtcNow l.Expiration
    ] |> validate
    
let validateSetExpiration (l:Request.SetExpiration) =
    [
        nameof(l.UserId), validateNotEmptyGuid l.UserId
        nameof(l.Expiration), validateMinimumDate DateTimeOffset.UtcNow l.Expiration
    ] |> validate
    

let validateCreateLessons (r:Request.CreateLessons) =
    [
        nameof(r.Dates), validateNotEmptyList r.Dates
        nameof(r.Name), validateNotEmpty r.Name
        nameof(r.Description), validateNotEmpty r.Description
        nameof(r.Capacity), validateMinimumValue 1 r.Capacity
    ] |> validate

let validateCreateWorkshops (r:Request.CreateWorkshops) =
    [
        nameof(r.Dates), validateNotEmptyList r.Dates
        nameof(r.Name), validateNotEmpty r.Name
        nameof(r.Description), validateNotEmpty r.Description
    ] |> validate

let validateCreateOnlineLessons (r:Request.CreateOnlineLessons) =
    [
        nameof(r.Dates), validateNotEmptyList r.Dates
        nameof(r.Name), validateNotEmpty r.Name
        nameof(r.Description), validateNotEmpty r.Description
        nameof(r.Capacity), validateMinimumValue 1 r.Capacity
    ] |> validate
    
let validateChangeLessonDescription (r:Request.ChangeLessonDescription) =
    [
        nameof(r.Id), validateNotEmptyGuid r.Id
        nameof(r.Name), validateNotEmpty r.Name
        nameof(r.Description), validateNotEmpty r.Description
    ] |> validate