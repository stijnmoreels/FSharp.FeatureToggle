module Properties

open FsCheck.Xunit
open Feature

[<Property>]
let ``Function runs once when feature enabled/disabled`` 
    (f : int -> int ) 
    (x : int) 
    (t : Toggle) =
    Feature.liftA (>>) (Feature.enabled f) (Feature.disabled f)
    |> Feature.run t
    |> fun g -> g x = f x

[<Property>]
let ``Toggled gets enabled or disabled value`` 
    (x : int) 
    (y : int) 
    (t : Toggle) =
    Feature.toggled x y
    |> Feature.run t
    |> fun z -> z = x || z = y

[<Property>]
let ``Mapping inner value of Feature is same as mapping input`` 
    (x : int) 
    (f : int -> int)
    (g : int -> int)
    (t : Toggle) =
    Feature.retn x
    |> Feature.map f
    |> Feature.map g
    |> Feature.run t
    |> (=) <| (f >> g) x
