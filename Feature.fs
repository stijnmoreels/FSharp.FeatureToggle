/// A small utility module that provides a foundation for dynamically enabling and disabling features.
module Feature

/// A simple toggle for selectively enabling or disabling functionality.
type Toggle = Enabled | Disabled

/// Feature type for defining a value/function which can be toggled.
type Feature<'a> = Feature of (Toggle -> 'a)

/// Lifts a value into the Feature type.
/// 'a -> Feature<'a>
let retn x = Feature <| fun _ -> x

/// Ask for a toggle value.
/// (Toggle -> 'a) -> Feature<'a>
let private ask = Feature

/// Run the Feature for a given Toggle.
/// Feature<'a> -> Toggle -> 'a
let run t (Feature f) = f t

/// Applies a given function to a Feature.
/// Feature<('a -> 'b)> -> Feature<'a> -> Feature<'b>
let apply fT xT = ask <| fun t ->
    let f = run t fT
    let x = run t xT
    f x

let (<*>) = apply

/// Combines two Feature values using a specified function.
/// ('a -> 'b -> 'c) -> Feature<'a> -> Feature<'b> -> Feature<'c>
let liftA f x y = retn f <*> x <*> y

/// Combines three Feature values using a specified function.
/// ('a -> 'b -> 'c -> 'd) -> Feature<'a> -> Feature<'b> -> Feature<'c> -> Feature<'d>
let liftA3 f x y z = retn f <*> x <*> y <*> z

/// Allows to compose cross-world Feature functions.
/// ('a -> Feature<'b>) -> Feature<'a> -> Feature<'b>
let bind f xT = ask <| fun t ->
    let x = run t xT
    run t (f x)

/// Lifts a function into a Feature function.
/// ('a -> 'b) -> (Feature<'a> -> Feature<'b>)
let map f = bind (f >> retn)

let bool = function
    | true -> Enabled
    | false -> Disabled

let private withToggle t x y =
    match t with
    | Enabled -> x
    | Disabled -> y

/// Flip a toggle from enabled to disabled or vice versa.
let toggle t = withToggle t Disabled Enabled

/// Switch on values depending on whether a toggle is enabled or disabled.
/// 'a -> 'a -> Feature<'a>
let toggled x y = ask <| fun t -> withToggle t x y
let (<|>) = toggled

/// Execute an action only when the specified feature is enabled.
/// ('a -> 'a) -> Feature<('a -> 'a)>
let enabled f = ask <| fun t -> withToggle t f id

/// Execute an action only when the specified feature is disabled.
/// ('a -> 'a) -> Feature<('a -> 'a)>
let disabled f = ask <| fun t -> withToggle t id f
