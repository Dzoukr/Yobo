module Yobo.Client.Forms

open Yobo.Shared.Validation

type ValidatedForm<'a> = {
    ValidationErrors : ValidationError list
    FormData : 'a
    IsLoading : bool
    WasSent : bool
}

module ValidatedForm =
    let updateWith f form = { form with FormData = f }
    let updateWithFn f form = { form with FormData = form.FormData |> f }
    let updateWithErrors errs form = { form with ValidationErrors = errs }
    let validateWith validFn form = { form with ValidationErrors = validFn form.FormData }
    let validateWithIfSent validFn form = if form.WasSent then validateWith validFn form else form
    let isValid form = form.ValidationErrors.IsEmpty
    let startLoading form = { form with IsLoading = true }
    let stopLoading form = { form with IsLoading = false }
    let markAsSent form = { form with WasSent = true }
    let init v = { FormData = v; ValidationErrors = []; IsLoading = false; WasSent = false }
