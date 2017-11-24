# FSharp.FeatureToggle

No, I wasn’t very found of the idea when I first heard about the concept of _Feature Toggles_. I thought it was some kind of workaround for a problem or a possible solution that can lead to an enormous Technical Debt (which I still think can happen).
I’m not going to talk about the pro’s or cons about the concept of a Feature Toggle or how it would increase your Test-Driven approach or decrease you Quality…
What I wanted to do, was to find a way in which this concept could be expressed in my functional playground (because I couldn’t find a nice example of this). 
My recent task was to implement some kind of Feature Toggle so I thought it was a good time to think about the concept.

## Toggle Alternatives

Feature Toggles can be used to toggle between different functionalities. In the previous snippet, I’ve probably seen the toggled function. This function asks for a Toggle type and returns the Enabled or Disabled value associated with it.
So, let’ say we have two kind of sorting algorithms: **QuickSort** and **BubbleSort**; and we want to switch between the two:

```fsharp
let sort = Feature.toggled quicksort bubblesort
let example1 = sort |> Feature.run Enabled
```

We actually use a combinator for this:
```fsharp
let (<|>) = toggled
let example1 = quicksort <|> bubblesort |> Feature.run Enabled
```

## Toggle Logging

Feature Toggles can also be used to toggle the logging (or other “additional” features). Let’s say we want to log the input and output of a function so we can see what’s changed. I came up with this:

```fsharp
let tee f x = f x; x

let logT m = Feature.enabled (tee <| printfn "%s: %i" m)
let logBeforeT = logT "Before"
let logAfterT = logT "After"

let add1 = (+) 1
let add1T = Feature.retn add1

let (>>>) f g h = f >> g >> h
let example2 = Feature.liftA3 (>>>) logBeforeT add1T logAfterT |> Feature.run Enabled
```

We **lift** the add1 function (which is our function we want to monitor) so we can compose it together with the before and after logging Features.
For non-familiar _Functional Programmers_, the `liftA3` function will in an Applicative-manner “lift” the function (`>>>` in this example) to the “world” of Features. This way, we have a compose function in the world of Features. Here’s the implementation:

```fsharp
/// Applies a given function to a Feature.
/// Feature<('a -> 'b)> -> Feature<'a> -> Feature<'b>
let apply fT xT = ask <| fun t ->
    let f = run t fT
    let x = run t xT
    f x

let (<*>) = apply
/// Combines three Feature values using a specified function.
/// ('a -> 'b -> 'c -> 'd) -> Feature<'a> -> Feature<'b> -> Feature<'c> -> Feature<'d>
let liftA3 f x y z = retn f <*> x <*> y <*> z
```

## Toggle Memoize

Memoizing is also a term that you frequently hear in the _Functional Programming_-community. So, I thought it would be useful to also add a example of how you would toggle the _Memoization_ of a function.

```fsharp
let memoize f =
    let cache = ref Map.empty
    let add k v = cache := Map.add k v !cache
    fun x ->
        !cache
        |> Map.tryFind x
        |> Option.defaultWith
            (fun () -> f x |> tee (add x))

let dbCall () = 4815162342L

let example3 =
    memoize
    |> Feature.enabled
    |> Feature.map (fun mem -> mem dbCall)
    |> Feature.run Enabled
```

The `dbCall` function is just a function I use to memorize. We send the `unit` value with it, so it only calls this function once.
We also use the map function on a `Feature`:

```fsharp
/// Allows to compose cross-world Feature functions.
/// ('a -> Feature<'b>) -> Feature<'a> -> Feature<'b>
let bind f xT = ask <| fun t ->
    let x = run t xT
    run t (f x)

/// Lifts a function into a Feature function.
/// ('a -> 'b) -> (Feature<'a> -> Feature<'b>)
let map f = bind (f >> retn)
```
