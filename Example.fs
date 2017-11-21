// Sorting Example

let rec quicksort = function
    | [] -> []
    | [x] -> [x]
    | h::t -> 
        let less, greater = List.partition ((>=) h) t
        List.concat [ quicksort less; [h]; quicksort greater ]

let rec sink ls =
    match ls with
    | [] -> []
    | x :: y :: rest when x > y -> y :: (sink <| x :: rest)
    | x :: rest -> x :: (sink rest)

let rec bubblesort ls =
    let rec outer ls lsSize =
        match lsSize with
        | 0 -> ls
        | x -> outer (sink ls) (x - 1)
    outer ls (Seq.length ls)

let sorted : (int list -> int list) = quicksort <|> bubblesort |> Feature.run Enabled 

// Logging Example

let tee f x = f x; x

let logT m = Feature.enabled (tee <| printfn "%s: %i" m)
let logBeforeT = logT "Before"
let logAfterT = logT "After"

let add1 = (+) 1
let add1T = Feature.retn add1

let (>>>) f g h = f >> g >> h
let example2 = Feature.liftA3 (>>>) logBeforeT add1T logAfterT

// Memoize Example

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
    |> Feature.run Enabled <| ()
