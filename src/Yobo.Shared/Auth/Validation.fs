module Yobo.Shared.Auth.Validation

open Yobo.Shared.Validation
open Yobo.Shared.Auth.Communication

let validateLogin (l:Request.Login) =
    [
        nameof(l.Email), validateEmail l.Email
        nameof(l.Password), validateNotEmpty l.Password
    ] |> validate
    
let validateRegister (r:Request.Register) =
    [
        nameof(r.FirstName), validateNotEmpty r.FirstName
        nameof(r.LastName), validateNotEmpty r.LastName
        nameof(r.Email), validateEmail r.Email
        nameof(r.Password), validateMinimumLength 6 r.Password
        nameof(r.SecondPassword), validateMinimumLength 6 r.SecondPassword
        nameof(r.SecondPassword), validatePasswordsMatch r.Password r.SecondPassword
        nameof(r.AgreeButtonChecked), validateTermsAgreed r.AgreeButtonChecked
    ] |> validate
    
let validateForgottenPassword (l:Request.ForgottenPassword) =
    [
        nameof(l.Email), validateEmail l.Email
    ] |> validate    

let validateResetPassword (r:Request.ResetPassword) =
    [
        nameof(r.Password), validateMinimumLength 6 r.Password
        nameof(r.SecondPassword), validateMinimumLength 6 r.SecondPassword
        nameof(r.SecondPassword), validatePasswordsMatch r.Password r.SecondPassword
    ] |> validate